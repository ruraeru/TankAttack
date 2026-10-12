using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Speed, Heavy, Normal }

public class EnemyAI : MonoBehaviour
{
    public EnemyType enemyType;
    public float hp = 50f;
    public float attackRange = 15f;
    public float attackCooldown = 2f;
    public int damage = 10;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        if (enemyType == EnemyType.Speed)
        {
            agent.speed = 8f;
            hp = 50f;
            attackRange = 10f;
            damage = 8;
        }
        else if (enemyType == EnemyType.Heavy)
        {
            agent.speed = 2f;
            hp = 120f;
            attackRange = 22f;
            damage = 25;
        }
        else // Normal
        {
            agent.speed = 3f;
            hp = 100f;
            attackRange = 15f;
            damage = 10;
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist <= attackRange)
        {
            agent.isStopped = true;

            Vector3 look = playerTarget.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 5f * Time.deltaTime);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                Attack();
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
    }

    void Attack()
    {
        GameManager gm = GameManager.instance;
        if (gm == null || gm.playerHP <= 0) return;

        gm.playerHP -= damage;
        if (gm.playerHP <= 0)
        {
            gm.playerHP = 0;
            gm.OnPlayerDead();
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0) Destroy(gameObject);
    }
}
