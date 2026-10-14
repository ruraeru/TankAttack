using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public float speed = 6000.0f;
    public GameObject expEffect;
    public float damage = 30.0f;
    public float blastRadius = 3.0f;
    public float effectLift = 1.0f; // 폭발 이펙트를 표면에서 띄우는 높이 (지면에 반쯤 묻혀 보이지 않게)
    public float lifetime = 3.0f; // 이 시간이 지나면 허공에서 터진다 (공습 미사일은 더 길게)
    public float effectScale = 1f; // 폭발 이펙트 크기 배율
    public bool fromEnemy; // 적이 쏜 포탄: 플레이어에게 피해, 적 탱크는 무시
    public float proximityRadius = 6f; // 근접 신관: 플레이어 포탄이 공중 적 이 거리 안을 지나가면 터진다
    private bool _exploded;
    private Vector3 _prevPos;
    private CapsuleCollider _collider;
    Rigidbody _rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().AddForce(transform.forward * speed);
        _prevPos = transform.position;
        StartCoroutine(this.ExplosionCannon(lifetime));
    }
    // 고속탄은 한 프레임에 지형을 뚫고 지나가므로, 이전 위치에서 현재 위치까지 레이를 쏴서 먼저 닿은 곳에서 터뜨린다
    void FixedUpdate()
    {
        if (_exploded) return;
        Vector3 pos = _rigidbody.position;
        if (TryHitBetween(_prevPos, pos, out RaycastHit hit))
        {
            Explode(hit.point, hit.normal);
            return;
        }
        if (!fromEnemy && TryProximity(_prevPos, pos)) return;
        _prevPos = pos;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_exploded) return;
        if (fromEnemy && other.GetComponentInParent<EnemyAI>() != null) return;
        // 트리거는 물리 스텝 뒤에 불려서 이미 지형 속으로 들어가 있다. 이전 위치부터 다시 쏴서 표면 지점을 찾는다
        if (TryHitBetween(_prevPos, _rigidbody.position, out RaycastHit hit)) Explode(hit.point, hit.normal);
        else Explode(transform.position, Vector3.up);
    }
    bool TryHitBetween(Vector3 from, Vector3 to, out RaycastHit hit)
    {
        Vector3 delta = to - from;
        if (delta.sqrMagnitude > 0.0001f &&
            Physics.Raycast(from, delta.normalized, out hit, delta.magnitude, ~0, QueryTriggerInteraction.Ignore) &&
            IsValidHit(hit.collider))
            return true;
        hit = default;
        return false;
    }
    // 근접 신관: 이번 스텝에 지나간 구간 둘레 proximityRadius 안에 공중 적이 있으면 그 자리에서 터뜨린다
    // 폭발 반경이 작은 포탄도 맞도록 그 적에게는 직접 피해를 준다
    bool TryProximity(Vector3 from, Vector3 to)
    {
        if (proximityRadius <= 0f) return false;
        foreach (Collider c in Physics.OverlapCapsule(from, to, proximityRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            EnemyAI enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy == null || !enemy.isAir) continue;
            Vector3 seg = to - from;
            float t = seg.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector3.Dot(c.bounds.center - from, seg) / seg.sqrMagnitude) : 0f;
            enemy.TakeDamage(damage);
            Explode(from + seg * t, Vector3.up, enemy);
            return true;
        }
        return false;
    }
    bool IsValidHit(Collider c)
    {
        if (fromEnemy) return c.GetComponentInParent<EnemyAI>() == null;
        return !c.CompareTag("Player");
    }
    IEnumerator ExplosionCannon(float tm)
    {

        yield return new WaitForSeconds(tm);
        Explode(transform.position, Vector3.up); // 3초 타이머: 허공에서 터짐
    }
    void Explode(Vector3 point, Vector3 normal, EnemyAI alreadyHit = null)
    {
        if (_exploded) return; // 충돌과 3초 타이머가 겹쳐도 한 번만 폭발
        _exploded = true;
        _rigidbody.isKinematic = true;  //물리엔진의 영향을 받지 않음
        transform.position = point;
        // 폭발 범위 안의 적에게 피해
        bool playerHit = false;
        foreach (Collider hit in Physics.OverlapSphere(point, blastRadius))
        {
            if (fromEnemy)
            {
                if (!playerHit && hit.CompareTag("Player"))
                {
                    playerHit = true;
                    DamagePlayer();
                }
                continue;
            }
            EnemyAI enemy = hit.GetComponentInParent<EnemyAI>();
            if (enemy != null && enemy != alreadyHit) enemy.TakeDamage(damage);
        }
        _collider.enabled = false; // 충돌이 안되도록
        // 모델은 바로 숨기고 궤적(Trail)만 1초 남긴다. 폭발 이펙트만 보이게
        foreach (Renderer r in GetComponentsInChildren<MeshRenderer>()) r.enabled = false;
        //폭발 프리팹 생성
        SpawnEffect(expEffect, point + normal * effectLift, effectScale);
        Destroy(this.gameObject, 1.0f); //Trail Renderer가 사라질 때 까지 기다림
    }
    // 폭발 이펙트를 만들고 3초 뒤 지운다 (Particle Pack 이펙트가 길다). 배율을 주면 파티클 크기도 같이 키운다
    public static GameObject SpawnEffect(GameObject prefab, Vector3 pos, float scale)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        if (!Mathf.Approximately(scale, 1f))
        {
            foreach (ParticleSystem ps in obj.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy; // 파티클 크기도 같이 커지게
            }
            obj.transform.localScale *= scale;
        }
        Destroy(obj, 3.0f);//폭발효과 3초뒤 사라짐(Particle Pack 이펙트가 길다),
        return obj;
    }
    void DamagePlayer()
    {
        if (GameManager.instance != null) GameManager.instance.DamagePlayer(Mathf.RoundToInt(damage));
    }
    // Update is called once per frame
    void Update()
    {

    }
}
