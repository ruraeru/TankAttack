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
    public GameObject shellPrefab; // 플레이어 1번 포탄 (Shell_Normal)
    public Transform firePos;      // 포구 (비우면 이름이 Cannon인 자식)

    private NavMeshAgent agent;
    private Transform playerTarget;
    private float lastAttackTime;
    private bool dead;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (firePos == null) firePos = FindDeep(transform, "Cannon");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;

        if (enemyType == EnemyType.Speed)
        {
            agent.speed = 8f;
            hp = 50f;
            attackRange = 70f;
            damage = 8;
        }
        else if (enemyType == EnemyType.Heavy)
        {
            agent.speed = 2f;
            hp = 120f;
            attackRange = 110f;
            damage = 25;
        }
        else // Normal
        {
            agent.speed = 3f;
            hp = 100f;
            attackRange = 90f;
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
        if (gm == null || gm.playerHP <= 0 || shellPrefab == null) return;

        Vector3 muzzle = firePos != null ? firePos.position : transform.position + Vector3.up * 3f + transform.forward * 3f;
        Vector3 aim = playerTarget.position + Vector3.up * 0.5f;
        // 포탄이 중력으로 떨어지는 만큼 위로 보정
        Cannon prefabCannon = shellPrefab.GetComponent<Cannon>();
        Rigidbody prefabRb = shellPrefab.GetComponent<Rigidbody>();
        if (prefabCannon != null && prefabRb != null && prefabRb.useGravity)
        {
            float v = prefabCannon.speed * Time.fixedDeltaTime / prefabRb.mass;
            float t = Vector3.Distance(muzzle, aim) / v;
            aim.y += 0.5f * Mathf.Abs(Physics.gravity.y) * t * t;
        }
        GameObject obj = Instantiate(shellPrefab, muzzle, Quaternion.LookRotation(aim - muzzle));
        Cannon c = obj.GetComponent<Cannon>();
        c.fromEnemy = true;
        c.damage = damage;

        AudioClip clip = Resources.Load<AudioClip>("CannonFire");
        if (clip != null) AudioSource.PlayClipAtPoint(clip, muzzle);
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
            if (t.name == name) return t;
        return null;
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0) Die();
    }

    void Die()
    {
        if (dead) return;
        dead = true;
        Cannon c = shellPrefab != null ? shellPrefab.GetComponent<Cannon>() : null;
        if (c != null && c.expEffect != null)
            Destroy(Instantiate(c.expEffect, transform.position + Vector3.up * 1.5f, Quaternion.identity), 3.0f);
        Destroy(gameObject);
    }
}
