using UnityEngine;

public class TrackScroll : MonoBehaviour
{
    public float cleatSize = 0.5f; //텍스처 한 주기에 해당하는 길이(m)
    public float halfWidth = 2.3f; //좌우 궤도 사이 거리의 절반(m)
    Rigidbody rbody;
    Material mat;
    float offset;

    void Start()
    {
        rbody = GetComponentInParent<Rigidbody>();
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (rbody == null) return;
        float forward = Vector3.Dot(rbody.linearVelocity, rbody.transform.forward);
        float turn = Mathf.Abs(rbody.angularVelocity.y) * halfWidth;
        //제자리 회전할 때는 전진 속도가 없으므로 회전 속도로 궤도를 돌린다
        float trackSpeed = Mathf.Abs(forward) > 0.1f ? forward : turn;
        //바닥에 닿은 궤도는 차체 기준 뒤로 흐른다
        offset += trackSpeed / cleatSize * Time.deltaTime;
        mat.mainTextureOffset = new Vector2(Mathf.Repeat(offset, 1f), 0f);
    }
}
