using UnityEngine;

public class CannonCtrl : MonoBehaviour
{
    Transform tr;
    public float rotSpeed = 50.0f;
    public float minAngle = -8.0f; //포신을 내릴 수 있는 한계(도)
    public float maxAngle = 25.0f; //포신을 올릴 수 있는 한계(도)
    public float autoAimSpeed = 60.0f; //자동 조준 때 포신이 움직이는 속도(도/초)
    float pitch;
    bool hasAutoTarget;
    Vector3 autoTarget;
    FireCannon fire;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        if (hasAutoTarget)
            pitch = Mathf.MoveTowards(pitch, Mathf.Clamp(AutoPitch(), minAngle, maxAngle), autoAimSpeed * Time.deltaTime);
        else
            //휠을 올리면 포신이 올라가고, 제한 각도 안으로 고정
            pitch = Mathf.Clamp(pitch + Input.GetAxis("Mouse ScrollWheel") * rotSpeed, minAngle, maxAngle);
        tr.localRotation = Quaternion.Euler(-pitch, 0, 0);
    }

    //TurretCtrl이 공중 적을 가리킬 때 부른다. 휠 대신 그 지점을 향하도록 포신 각도를 맞춘다
    public void SetAutoTarget(Vector3 point)
    {
        hasAutoTarget = true;
        autoTarget = point;
    }

    public void ClearAutoTarget()
    {
        hasAutoTarget = false;
    }

    //포신 피벗에서 목표까지의 올려본 각도. 중력으로 처지는 포탄이면 떨어지는 만큼 높여 잡는다
    float AutoPitch()
    {
        if (tr == null) return pitch;
        Vector3 target = autoTarget;
        if (fire == null) fire = GetComponentInParent<FireCannon>();
        if (fire != null)
        {
            ShellType shell = fire.Current;
            GameObject prefab = shell.shellPrefab != null ? shell.shellPrefab : fire.cannon;
            Rigidbody rb = prefab != null ? prefab.GetComponent<Rigidbody>() : null;
            if (rb != null && rb.useGravity)
            {
                float v = shell.speed * Time.fixedDeltaTime / rb.mass;
                float t = Vector3.Distance(tr.position, target) / Mathf.Max(1f, v);
                target.y += 0.5f * Mathf.Abs(Physics.gravity.y) * t * t;
            }
        }
        Vector3 local = tr.parent != null ? tr.parent.InverseTransformPoint(target) - tr.localPosition : target - tr.position;
        return Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg;
    }
}
