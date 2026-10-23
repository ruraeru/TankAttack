using UnityEngine;
using UnityEngine.AI;

// 적의 공격 방식
// Shell: 사거리 안에서 멈추고 포탄을 쏜다 (헬기도 이 방식, 공중에서 쏜다)
// Ram: 멈추지 않고 돌진해서 가까워지면 자폭한다
// Artillery: 먼 사거리에서 멈추고 곡사한다. 착탄 지점에 경고 표시를 띄운다
public enum EnemyAttackMode { Shell, Ram, Artillery }

// 능력치는 종류별 프리팹 (Assets/03.Prefabs/Enemies/Enemy_*.prefab)에 저장한다.
public class EnemyAI : MonoBehaviour
{
    public EnemyAttackMode attackMode = EnemyAttackMode.Shell;
    public bool isAir;               // 공중 적 (헬기): 플레이어 포탄의 근접 신관과 포탑 자동 조준 대상
    public float moveSpeed = 3f;
    public float hp = 100f;
    public float attackRange = 90f;
    public float attackCooldown = 2f;
    public int damage = 10;
    public int scoreValue = 100;
    public GameObject shellPrefab;   // Shell, Artillery가 쏘는 포탄
    public Transform firePos;        // 포구 (비우면 이름이 FirePos, 없으면 Cannon인 자식)
    public GameObject deathEffect;   // 비우면 포탄 프리팹의 expEffect
    public float deathEffectScale = 1f;

    [Header("Ram (자폭)")]
    public float explodeRange = 9f;  // 플레이어 표면까지 이 거리 안이면 터진다
    public float explodeRadius = 12f;

    [Header("Artillery (곡사)")]
    public float launchAngle = 50f;
    public GameObject markerPrefab;  // 착탄 경고 원판

    private NavMeshAgent agent;
    private Transform playerTarget;
    private Collider playerCollider;
    private float lastAttackTime = -999f;
    private bool dead;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        if (firePos == null) firePos = FindDeep(transform, "FirePos");
        if (firePos == null) firePos = FindDeep(transform, "Cannon");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            playerCollider = playerObj.GetComponent<Collider>();
        }
    }

    void Update()
    {
        if (playerTarget == null || dead) return;

        if (attackMode == EnemyAttackMode.Ram)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
            Vector3 closest = playerCollider != null ? playerCollider.ClosestPoint(transform.position) : playerTarget.position;
            if (Vector3.Distance(closest, transform.position) <= explodeRange) Explode();
            return;
        }

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
                if (attackMode == EnemyAttackMode.Artillery) FireArtillery();
                else Attack();
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
        if (!CanShoot()) return;

        Vector3 muzzle = firePos != null ? firePos.position : transform.position + Vector3.up * 3f + transform.forward * 3f;
        Vector3 aim = playerTarget.position + Vector3.up * 0.5f;
        // 포탄이 중력으로 떨어지는 만큼 위로 보정
        Cannon prefabCannon = shellPrefab.GetComponent<Cannon>();
        Rigidbody prefabRb = shellPrefab.GetComponent<Rigidbody>();
        if (prefabCannon != null && prefabRb != null && prefabRb.useGravity)
            aim.y += Cannon.GravityDrop(prefabCannon.speed, prefabRb.mass, Vector3.Distance(muzzle, aim));
        SpawnShell(muzzle, Quaternion.LookRotation(aim - muzzle));
        PlayFireSound(muzzle);
    }

    bool CanShoot()
    {
        GameManager gm = GameManager.instance;
        return gm != null && gm.playerHP > 0 && shellPrefab != null;
    }

    Cannon SpawnShell(Vector3 muzzle, Quaternion rot)
    {
        Cannon c = Instantiate(shellPrefab, muzzle, rot).GetComponent<Cannon>();
        c.fromEnemy = true;
        c.damage = damage;
        return c;
    }

    // 발사각을 고정하고, 목표까지 수평 거리와 높이 차로 필요한 초속을 구해 포물선으로 쏜다
    void FireArtillery()
    {
        if (!CanShoot()) return;

        Vector3 muzzle = firePos != null ? firePos.position : transform.position + Vector3.up * 4f;
        Vector3 target = playerTarget.position;
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null) target.y = terrain.SampleHeight(target) + terrain.transform.position.y;

        Vector3 flat = target - muzzle; flat.y = 0f;
        float d = flat.magnitude;
        float h = target.y - muzzle.y;
        float g = Mathf.Abs(Physics.gravity.y);
        float angle = launchAngle * Mathf.Deg2Rad;
        float denom = 2f * Mathf.Cos(angle) * Mathf.Cos(angle) * (d * Mathf.Tan(angle) - h);
        if (d < 1f || denom <= 0f) return; // 너무 가깝거나 각도로 닿지 않는 높이
        float speed = Mathf.Sqrt(g * d * d / denom);
        Vector3 vel = flat.normalized * speed * Mathf.Cos(angle) + Vector3.up * speed * Mathf.Sin(angle);
        float flightTime = d / (speed * Mathf.Cos(angle));

        Cannon c = SpawnShell(muzzle, Quaternion.LookRotation(vel));
        c.speed = 0f;                    // AddForce 대신 속도를 직접 넣는다
        c.lifetime = flightTime + 2f;
        c.GetComponent<Rigidbody>().linearVelocity = vel;

        if (markerPrefab != null)
        {
            GameObject marker = Instantiate(markerPrefab, target + Vector3.up * 0.2f, Quaternion.identity);
            marker.transform.localScale = new Vector3(c.blastRadius * 2f, marker.transform.localScale.y, c.blastRadius * 2f);
            Destroy(marker, flightTime + 0.3f);
        }
        PlayFireSound(muzzle);
    }

    void Explode()
    {
        if (dead) return;
        dead = true;
        foreach (Collider col in Physics.OverlapSphere(transform.position, explodeRadius))
        {
            if (!col.CompareTag("Player")) continue;
            GameManager gm = GameManager.instance;
            if (gm != null) gm.DamagePlayer(damage);
            break;
        }
        SpawnDeathEffect();
        Destroy(gameObject); // 자폭은 점수를 주지 않는다
    }

    static void PlayFireSound(Vector3 pos)
    {
        AudioClip clip = Resources.Load<AudioClip>("CannonFire");
        if (clip != null) AudioSource.PlayClipAtPoint(clip, pos);
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
        ScoreBoard.Add(scoreValue);
        SpawnDeathEffect();
        Destroy(gameObject);
    }

    void SpawnDeathEffect()
    {
        GameObject fx = deathEffect;
        if (fx == null)
        {
            Cannon c = shellPrefab != null ? shellPrefab.GetComponent<Cannon>() : null;
            if (c != null) fx = c.expEffect;
        }
        if (fx != null) Cannon.SpawnEffect(fx, transform.position + Vector3.up * 1.5f, deathEffectScale);
    }
}
