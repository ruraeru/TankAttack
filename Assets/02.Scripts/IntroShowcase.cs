using UnityEngine;

// Intro 배경: 고른 탱크를 받침 위에 세워 두고 카메라가 천천히 주위를 돈다.
// 전시용 복사본이라 조작 스크립트는 지우고 포탑만 좌우로 천천히 흔든다.
public class IntroShowcase : MonoBehaviour
{
    public TankCatalog catalog;
    public Transform spot;              // 탱크를 세울 위치
    public Transform cam;
    public float orbitSpeed = 6f;       // 초당 도는 각도
    public float distance = 30f;
    public float height = 10f;
    public float lookHeight = 3f;
    public float screenShift = 7f;      // 탱크가 화면 오른쪽에 오도록 시선을 왼쪽으로 민다 (왼쪽은 메뉴)
    public float turretSwing = 25f;

    GameObject current;
    Transform turret;
    float angle = 200f;

    void Start()
    {
        Show(TankSelection.Index);
    }

    public void Show(int index)
    {
        if (catalog == null || spot == null) return;
        GameObject prefab = catalog.Get(index);
        if (prefab == null) return;
        if (current != null) Destroy(current);

        Vector3 pos = spot.position;
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null) pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
        current = Instantiate(prefab, pos, spot.rotation);
        current.tag = "Untagged";

        TurretCtrl tc = current.GetComponentInChildren<TurretCtrl>();
        turret = tc != null ? tc.transform : null;
        foreach (MonoBehaviour mb in current.GetComponentsInChildren<MonoBehaviour>())
            if (!(mb is TankInfo))
            {
                mb.enabled = false; // 같은 프레임에 Update가 돌지 않게 먼저 끈다
                Destroy(mb);        // 입력을 받는 조작 스크립트 제거
            }
        Rigidbody rb = current.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void LateUpdate()
    {
        if (turret != null)
            turret.localRotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * 0.4f) * turretSwing, 0f);
        if (cam == null || spot == null) return;

        angle += orbitSpeed * Time.deltaTime;
        Vector3 center = spot.position + Vector3.up * lookHeight;
        cam.position = spot.position + Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, height, -distance);
        cam.LookAt(center);
        cam.LookAt(center - cam.right * screenShift);
    }
}
