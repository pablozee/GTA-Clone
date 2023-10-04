using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PedestrianController : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walk,
        AttackOrFlee,
        Flee,
        Attack
    }

    [SerializeField] private float health = 100f;
    [SerializeField] private bool agentActive = true;
    [SerializeField] private bool forceIdle = false;
    [SerializeField] private float walkPointRange;
    [SerializeField] private Vector3 walkPoint;

    [SerializeField] private bool isDangerClose;
    [SerializeField] private bool isAggressive;

    public State currentState;
    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 destination;
    private bool walkPointSet;
    private bool reachedDestination;
    private float idleEndTime;


    private void TransitionToState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Idle:
                idleEndTime = Time.time + Random.Range(3, 9);
                break;

            case State.Walk:
                if (!walkPointSet) SearchWalkPoint();
                if (walkPointSet) SetDestination(walkPoint);

                break;

            case State.Flee:
                // Logic to flee, maybe setting a destination opposite to the danger source
                break;

            case State.Attack:
                // Initialize attack, maybe lock onto the target, play an animation, etc.
                break;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        TransitionToState(State.Walk);
    }

    // Update is called once per frame
    void Update()
    {
        if (!agentActive || agent.enabled == false) return;

        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;

            case State.Walk:
                HandleWalkState();
                break;

            case State.AttackOrFlee:
                HandleAttackOrFleeState();
                break;

            case State.Attack:
                HandleAttackState();
                break;

            case State.Flee:
                HandleFleeState();
                break;
        }

    //    if (walkPointSet) CheckPathComplete();
    //    MovementAnimations();
    //    Roam();
    }

    void HandleIdleState()
    {
        if (Time.time > idleEndTime)
        {
            TransitionToState(State.Walk);
        }

        if (isDangerClose)
        {
            TransitionToState(State.AttackOrFlee);
        }
    }

    void HandleWalkState()
    {
        if (forceIdle) TransitionToState(State.Idle);
        if (walkPointSet) CheckPathComplete();

        MovementAnimations();

        if (reachedDestination)
        {
            float chance = Random.Range(0f, 1f);
            if (chance <= 0.2f) 
            {
                TransitionToState(State.Idle);
            }
            else
            {
                TransitionToState(State.Walk);
            }
        }

        if (isDangerClose)
        {
            TransitionToState(State.AttackOrFlee);
        }
    }

    void HandleAttackOrFleeState()
    {
        if (isAggressive || Random.Range(0f, 1f) <= 0.20f)
        {
            TransitionToState(State.Attack);
        }
        else
        {
            TransitionToState(State.Flee);
        }
    }

    void HandleAttackState()
    {
        // Handle attack behavior.

        if (!isDangerClose)
        {
            TransitionToState(State.Idle);
        }
    }

    void HandleFleeState()
    {
        // Handle fleeing behavior. Maybe increase speed and run away from the danger source

        if (!isDangerClose)
        {
            TransitionToState(State.Idle);
        }
    }


    void MovementAnimations()
    {
        if (animator) animator.SetFloat("MoveMagnitude", agent.velocity.magnitude);
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

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
