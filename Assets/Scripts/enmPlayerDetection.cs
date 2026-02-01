using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enmPlayerDetection : MonoBehaviour
{
    [SerializeField]
    private BoxCollider detectionCollider;
    [SerializeField]
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private float detectionInterval = 1f;
    private float detectionTimer;
    private bool detectionInProgress = false;
    private bool playerDetected = false;
    [SerializeField]
    private AudioSource detectionAudioSource;
    [SerializeField]
    private AudioClip detectionClip;
    [SerializeField]
    private Image detectBar;
    private float originalBarWidth;
    [SerializeField]
    private float enmSpeed = 3.5f;

    private void Start()
    {
        if (detectBar != null)
        {
            // Cache the initial width so we can scale it by health percent.
            // Note: if you use LayoutGroups the rect width may be zero at Start;
            // call LayoutRebuilder.ForceRebuildLayoutImmediate(...) if needed.
            originalBarWidth = detectBar.rectTransform.rect.width;
            UpdateHealthBar();
        }
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnEnable()
    {
        FPSController.OnPlayerDeath += onPlayerDeath;
    }
    private void OnDisable()
    {
        FPSController.OnPlayerDeath -= onPlayerDeath;
    }

    private void Update()
    {
        Debug.Log("Detection Timer: " + detectionTimer);
        Debug.Log(detectionInProgress);
        if (detectionInProgress && !playerDetected)
        {
            detectionTimer += Time.deltaTime;
            detectionTimer = Mathf.Clamp(detectionTimer, 0f, detectionInterval);
        }
        else
        {
            Debug.Log("Decreasing Detection Timer");
            detectionTimer -= Time.deltaTime;
            detectionTimer = Mathf.Clamp(detectionTimer, 0f, detectionInterval);
            if (!playerDetected)
                UpdateHealthBar();
        }
        if (detectionTimer >= detectionInterval)
        {
            if (!playerDetected)
            {
                detectionAudioSource.PlayOneShot(detectionClip);
            }
            playerDetected = true;
            detectionTimer = 0f;
        }
        if (playerDetected)
        {
            if (TryGetComponent<enemyHealthSystem>(out enemyHealthSystem enemyHealth))
            {
                if (enemyHealth.currentHealth <= 0)
                    return;
            }
            navMeshAgent.SetDestination(player.transform.position);
            navMeshAgent.speed = enmSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            detectionInProgress = true;
            //detectionTimer += Time.deltaTime;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player)
        {
            if(!detectionInProgress)
                detectionInProgress = true;
            //detectionTimer += Time.deltaTime;
            if(!playerDetected)
                UpdateHealthBar();
            
        }
        else
        {
            //detectionTimer = Mathf.Max(detectionTimer - Time.deltaTime, 0f);
            if(!playerDetected)
                UpdateHealthBar();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            detectionInProgress = false;
        }
    }
    private void UpdateHealthBar()
    {
        float pct = detectionTimer / detectionInterval;
        float newWidth = originalBarWidth * pct;

        // Recommended API to change RectTransform width at runtime:
        detectBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    private void onPlayerDeath()
    {
        if(TryGetComponent<NavMeshAgent>(out NavMeshAgent navMeshAgent))
        {
            //navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

    }
}
