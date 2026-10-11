using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Speed, Heavy, Normal }

public class EnemyAI : MonoBehaviour
{
    public EnemyType enemyType;
    public float hp = 50f;
    public float attackRange = 15f;
    public float attackCooldown = 2f;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        if (enemyType == EnemyType.Speed)
        {
            agent.speed = 8f;
            hp = 50f;
        }
        else if (enemyType == EnemyType.Heavy)
        {
            agent.speed = 2f;
            hp = 120f;
        }
        else // Normal
        {
            agent.speed = 3f;
            hp = 100f;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist <= attackRange)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
    }
}