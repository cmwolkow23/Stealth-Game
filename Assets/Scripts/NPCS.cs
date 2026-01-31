using UnityEngine;

public class VisionDetection : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Vision Settings")]
    public float viewDistance = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    public LayerMask obstructionMask; // walls, props, etc.

    [Header("Detection Settings")]
    public float detectionTimeRequired = 3f;
    public float detectionDecayRate = 1.5f;

    [Header("Debug")]
    public bool drawGizmos = true;

    private float detectionTimer;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
        }
    }

    public enum AlertState
    {
        Idle,
        Suspicious,
        Alerted
    }

    public AlertState currentState = AlertState.Idle;

    void Update()
    {
        bool canSeePlayer = CanSeePlayer();
        UpdateDetection(canSeePlayer);
        UpdateState();
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 toPlayer = player.position - transform.position;

        if (toPlayer.magnitude > viewDistance)
            return false;

        float angle = Vector3.Angle(-transform.forward, toPlayer);
        if (angle > viewAngle * 0.5f)
            return false;

        Vector3 eyePosition = transform.position + Vector3.forward * 1.6f;
        Vector3 targetPosition = player.position + Vector3.forward * 1.6f;
        Vector3 direction = (targetPosition - eyePosition).normalized;

        
        if (Physics.Raycast(eyePosition, direction, out RaycastHit hit, viewDistance))
        {
            Debug.DrawLine(eyePosition, hit.point, Color.red, 0.1f);
            return hit.transform == player;
        }

        return false;
    }

    void UpdateDetection(bool canSeePlayer)
    {
        if (canSeePlayer)
        {
            detectionTimer += Time.deltaTime;
        }
        else
        {
            detectionTimer -= Time.deltaTime * detectionDecayRate;
        }

        detectionTimer = Mathf.Clamp(detectionTimer, 0f, detectionTimeRequired);
        Debug.Log($"{name} Detection Timer: {detectionTimer},canSeePlayer: {canSeePlayer}");
    }

    void UpdateState()
    {
        if (detectionTimer >= detectionTimeRequired)
        {
            if (currentState != AlertState.Alerted)
                OnAlerted();

            currentState = AlertState.Alerted;
        }
        else if (detectionTimer > 0f)
        {
            if (currentState != AlertState.Suspicious)
                OnSuspicious();

            currentState = AlertState.Suspicious;
        }
        else
        {
            if (currentState != AlertState.Idle)
                OnIdle();

            currentState = AlertState.Idle;
        }
    }

    void OnIdle()
    {
        // Patrol normally
        // Animator.SetBool("Alerted", false);
        Debug.Log($"{name}: Idle");
    }

    void OnSuspicious()
    {
        // Look at player, slow patrol, play voice line
        Debug.Log($"{name}: Suspicious...");
    }

    void OnAlerted()
    {
        // ALERT! Chase player, raise alarm, etc.   
        Debug.Log($"{name}: ALERTED!");
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 3, 0) * -transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 3, 0) * -transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.forward * viewDistance);
    }
}

