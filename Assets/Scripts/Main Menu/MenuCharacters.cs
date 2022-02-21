using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCharacters : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody2D;

    private string currentAnimation;
    private Animator animator;

    [SerializeField] Transform projectiles;
    [SerializeField] GameObject projectilePrefab;

    [SerializeField] private LayerMask jumpableGround;

    private float xAxis;

    [SerializeField] bool isRanged = false;
    [SerializeField] private float jumpForce = 450;
    private bool isJumpPressed;

    [SerializeField] private float attackRateSec = 0.75f;
    private float nextAttackTime = 0.0f;
    private bool isAttackPressed;
    private bool isAttaking;
    private bool isGrounded;

    [SerializeField] string characterAnimationName;
    private string CHARACTER_IDLE;
    private string CHARACTER_JUMP;
    private string CHARACTER_FALL;
    private string CHARACTER_ATTACK;

    [SerializeField] bool isArcher;


    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        CHARACTER_IDLE = characterAnimationName + "_Idle";
        CHARACTER_JUMP = characterAnimationName + "_Jump";
        CHARACTER_FALL = characterAnimationName + "_Fall";
        CHARACTER_ATTACK = characterAnimationName + "_Attack";
    }

    // Update is called once per frame
    void Update()
    {
        if (!isJumpPressed)
        {
            if (isArcher)
            {
                isAttackPressed = true;
            }
        }
    }

    private void FixedUpdate()
    {
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

        if (isGrounded && !isAttaking)
        {

            ChangeAnimationState(CHARACTER_IDLE);

        }

        if (isJumpPressed && isGrounded && !isAttaking)
        {
            isJumpPressed = false;
            ChangeAnimationState(CHARACTER_JUMP);
            rigidBody2D.AddForce(new Vector2(0, jumpForce));
        }
        else if (rigidBody2D.velocity.y < -.1f)
        {
            if (!isGrounded)
            {
                ChangeAnimationState(CHARACTER_FALL);
            }
        }

        if (isAttackPressed && xAxis == 0 && isGrounded && !isAttaking)
        {
            isAttackPressed = false;

            if (!isAttaking)
            {
                if (Time.time > nextAttackTime)
                {
                    nextAttackTime = Time.time + attackRateSec;
                    isAttaking = true;

                    if (isGrounded)
                    {
                        ChangeAnimationState(CHARACTER_ATTACK);
                    }

                    float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                    Invoke("AttackComplete", delay);
                }
            }
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
            if (!isRanged)
            {
                newProjectile.GetComponent<Projectile>().SetStatic();
            }
            newProjectile.transform.SetParent(projectiles);
        }
        else
        {
            position = new Vector3(transform.position.x + 0.5f, transform.position.y, 0);
            GameObject newProjectile = Instantiate(projectilePrefab, position, Quaternion.identity);
            newProjectile.GetComponent<SpriteRenderer>().flipX = false;
            newProjectile.GetComponent<Projectile>().SetParent(gameObject.name);
            newProjectile.GetComponent<Projectile>().SetDirection(false);
            if (!isRanged)
            {
                newProjectile.GetComponent<Projectile>().SetStatic();
            }
            newProjectile.transform.SetParent(projectiles);
        }
    }

    public void Jump()
    {
        isJumpPressed = true;
    }

    void AttackComplete()
    {
        isAttaking = false;
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation)
        {
            return;
        }

        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
