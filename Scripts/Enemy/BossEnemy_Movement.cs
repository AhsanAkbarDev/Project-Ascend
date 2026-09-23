using UnityEngine;

public class BossEnemy_Movement : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public float speed;
    public float flySpeed = 20f;

    [Header("Player")]
    public Transform player;
    public Movement playerScript;

    [Header("AI Ranges")]
    public float attackRange;
    public float flyRange;

    [Header("Boss Positions")]
    public Transform idlePosition;
    public Transform flyPosition;

    [Header("Combat")]
    public float health;
    public GameObject attack1Prefab;
    public Transform attackPoint;

    private bool hasAttacked;
    private bool isDead;

    [Header("Game State")]
    public VictoryAndDefeat victory;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] bossEnemySounds;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public enum EnemyState
    {
        Idle,
        Fly,
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

        if (idlePosition != null)
        {
            transform.position =
                idlePosition.position;
        }
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

            case EnemyState.Fly:
                Fly();
                break;

            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    private void Idle()
    {
        transform.position =
            Vector2.MoveTowards(
                transform.position,
                idlePosition.position,
                flySpeed * Time.deltaTime
            );

        if (DistanceFromPlayer() <= flyRange)
        {
            animator.SetBool("IsFlying", true);
            currentState = EnemyState.Fly;
        }
        else
        {
            animator.SetBool("IsFlying", false);
        }
    }

    private void Fly()
    {
        transform.position =
            Vector2.MoveTowards(
                transform.position,
                flyPosition.position,
                flySpeed * Time.deltaTime
            );

        rb.constraints =
            RigidbodyConstraints2D.FreezePositionY;

        float distance =
            DistanceFromPlayer();

        if (distance <= attackRange &&
            playerScript.boss2AbleToAttack)
        {
            animator.SetBool("IsFlying", false);
            currentState = EnemyState.Attack;
            return;
        }

        if (distance >= flyRange)
        {
            animator.SetBool("IsFlying", false);
            currentState = EnemyState.Idle;
        }
    }

    private void Attack()
    {
        if (playerScript.isDead)
            return;

        if (DistanceFromPlayer() > attackRange)
        {
            animator.SetBool("IsFlying", true);
            currentState = EnemyState.Fly;
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
            player.position.x >
            transform.position.x;
    }

    // Called by an animation event when
    // the boss projectile should be fired.
    public void AttackAnimation()
    {
        audioSource.PlayOneShot(
            bossEnemySounds[3]
        );

        GameObject projectile =
            Instantiate(
                attack1Prefab,
                attackPoint.position,
                attackPoint.rotation
            );

        Boss_Attack_Bullet projectileScript =
            projectile.GetComponent<Boss_Attack_Bullet>();

        if (projectileScript == null)
            return;

        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)attackPoint.position)
            .normalized;

        projectileScript.SetDirection(
            direction
        );
    }

    public void AttackDelay()
    {
        Invoke(
            nameof(EndAttackCooldown),
            1.5f
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
            bossEnemySounds[1]
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
            bossEnemySounds[2]
        );
    }

    // Called at the end of the death animation.
    public void DestroyEnemy()
    {
        if (victory != null)
        {
            victory.Victory();
        }

        Destroy(gameObject);
    }

    public void FlyingSoundEffects()
    {
        audioSource.PlayOneShot(
            bossEnemySounds[0]
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
