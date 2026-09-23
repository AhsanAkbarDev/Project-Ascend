using UnityEngine;

public class Player_Shoot : MonoBehaviour
{
    [Header("Attack")]
    public GameObject attack1Prefab;
    public Transform attack1PrefabPoint;
    public bool hasShot;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackSoundEffect;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) &&
            !hasShot)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        hasShot = true;
        animator.SetTrigger("Attack1");
    }

    // Called by an animation event when
    // the projectile should be released.
    public void Attack1()
    {
        if (audioSource != null &&
            attackSoundEffect != null)
        {
            audioSource.PlayOneShot(
                attackSoundEffect
            );
        }

        GameObject projectile =
            Instantiate(
                attack1Prefab,
                attack1PrefabPoint.position,
                attack1PrefabPoint.rotation
            );

        Attack1_Prefab projectileScript =
            projectile.GetComponent<Attack1_Prefab>();

        if (projectileScript == null)
            return;

        float direction =
            spriteRenderer.flipX
                ? -1f
                : 1f;

        projectileScript.SetDirection(
            direction
        );
    }

    // Called near the end of the attack animation.
    public void AttackDelay()
    {
        Invoke(
            nameof(EndAttackCooldown),
            1f
        );
    }

    private void EndAttackCooldown()
    {
        hasShot = false;
    }
}
