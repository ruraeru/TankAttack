using UnityEngine;

public class TurretCtrl : MonoBehaviour
{
    Transform tr;
    RaycastHit hit; //Ray의 지면에 맞은 위치를 저장할 변수
    public float rotSpeed = 5.0f;
    public float airAimRadius = 4f; //마우스 레이가 공중 적 이 거리 안을 지나가면 그 적을 조준한다
    CannonCtrl cannon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tr = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green);
        //마우스가 공중 적(헬기)을 가리키면 그 적을 조준하고 포신 각도도 자동으로 맞춘다
        if (cannon == null) cannon = GetComponentInChildren<CannonCtrl>();
        EnemyAI air = AirTargetUnderMouse(ray);
        if (air != null)
        {
            Vector3 aim = air.GetComponent<Collider>() != null ? air.GetComponent<Collider>().bounds.center : air.transform.position;
            RotateToward(aim);
            if (cannon != null) cannon.SetAutoTarget(aim);
            return;
        }
        if (cannon != null) cannon.ClearAutoTarget();
        //Ray가 8번째 레이어(Terrain)과 충돌되면 hit에 저장
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 8))
        {
            //Raycast: 월드공간 기준을 현재 tr(터렛)을 중심으로 한 지역 좌표계(Local)로 변환
            //터렛 입장에서 목표물이 어디에 있는지(방향) 확인이 가능함
            Vector3 relative = tr.InverseTransformPoint(hit.point);
            //터렛의 정면(Z축)을 기준으로 목표지점(X, Z좌표)까지의 각도를 구함,
            //계산된 라디안 값을 Mathf.Rad2Deg를 곱해 Degree(일반적인 각도)로 변환
            float angle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
            //구한 각도(angle)에 시간 오차를 보정하여 Y축을 부드럽게 회전
            tr.Rotate(0, angle * Time.deltaTime * rotSpeed, 0);
        }
    }

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
