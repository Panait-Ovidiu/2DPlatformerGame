using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerTwo : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody2D;

    private string currentAnimation;
    private Animator animator;

    [SerializeField] Transform projectiles;
    [SerializeField] GameObject projectilePrefab;

    [SerializeField] WorldGeneration worldGeneration;
    [SerializeField] float initialPosOffsetY = 5f;
    [SerializeField] private LayerMask jumpableGround;
    private bool isStartPosSet;
    private bool isGrounded;

    private float xAxis;
    private float yAxis;

    [SerializeField] private float walkSpeed = 5f;
    private bool isWalking;
    [SerializeField] private float runSpeed = 7f;
    private bool isRunning;
    [SerializeField] private float jumpForce = 450;
    private bool isJumpPressed;
    [SerializeField] private float attackRateSec = 0.75f;
    private float nextAttackTime = 0.0f;
    private bool isAttackPressed;
    private bool isAttaking;
    private bool isHit;
    private bool isDead;

    [SerializeField] private Image healthBar;
    private float maxHealth = 100f;
    private float hitDamage = 10f;
    private float playerHealth;

    [SerializeField] string playerAnimationName;
    private string PLAYER_IDLE;
    private string PLAYER_WALK;
    private string PLAYER_RUN;
    private string PLAYER_JUMP;
    private string PLAYER_FALL;
    private string PLAYER_ATTACK;
    private string PLAYER_HIT;
    private string PLAYER_DIE;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerHealth = maxHealth;

        PLAYER_IDLE = playerAnimationName + "_Idle";
        PLAYER_WALK = playerAnimationName + "_Walk";
        PLAYER_RUN = playerAnimationName + "_Run";
        PLAYER_JUMP = playerAnimationName + "_Jump";
        PLAYER_FALL = playerAnimationName + "_Fall";
        PLAYER_ATTACK = playerAnimationName + "_Attack";
        PLAYER_HIT = playerAnimationName + "_Hit";
        PLAYER_DIE = playerAnimationName + "_Die";

        SwitchToWalking();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStartPosSet)
        {
            isStartPosSet = true;
            transform.position = new Vector3(transform.position.x, worldGeneration.GetTerrainHeight((int)transform.position.x) + initialPosOffsetY, 0);
        }

        if (!isDead)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                xAxis = 1f;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                xAxis = -1f;
            }
            else
            {
                xAxis = 0f;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                isJumpPressed = true;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (Time.time > nextAttackTime)
                {
                    nextAttackTime = Time.time + attackRateSec;
                    isAttackPressed = true;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            //Check if player is on the ground
            bool hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, .1f, jumpableGround);

            //Check update movement based on input
            Vector2 velocity = new Vector2(0, rigidBody2D.velocity.y);

            if (hit)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            if (xAxis < 0 && !isHit && !isAttaking)
            {
                if (isWalking)
                {
                    velocity.x = -walkSpeed;
                }
                else if (isRunning)
                {
                    velocity.x = -runSpeed;
                }
                transform.localScale = new Vector2(-1, 1);
            }
            else if (xAxis > 0 && !isHit && !isAttaking)
            {
                if (isWalking)
                {
                    velocity.x = walkSpeed;
                }
                else if (isRunning)
                {
                    velocity.x = runSpeed;
                }
                transform.localScale = new Vector2(1, 1);
            }
            else
            {
                velocity.x = 0;
                SwitchToWalking();
            }

            if (isGrounded && !isHit && !isAttaking)
            {
                if (xAxis != 0)
                {
                    if (isWalking)
                    {
                        ChangeAnimationState(PLAYER_WALK);

                        float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                        Invoke("SwitchToRunning", delay);
                    }
                    else if (isRunning)
                    {
                        ChangeAnimationState(PLAYER_RUN);
                    }
                }
                else
                {
                    ChangeAnimationState(PLAYER_IDLE);
                }
            }

            //Check if trying to jump 
            if (isJumpPressed && isGrounded && !isHit && !isAttaking)
            {
                isJumpPressed = false;
                ChangeAnimationState(PLAYER_JUMP);
                rigidBody2D.AddForce(new Vector2(0, jumpForce));
            }
            else if (rigidBody2D.velocity.y < -.1f)
            {
                if (!isGrounded)
                {
                    ChangeAnimationState(PLAYER_FALL);
                }
            }

            //Assign the new velocity to the rigidbody
            rigidBody2D.velocity = velocity;

            //Attack
            if (isAttackPressed && xAxis == 0 && !isHit && !isAttaking)
            {
                isAttackPressed = false;

                if (!isAttaking)
                {
                    isAttaking = true;

                    if (isGrounded)
                    {
                        ChangeAnimationState(PLAYER_ATTACK);
                    }

                    float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                    Invoke("AttackComplete", delay);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            string collisonParent = collision.gameObject.GetComponent<Projectile>().GetParent();

            if (collisonParent.Equals(gameObject.name))
            {
                Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            }
            else
            {
                isHit = true;
                playerHealth -= hitDamage;
                healthBar.fillAmount = playerHealth / maxHealth;
                Destroy(collision.gameObject);
                ChangeAnimationState(PLAYER_HIT);
                float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                Invoke("HitComplete", delay);

                if (playerHealth <= 0)
                {
                    isDead = true;
                    rigidBody2D.bodyType = RigidbodyType2D.Static;
                    Destroy(boxCollider);
                    ChangeAnimationState(PLAYER_DIE);
                }
            }

        }

        if (collision.gameObject.CompareTag("Trap"))
        {
            isDead = true;
            playerHealth = 0;
            healthBar.fillAmount = playerHealth / maxHealth;
            rigidBody2D.bodyType = RigidbodyType2D.Static;
            Destroy(boxCollider);
            ChangeAnimationState(PLAYER_DIE);
        }
    }

    public void GenerateProjectile()
    {
        Vector3 position = Vector3.zero;

        if (transform.localScale.x == -1)
        {
            position = new Vector3(transform.position.x - 0.5f, transform.position.y, 0);
            GameObject newProjectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            newProjectile.GetComponent<SpriteRenderer>().flipX = true;
            newProjectile.GetComponent<Projectile>().SetParent(gameObject.name);
            newProjectile.GetComponent<Projectile>().SetDirection(true);
            newProjectile.transform.SetParent(projectiles);
        }
        else
        {
            position = new Vector3(transform.position.x + 0.5f, transform.position.y, 0);
            GameObject newProjectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            newProjectile.GetComponent<SpriteRenderer>().flipX = false;
            newProjectile.GetComponent<Projectile>().SetParent(gameObject.name);
            newProjectile.GetComponent<Projectile>().SetDirection(false);
            newProjectile.transform.SetParent(projectiles);
        }
    }

    private void SwitchToWalking()
    {
        isRunning = false;
        isWalking = true;
    }

    private void SwitchToRunning()
    {
        isRunning = true;
        isWalking = false;
    }

    private void AttackComplete()
    {
        isAttaking = false;
    }

    private void HitComplete()
    {
        isHit = false;
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    private void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation)
        {
            return;
        }

        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
