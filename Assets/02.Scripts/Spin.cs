using UnityEngine;

// 헬기 회전날개처럼 계속 돌리는 장식용 회전
public class Spin : MonoBehaviour
{
    public Vector3 axis = Vector3.up;   // 월드 기준 회전축
    public float speed = 900f;          // 초당 각도

    void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime, Space.World);
    }
}
