using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianController : MonoBehaviour
{
    [SerializeField] private bool agentActive = true;
    [SerializeField] private float walkPointRange;
    [SerializeField] private Vector3 walkPoint;

    private NavMeshAgent agent;
    private Vector3 destination;
    private bool walkPointSet;
    private bool reachedDestination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!agentActive || agent.enabled == false) return;

        if (walkPointSet) CheckPathComplete();
        Roam();
    }

    void Roam()
    {
        if (!agentActive || agent.enabled == false) return;

        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet) agent.SetDestination(walkPoint);
    }

    void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        int sidewalkArea = 4;
        int grassArea = 5;
        int crossingArea = 6;
        int sidewalkAreaMask = 1 << sidewalkArea;
        int grassAreaMask = 1 << grassArea;
        int crossingAreaMask = 1 << crossingArea;
        int finalMask = sidewalkAreaMask | grassAreaMask | crossingAreaMask;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(walkPoint, out hit, 25.0f, finalMask))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    void CheckPathComplete()
    {
        if (!agentActive || agent.enabled == false) return;

        if (!agent.pathPending)
        {
            if (agent.isActiveAndEnabled && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    reachedDestination = true;
                    walkPointSet = false;
                }
            }
        }
    }

    private void SetDestination(Vector3 destination)
    {
        if (!agentActive || agent.enabled == false) return;

        if (agent.isActiveAndEnabled) agent.SetDestination(destination);
        this.destination = destination;
        reachedDestination = false;
    }
}
