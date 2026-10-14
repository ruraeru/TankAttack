using UnityEngine;

// 차체 모델을 지면 경사에 맞춰 앞뒤(pitch), 좌우(roll)로 기울인다.
// 루트는 TankMove가 Y축 회전만 하고 X/Z 회전은 Rigidbody에서 막혀 있으므로, 보이는 모델 자식만 돌린다.
public class HullTilt : MonoBehaviour
{
    public Transform model;          // 기울일 모델 자식 (Player_TANK는 Tank, Player_Tank006은 Model)
    public float halfLength = 5f;    // 차체 앞뒤 샘플 거리
    public float halfWidth = 2.5f;   // 차체 좌우 샘플 거리
    public float maxAngle = 30f;
    public float smooth = 8f;

    Terrain terrain;
    Quaternion baseRotation;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        if (model != null) baseRotation = model.localRotation;
    }

    void LateUpdate()
    {
        if (terrain == null || model == null) return;

        Vector3 p = transform.position;
        Vector3 f = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * halfLength;
        Vector3 r = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized * halfWidth;
        float hF = terrain.SampleHeight(p + f), hB = terrain.SampleHeight(p - f);
        float hR = terrain.SampleHeight(p + r), hL = terrain.SampleHeight(p - r);

        // 앞이 높으면 기수를 든다 (X축 음수), 오른쪽이 높으면 오른쪽을 든다 (Z축 양수)
        float pitch = Mathf.Clamp(Mathf.Atan2(hB - hF, halfLength * 2f) * Mathf.Rad2Deg, -maxAngle, maxAngle);
        float roll = Mathf.Clamp(Mathf.Atan2(hR - hL, halfWidth * 2f) * Mathf.Rad2Deg, -maxAngle, maxAngle);
        Quaternion target = Quaternion.Euler(pitch, 0f, roll) * baseRotation;
        model.localRotation = Quaternion.Slerp(model.localRotation, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
