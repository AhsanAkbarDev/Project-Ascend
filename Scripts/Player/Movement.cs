using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public float speed;
    public float jumpForce;

    [SerializeField] private bool isJumping;
    public bool isGrounded;
    public bool isKnocked;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize =
        new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Physics")]
    public float gravityScale;
    public float fallGravityScale;

    [Header("Health")]
    public float healthAmount;
    public float maxHealth;
    public bool isDead;

    [Header("Progression")]
    public int coinCounter;
    public Vector2 currentSpawnPoint;

    [Header("Game State")]
    public VictoryAndDefeat defeat;
    public bool boss1AbleToAttack;
    public bool boss2AbleToAttack;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] playerSounds;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        maxHealth = healthAmount;
        currentSpawnPoint = transform.position;
    }

    private void Update()
    {
        if (isDead)
            return;

        CheckJumpInput();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isKnocked)
            return;

        ApplyJumpGravity();
        CheckGrounded();
        Move();
        Jump();
    }

    private void Move()
    {
        float horizontal =
            Input.GetAxis("Horizontal");

        animator.SetFloat(
            "Speed",
            Mathf.Abs(horizontal)
        );

        Vector2 targetVelocity =
            new Vector2(
                horizontal * speed,
                rb.linearVelocity.y
            );

        Vector2 smoothMovement =
            Vector2.Lerp(
                rb.linearVelocity,
                targetVelocity,
                speed * Time.fixedDeltaTime
            );

        smoothMovement.y =
            rb.linearVelocity.y;

        rb.linearVelocity =
            smoothMovement;

        if (horizontal > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontal < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void CheckJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            isGrounded)
        {
            isJumping = true;
        }
    }

    private void Jump()
    {
        if (!isJumping)
            return;

        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );

        isJumping = false;
    }

    private void ApplyJumpGravity()
    {
        if (rb.linearVelocity.y > 0f)
        {
            rb.gravityScale = gravityScale;
        }
        else
        {
            rb.gravityScale = fallGravityScale;
        }
    }

    private void CheckGrounded()
    {
        isGrounded =
            Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer
            );

        animator.SetBool(
            "IsGrounded",
            isGrounded
        );
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        healthAmount -= damage;

        healthAmount =
            Mathf.Clamp(
                healthAmount,
                0,
                maxHealth
            );

        if (healthAmount <= 0)
        {
            Death(playerSounds[1]);
            return;
        }

        audioSource.PlayOneShot(
            playerSounds[0]
        );

        animator.SetTrigger("IsHurt");
    }

    public void Heal(float health)
    {
        if (healthAmount >= maxHealth)
            return;

        healthAmount += health;

        healthAmount =
            Mathf.Clamp(
                healthAmount,
                0,
                maxHealth
            );

        audioSource.PlayOneShot(
            playerSounds[2]
        );
    }

    public void Death(AudioClip clip)
    {
        isDead = true;

        rb.linearVelocity =
            Vector2.zero;

        animator.SetBool(
            "IsDead",
            true
        );

        audioSource.PlayOneShot(clip);
    }

    // Called when the death animation finishes.
    public void DeathAnimFinished()
    {
        defeat.Defeat();
    }

    public void Respawn()
    {
        animator.SetBool(
            "IsDead",
            false
        );

        healthAmount = maxHealth;
        coinCounter = 0;

        rb.linearVelocity =
            Vector2.zero;

        transform.position =
            currentSpawnPoint;

        isDead = false;
    }

    public void KnockBack(
        Vector2 direction,
        float force)
    {
        isKnocked = true;

        rb.linearVelocity =
            Vector2.zero;

        rb.AddForce(
            direction * force,
            ForceMode2D.Impulse
        );

        Invoke(
            nameof(EndOfKnockBack),
            0.3f
        );
    }

    private void EndOfKnockBack()
    {
        isKnocked = false;
    }

    // Called by a walking animation event.
    public void WalkingSound()
    {
        audioSource.PlayOneShot(
            playerSounds[3]
        );
    }
}
