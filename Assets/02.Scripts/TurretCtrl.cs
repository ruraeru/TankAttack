using UnityEngine;

public class TurretCtrl : MonoBehaviour
{
    Transform tr;
    public float rotSpeed = 5.0f;
    public float airAimRadius = 4f; //마우스 레이가 공중 적 이 거리 안을 지나가면 그 적을 조준한다
    CannonCtrl cannon;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //마우스가 공중 적(헬기)을 가리키면 그 적을 조준하고 포신 각도도 자동으로 맞춘다
        if (cannon == null) cannon = GetComponentInChildren<CannonCtrl>();
        EnemyAI air = AirTargetUnderMouse(ray);
        if (air != null)
        {
            Collider col = air.GetComponent<Collider>();
            Vector3 aim = col != null ? col.bounds.center : air.transform.position;
            RotateToward(aim);
            if (cannon != null) cannon.SetAutoTarget(aim);
            return;
        }
        if (cannon != null) cannon.ClearAutoTarget();
        //Ray가 8번째 레이어(Terrain)과 충돌되면 hit에 저장
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 8)) RotateToward(hit.point);
    }

    //목표를 터렛 지역 좌표로 바꿔 정면(Z축) 기준 각도를 구하고, 시간 오차를 보정해 Y축을 부드럽게 회전
    void RotateToward(Vector3 point)
    {
        Vector3 relative = tr.InverseTransformPoint(point);
        float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
        tr.Rotate(0, angle * Time.deltaTime * rotSpeed, 0);
    }

    //마우스 레이 둘레 airAimRadius 안에 걸리는 가장 가까운 공중 적
    EnemyAI AirTargetUnderMouse(Ray ray)
    {
        EnemyAI best = null;
        float bestDist = Mathf.Infinity;
        foreach (RaycastHit h in Physics.SphereCastAll(ray, airAimRadius, 500f, ~0, QueryTriggerInteraction.Ignore))
        {
            EnemyAI e = h.collider.GetComponentInParent<EnemyAI>();
            if (e == null || !e.isAir || h.distance >= bestDist) continue;
            best = e;
            bestDist = h.distance;
        }
        return best;
    }
}
