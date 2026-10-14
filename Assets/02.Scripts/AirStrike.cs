using System.Collections;
using UnityEngine;

// G키 공습: 마우스가 가리키는 지형 위로 전투기가 화면을 가로질러 날아가며 미사일을 줄지어 떨어뜨린다.
// 미사일은 Cannon을 재사용하므로 지형 충돌, 폭발, 적 피해는 Cannon이 처리한다.
public class AirStrike : MonoBehaviour
{
    public GameObject planePrefab;
    public GameObject missilePrefab;
    public KeyCode key = KeyCode.G;
    public float cooldown = 25f;
    public float flightHeight = 120f;      // 목표 지점 지면 위 고도
    public float planeSpeed = 100f;
    public float approachDistance = 250f;  // 목표 앞뒤로 날아가는 거리
    public float bombRunHalfLength = 60f;   // 목표 앞뒤 이 거리 안에서 투하
    public int missileCount = 8;
    public float damage = 40f;
    public float blastRadius = 16f;
    public float effectScale = 2.5f;        // 폭발 이펙트 배율
    public float missileSpeed = 2500f;      // Cannon이 AddForce(forward * speed)로 아래로 민다
    public float missileLifetime = 6f;
    public float planeHeightOffset = 3.9f;  // Su57 모델이 피벗보다 이만큼 위에 그려진다

    float nextReadyTime;
    AudioClip dropSfx;

    public float RemainingTime => Mathf.Max(0f, nextReadyTime - Time.time);
    public float CooldownRatio => cooldown <= 0f ? 0f : Mathf.Clamp01(RemainingTime / cooldown);

    void Awake()
    {
        dropSfx = Resources.Load<AudioClip>("CannonFire");
    }

    void Update()
    {
        if (!Input.GetKeyDown(key) || Time.timeScale <= 0f || RemainingTime > 0f) return;
        if (planePrefab == null || missilePrefab == null || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 8)) return; // 지형 레이어만

        Vector3 dir = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
        nextReadyTime = Time.time + cooldown;
        StartCoroutine(BombingRun(hit.point, dir));
    }

    IEnumerator BombingRun(Vector3 target, Vector3 dir)
    {
        Vector3 start = target - dir * approachDistance;
        start.y = target.y + flightHeight - planeHeightOffset;
        GameObject plane = Instantiate(planePrefab, start, Quaternion.LookRotation(dir));

        float total = approachDistance * 2f;
        float spacing = missileCount > 1 ? bombRunHalfLength * 2f / (missileCount - 1) : 0f;
        float travelled = 0f;
        int dropped = 0;
        while (travelled < total)
        {
            float step = planeSpeed * Time.deltaTime;
            travelled += step;
            plane.transform.position += dir * step;

            while (dropped < missileCount && travelled >= approachDistance - bombRunHalfLength + spacing * dropped)
            {
                Drop(plane.transform.position + Vector3.up * planeHeightOffset);
                dropped++;
            }
            yield return null;
        }
        Destroy(plane);
    }

    void Drop(Vector3 pos)
    {
        GameObject obj = Instantiate(missilePrefab, pos + Vector3.down * 2f, Quaternion.LookRotation(Vector3.down));
        Cannon c = obj.GetComponent<Cannon>();
        if (c != null)
        {
            c.speed = missileSpeed;
            c.damage = damage;
            c.blastRadius = blastRadius;
            c.lifetime = missileLifetime;
            c.effectScale = effectScale;
        }
        if (dropSfx != null) AudioSource.PlayClipAtPoint(dropSfx, pos, 0.6f);
    }
}
