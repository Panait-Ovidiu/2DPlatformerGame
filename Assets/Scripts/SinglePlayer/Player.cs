using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
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

    const string PLAYER_IDLE = "Player_Idle";
    const string PLAYER_WALK = "Player_Walk";
    const string PLAYER_RUN = "Player_Run";
    const string PLAYER_JUMP = "Player_Jump";
    const string PLAYER_FALL = "Player_Fall";
    const string PLAYER_ATTACK = "Player_Attack";
    const string PLAYER_HIT = "Player_Hit";
    const string PLAYER_DIE = "Player_Die";

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        playerHealth = maxHealth;
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
            xAxis = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isJumpPressed = true;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (Time.time > nextAttackTime)
                {
                    nextAttackTime = Time.time + attackRateSec;
                    isAttackPressed = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                worldGeneration.GenerateNewLevel();
                worldGeneration.Generation();
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
                        GenerateProjectile();
                    }

                    float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                    Invoke("AttackComplete", delay);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            isAttaking = true;

            ChangeAnimationState(PLAYER_ATTACK);
            GenerateProjectile();

            float delay = animator.GetCurrentAnimatorStateInfo(0).length;
            Invoke("AttackComplete", delay);
        }

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

                if(playerHealth <= 0)
                {
                    isDead = true;
                    transform.position = new Vector3(-1, 35.5f);
                    rigidBody2D.bodyType = RigidbodyType2D.Static;
                    ChangeAnimationState(PLAYER_DIE);
                    FindObjectOfType<MenuSinglePlayer>().ShowGameOver();
                }
            }

        }

        if (collision.gameObject.CompareTag("Trap"))
        {
            isDead = true;
            playerHealth = 0;
            healthBar.fillAmount = playerHealth / maxHealth;
            transform.position = new Vector3(-1, 35.5f);
            rigidBody2D.bodyType = RigidbodyType2D.Static;
            ChangeAnimationState(PLAYER_DIE);
            FindObjectOfType<MenuSinglePlayer>().ShowGameOver();
        }
    }

    public void GenerateProjectile()
    {
        Vector3 position = Vector3.zero;

        if (transform.localScale.x == -1)
        {
            position = new Vector3(transform.position.x - 0.5f, transform.position.y, 0);
            GameObject newProjectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            newProjectile.transform.localScale = new Vector2(1, 1);
            newProjectile.GetComponent<Projectile>().SetParent(gameObject.name);
            newProjectile.GetComponent<Projectile>().SetStatic();
            newProjectile.transform.SetParent(projectiles);
        }
        else
        {
            position = new Vector3(transform.position.x + 0.5f, transform.position.y, 0);
            GameObject newProjectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            newProjectile.transform.localScale = new Vector2(-1, 1);
            newProjectile.GetComponent<Projectile>().SetParent(gameObject.name);
            newProjectile.GetComponent<Projectile>().SetStatic();
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
