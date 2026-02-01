using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class enemyHealthSystem : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    private int currentHealth;
    [SerializeField]
    private Image healthBar;

    // Cached full width of the health image's RectTransform (set at Start)
    private float originalBarWidth;

    // Death sound settings
    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;
    [SerializeField] private bool destroyIfNoClip = true;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            // Cache the initial width so we can scale it by health percent.
            // Note: if you use LayoutGroups the rect width may be zero at Start;
            // call LayoutRebuilder.ForceRebuildLayoutImmediate(...) if needed.
            originalBarWidth = healthBar.rectTransform.rect.width;
            UpdateHealthBar();
        }
    }

    public void OnHit(int Damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - Damage, 0, maxHealth);

        if (healthBar != null)
            UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    private void UpdateHealthBar()
    {
        float pct = (float)currentHealth / maxHealth;
        float newWidth = originalBarWidth * pct;

        // Recommended API to change RectTransform width at runtime:
        healthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    private void Death()
    {
        // Handle enemy death (e.g., play animation, drop loot, etc.)
        Debug.Log("Enemy has died.");

        // Start coroutine that plays death sound (if any) and waits before destroying the object
        StartCoroutine(PlayDeathAndDestroy());
    }

    private IEnumerator PlayDeathAndDestroy()
    {
        // Get or add an AudioSource to play the clip
        AudioSource source = GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        if (deathClip != null)
        {
            source.PlayOneShot(deathClip, deathVolume);

            // Wait for clip length adjusted by pitch (guard pitch != 0)
            float waitTime = deathClip.length;
            float pitch = Mathf.Approximately(source.pitch, 0f) ? 1f : Mathf.Abs(source.pitch);
            waitTime /= pitch;

            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            if (!destroyIfNoClip)
            {
                // If configured to not destroy when no clip is assigned, just exit
                yield break;
            }
            // otherwise proceed to destroy immediately
        }

        Destroy(gameObject);
    }
}

