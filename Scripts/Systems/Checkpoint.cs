using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Progress")]
    public ProgressBar progressBar;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip checkpointSoundEffect;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered ||
            !collision.CompareTag("Player"))
        {
            return;
        }

        Movement player =
            collision.GetComponent<Movement>();

        if (player == null)
            return;

        ActivateCheckpoint(player);
    }

    private void ActivateCheckpoint(Movement player)
    {
        hasTriggered = true;

        player.currentSpawnPoint =
            transform.position;

        if (progressBar != null)
        {
            progressBar.currentFill += 0.25f;
        }

        if (audioSource != null &&
            checkpointSoundEffect != null)
        {
            audioSource.PlayOneShot(
                checkpointSoundEffect
            );
        }
    }
}
