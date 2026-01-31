using UnityEngine;
using UnityEngine.AI;

public class EnemyWander : MonoBehaviour
{
    public float wanderRadius = 2.5f;   
    public float wanderDelay = 2f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        timer = wanderDelay;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderDelay && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPos = GetRandomPoint();
            agent.SetDestination(newPos);
            timer = 0f;
        }
    }

    Vector3 GetRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);

        return hit.position;
    }
}

