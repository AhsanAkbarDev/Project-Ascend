using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public float speed;

    [Header("Player")]
    public Transform player;
    public Movement playerScript;

    [Header("AI Ranges")]
    public float patrolRange;
    public float chaseRange;
    public float attackRange;

    [Header("Patrol")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public Transform currentPatrolPoint;

    [Header("Combat")]
    public float health;
    public GameObject attack1Prefab;
    public Transform attackPoint;

    private bool hasAttacked;
    private bool isDead;

    [Header("Level Progression")]
    public GameObject level2BouncyPlatform;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] enemySounds;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    private EnemyState currentState =
        EnemyState.Idle;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerScript =
                playerObject.GetComponent<Movement>();
        }

        currentPatrolPoint = patrolPointB;
        spriteRenderer.flipX = true;
    }

    private void Update()
    {
        if (isDead ||
            player == null ||
            playerScript == null)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;

            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    private void Idle()
    {
        float distance =
            DistanceFromPlayer();

        if (distance <= patrolRange)
        {
            animator.SetBool(
                "IsPatrolling",
                true
            );

            currentState =
                EnemyState.Patrol;
        }
    }

    private void Patrol()
    {
        CheckIfPointHasReached();

        float distance =
            DistanceFromPlayer();

        if (distance <= chaseRange)
        {
            animator.SetBool(
                "IsPatrolling",
                false
            );

            animator.SetBool(
                "IsChasing",
                true
            );

            currentState =
                EnemyState.Chase;

            return;
        }

        if (distance >= patrolRange)
        {
            animator.SetBool(
                "IsPatrolling",
                false
            );

            animator.SetBool(
                "IsChasing",
                false
            );

            currentState =
                EnemyState.Idle;

            return;
        }

        transform.position =
            Vector2.MoveTowards(
                transform.position,
                currentPatrolPoint.position,
                speed * Time.deltaTime
            );

        spriteRenderer.flipX =
            currentPatrolPoint ==
            patrolPointB;
    }

    private void CheckIfPointHasReached()
    {
        if (currentPatrolPoint == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                currentPatrolPoint.position
            );

        if (distance > 0.05f)
            return;

        currentPatrolPoint =
            currentPatrolPoint == patrolPointB
                ? patrolPointA
                : patrolPointB;
    }

    private void Chase()
    {
        float distance =
            DistanceFromPlayer();

        if (distance <= attackRange &&
            playerScript.boss1AbleToAttack)
        {
            animator.SetBool(
                "IsChasing",
                false
            );

            currentState =
                EnemyState.Attack;

            return;
        }

        if (distance > chaseRange)
        {
            animator.SetBool(
                "IsChasing",
                false
            );

            animator.SetBool(
                "IsPatrolling",
                true
            );

            currentState =
                EnemyState.Patrol;

            return;
        }

        transform.position =
            Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );
    }

    private void Attack()
    {
        if (playerScript.isDead)
            return;

        float distance =
            DistanceFromPlayer();

        if (distance > attackRange)
        {
            animator.SetBool(
                "IsChasing",
                true
            );

            currentState =
                EnemyState.Chase;

            return;
        }

        if (hasAttacked)
            return;

        FacePlayer();

        animator.SetTrigger("Attack");

        hasAttacked = true;
    }

    private void FacePlayer()
    {
        spriteRenderer.flipX =
            player.position.x <
            transform.position.x;
    }

    // Called by an animation event when
    // the enemy projectile should be fired.
    public void AttackAnimation()
    {
        audioSource.PlayOneShot(
            enemySounds[3]
        );

        GameObject projectile =
            Instantiate(
                attack1Prefab,
                attackPoint.position,
                attackPoint.rotation
            );

        Enemy_Attack1_Prefab projectileScript =
            projectile.GetComponent<
                Enemy_Attack1_Prefab
            >();

        if (projectileScript == null)
            return;

        float direction =
            spriteRenderer.flipX
                ? 1f
                : -1f;

        projectileScript.SetDirection(
            direction
        );
    }

    public void AttackDelay()
    {
        Invoke(
            nameof(EndAttackCooldown),
            3f
        );
    }

    private void EndAttackCooldown()
    {
        hasAttacked = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        health -= damage;

        health =
            Mathf.Clamp(
                health,
                0,
                100
            );

        if (health <= 0)
        {
            Die();
            return;
        }

        animator.SetTrigger("IsHurt");

        audioSource.PlayOneShot(
            enemySounds[0]
        );
    }

    private void Die()
    {
        isDead = true;

        animator.SetBool(
            "IsDead",
            true
        );

        audioSource.PlayOneShot(
            enemySounds[1]
        );
    }

    // Called at the end of the death animation.
    public void DestroyEnemy()
    {
        if (level2BouncyPlatform != null)
        {
            level2BouncyPlatform.SetActive(
                true
            );
        }

        Destroy(gameObject);
    }

    public void WalkingSoundEffect()
    {
        audioSource.PlayOneShot(
            enemySounds[2]
        );
    }

    private float DistanceFromPlayer()
    {
        return Vector2.Distance(
            transform.position,
            player.position
        );
    }
}
