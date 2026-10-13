using UnityEngine;

public class TankMove : MonoBehaviour
{
    public float moveSpeed = 20.0f;
    public float rotSpeed = 50.0f;
    public float groundOffset = 0.1f; //모델 바닥과 지면 사이 간격
    public float edgeMargin = 5.0f; //지형 가장자리에서 이만큼 안쪽까지만 이동
    Rigidbody rbody;
    Transform tr;
    Terrain terrain;
    float h, v;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        terrain = Terrain.activeTerrain;
        //Rigidbody의 무게중심을 낮게 설정
        rbody.centerOfMass = new Vector3(0.0f, -0.5f, 0.0f);
        //지형 높이는 코드로 따라가므로 지형과의 물리 충돌은 끔 (건물, 적과는 그대로 충돌)
        rbody.useGravity = false;
        TerrainCollider terrainCol = terrain.GetComponent<TerrainCollider>();
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(c, terrainCol);
        }
    }

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("Horizontal"); //방향키 좌우(-1 - +1 값 리턴)
        v = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        //수평 속도는 평지든 경사든 항상 moveSpeed
        Vector3 forward = Vector3.ProjectOnPlane(tr.forward, Vector3.up).normalized;
        Vector3 vel = forward * v * moveSpeed;

        //지형 가장자리 밖으로 나가지 못하게 다음 위치를 제한
        Vector3 next = rbody.position + vel * Time.fixedDeltaTime;
        Vector3 min = terrain.transform.position + Vector3.one * edgeMargin;
        Vector3 max = terrain.transform.position + terrain.terrainData.size - Vector3.one * edgeMargin;
        next.x = Mathf.Clamp(next.x, min.x, max.x);
        next.z = Mathf.Clamp(next.z, min.z, max.z);
        vel.x = (next.x - rbody.position.x) / Time.fixedDeltaTime;
        vel.z = (next.z - rbody.position.z) / Time.fixedDeltaTime;

        //다음 위치의 지형 높이로 올라가거나 내려가도록 수직 속도를 정함
        float targetY = GroundY(next) + groundOffset;
        vel.y = (targetY - rbody.position.y) / Time.fixedDeltaTime;

        rbody.linearVelocity = vel;
        rbody.angularVelocity = Vector3.up * (h * rotSpeed * Mathf.Deg2Rad);
    }

    float GroundY(Vector3 pos)
    {
        return terrain.SampleHeight(pos) + terrain.transform.position.y;
    }
}
