using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public float speed = 6000.0f;
    public GameObject expEffect;
    public float damage = 30.0f;
    public float blastRadius = 3.0f;
    public float effectLift = 1.0f; // 폭발 이펙트를 표면에서 띄우는 높이 (지면에 반쯤 묻혀 보이지 않게)
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
        StartCoroutine(this.ExplosionCannon(3.0f));
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
        _prevPos = pos;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_exploded) return;
        // 트리거는 물리 스텝 뒤에 불려서 이미 지형 속으로 들어가 있다. 이전 위치부터 다시 쏴서 표면 지점을 찾는다
        if (TryHitBetween(_prevPos, _rigidbody.position, out RaycastHit hit)) Explode(hit.point, hit.normal);
        else Explode(transform.position, Vector3.up);
    }
    bool TryHitBetween(Vector3 from, Vector3 to, out RaycastHit hit)
    {
        Vector3 delta = to - from;
        if (delta.sqrMagnitude > 0.0001f &&
            Physics.Raycast(from, delta.normalized, out hit, delta.magnitude, ~0, QueryTriggerInteraction.Ignore) &&
            !hit.collider.CompareTag("Player"))
            return true;
        hit = default;
        return false;
    }
    IEnumerator ExplosionCannon(float tm)
    {

        yield return new WaitForSeconds(tm);
        Explode(transform.position, Vector3.up); // 3초 타이머: 허공에서 터짐
    }
    void Explode(Vector3 point, Vector3 normal)
    {
        if (_exploded) return; // 충돌과 3초 타이머가 겹쳐도 한 번만 폭발
        _exploded = true;
        _rigidbody.isKinematic = true;  //물리엔진의 영향을 받지 않음
        transform.position = point;
        // 폭발 범위 안의 적에게 피해
        foreach (Collider hit in Physics.OverlapSphere(point, blastRadius))
        {
            EnemyAI enemy = hit.GetComponentInParent<EnemyAI>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
        _collider.enabled = false; // 충돌이 안되도록
        //폭발 프리팹 생성
        GameObject obj = (GameObject)Instantiate(expEffect, point + normal * effectLift, Quaternion.identity);
        Destroy(obj, 3.0f);//폭발효과 3초뒤 사라짐(Particle Pack 이펙트가 길다),
        Destroy(this.gameObject, 1.0f); //Trail Renderer가 사라질 때 까지 기다림
    }
    // Update is called once per frame
    void Update()
    {

    }
}
