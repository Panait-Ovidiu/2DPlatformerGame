using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody2D;

    private string currentAnimation;
    private Animator animator;
    private enum MoveDirection { left, right, stop }
    private int moveDirection;

    [SerializeField] Transform projectiles;
    [SerializeField] GameObject projectilePrefab;

    [SerializeField] WorldGeneration worldGeneration;
    [SerializeField] float initialPosOffsetY = 5f;
    [SerializeField] private LayerMask jumpableGround;
    [SerializeField] private Transform player;
    private bool isStartPosSet;
    private bool isGrounded;

    private float xAxis;
    private float yAxis;

    [SerializeField] bool isRanged = false;
    [SerializeField] float aggressionRange = 5f;
    [SerializeField] float patrolRange = 5f;
    private float patrolStartTime;
    private float patrolStartX;
    private bool isPatroling = false;
    private bool isChasing = true;

    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float jumpForce = 450;
    private bool isJumpPressed;
    [SerializeField] private float attackRateSec = 0.75f;
    private float nextAttackTime = 0.0f;
    private bool isAttackPressed;
    private bool isAttaking;
    private bool isHit;
    private bool isDead;

    [SerializeField] string enemyAnimationName;
    private string ENEMY_IDLE;
    private string ENEMY_WALK;
    private string ENEMY_RUN;
    private string ENEMY_JUMP;
    private string ENEMY_FALL;
    private string ENEMY_ATTACK;
    private string ENEMY_HIT;
    private string ENEMY_DIE;

    public void Instantiate(Transform projectiles, WorldGeneration worldGeneration, Transform player)
    {
        this.projectiles = projectiles;
        this.worldGeneration = worldGeneration;
        this.player = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        ENEMY_IDLE = enemyAnimationName + "_Idle";
        ENEMY_WALK = enemyAnimationName + "_Walk";
        ENEMY_RUN = enemyAnimationName + "_Run";
        ENEMY_JUMP = enemyAnimationName + "_Jump";
        ENEMY_FALL = enemyAnimationName + "_Fall";
        ENEMY_ATTACK = enemyAnimationName + "_Attack";
        ENEMY_HIT = enemyAnimationName + "_Hit";
        ENEMY_DIE = enemyAnimationName + "_Die";
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
            UpdateMovement();
        }
    }

    private void UpdateMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < aggressionRange && distanceToPlayer > 2) // In Aggression Range - Start Chasing Player
        {
            if (isRanged)
            {
                isAttackPressed = true;
            }
            ChasePlayer();
        }
        else if (distanceToPlayer < aggressionRange && distanceToPlayer < 2) // Near Player - Start Attack
        {
            MoveStop();
            isAttackPressed = true;
        }
        else if (distanceToPlayer > aggressionRange && isChasing) // Out of Aggression Range - Stop Chasing
        {
            MoveStop();
            Patrol();
        }
        else // Start Patrol
        {
            Patrol();
        }
    }

    private void ChasePlayer()
    {
        if (isPatroling)
        {
            isPatroling = false;
            isChasing = true;
        }

        float enemyPosX = transform.position.x;
        float playerPosX = player.position.x;

        float currentTerrainHeight = worldGeneration.GetTerrainHeight((int)enemyPosX);
        float terrainHeightLeft = worldGeneration.GetTerrainHeight((int)enemyPosX - 1);
        float terrainHeightRight = worldGeneration.GetTerrainHeight((int)enemyPosX + 1);

        if (enemyPosX < playerPosX) // Enemy to left side, move right
        {
            MoveRight();

            if (currentTerrainHeight < terrainHeightRight && isGrounded)
            {
                isJumpPressed = true;
            }

        }
        else if (enemyPosX > playerPosX)  // Enemy to right side, move left
        {
            MoveLeft();

            if (currentTerrainHeight < terrainHeightLeft && isGrounded)
            {
                isJumpPressed = true;
            }
        }
    }

    private void Patrol()
    {
        if (isChasing)
        {
            isChasing = false;
            isPatroling = true;
            patrolStartTime = Time.time;
            patrolStartX = transform.position.x;
            MoveLeft();

            if (patrolStartX + patrolRange > worldGeneration.GetWidth() - 1)
            {
                patrolStartX -= patrolRange;
            }
            else if (patrolStartX - patrolRange < 1)
            {
                patrolStartX += patrolRange;
            }
        }

        float enemyPosX = transform.position.x;

        float currentTerrainHeight = worldGeneration.GetTerrainHeight((int)enemyPosX);
        float terrainHeightLeft = worldGeneration.GetTerrainHeight((int)enemyPosX - 1);
        float terrainHeightRight = worldGeneration.GetTerrainHeight((int)enemyPosX + 1);

        if (enemyPosX < patrolStartX + patrolRange && transform.localScale.x == 1) // Enemy to left side, move right
        {
            MoveRight();

            if (currentTerrainHeight < terrainHeightRight && isGrounded)
            {
                isJumpPressed = true;
            }
        }
        else
        {
            MoveLeft();
        }

        if (enemyPosX > patrolStartX - patrolRange && transform.localScale.x == -1)  // Enemy to right side, move left
        {
            MoveLeft();

            if (currentTerrainHeight < terrainHeightLeft && isGrounded)
            {
                isJumpPressed = true;
            }
        }
        else
        {
            MoveRight();
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            //Check if enemy is on the ground
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

            if (moveDirection == (int)MoveDirection.left && !isHit && !isAttaking)
            {
                if (isPatroling)
                {
                    velocity.x = -walkSpeed;
                }
                else if (isChasing)
                {
                    velocity.x = -runSpeed;
                }
                //  transform.localScale = new Vector2(-1, 1);
            }
            else if (moveDirection == (int)MoveDirection.right && !isHit && !isAttaking)
            {
                if (isPatroling)
                {
                    velocity.x = walkSpeed;
                }
                else if (isChasing)
                {
                    velocity.x = runSpeed;
                }
                //  transform.localScale = new Vector2(1, 1);
            }
            else
            {
                velocity.x = 0;
                // SwitchToWalking();
            }

            if (isGrounded && !isHit && !isAttaking)
            {
                if (xAxis != 0)
                {
                    if (isPatroling)
                    {
                        ChangeAnimationState(ENEMY_WALK);
                    }
                    else if (isChasing)
                    {
                        ChangeAnimationState(ENEMY_RUN);
                    }
                }
                else
                {
                    ChangeAnimationState(ENEMY_IDLE);
                }
            }

            //Check if trying to jump 
            if (isJumpPressed && isGrounded && !isHit && !isAttaking)
            {
                isJumpPressed = false;
                ChangeAnimationState(ENEMY_JUMP);
                rigidBody2D.AddForce(new Vector2(0, jumpForce));
            }
            else if (rigidBody2D.velocity.y < -.1f)
            {
                if (!isGrounded)
                {
                    ChangeAnimationState(ENEMY_FALL);
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
                    if (Time.time > nextAttackTime)
                    {
                        nextAttackTime = Time.time + attackRateSec;
                        isAttaking = true;

                        if (isGrounded)
                        {
                            ChangeAnimationState(ENEMY_ATTACK);
                        }

                        float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                        Invoke("AttackComplete", delay);
                    }
                }
            }
        }
    }

    private void MoveLeft()
    {
        xAxis = -1f;
        transform.localScale = new Vector2(-1, 1);
        moveDirection = (int)MoveDirection.left;
    }

    private void MoveRight()
    {
        xAxis = 1f;
        transform.localScale = new Vector2(1, 1);
        moveDirection = (int)MoveDirection.right;
    }

    private void MoveStop()
    {
        xAxis = 0f;
        moveDirection = (int)MoveDirection.stop;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Die();
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
                ChangeAnimationState(ENEMY_HIT);
                float delay = animator.GetCurrentAnimatorStateInfo(0).length;
                Invoke("HitComplete", delay);
                Destroy(collision.gameObject);
            }
        }

        if (collision.gameObject.CompareTag("Trap"))
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rigidBody2D.bodyType = RigidbodyType2D.Static;
        ChangeAnimationState(ENEMY_DIE);
        Destroy(boxCollider);
        worldGeneration.GenerateCollectibles(transform.position);
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

    private void SwitchToRunning()
    {
        isPatroling = true;
        isChasing = false;
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
