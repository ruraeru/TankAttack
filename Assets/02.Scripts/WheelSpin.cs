using UnityEngine;

// 이동 속도에 맞춰 바퀴를 굴린다. 제자리 회전할 때는 좌우 바퀴가 반대로 돈다.
public class WheelSpin : MonoBehaviour
{
    public Transform[] leftWheels;
    public Transform[] rightWheels;
    public float radius = 0.8f;      // 바퀴 반지름 (월드 단위)
    public float halfWidth = 2f;     // 좌우 바퀴 사이 거리의 절반

    Rigidbody rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rbody == null || radius <= 0f) return;
        float forward = Vector3.Dot(rbody.linearVelocity, transform.forward);
        float turn = rbody.angularVelocity.y * halfWidth; // 오른쪽으로 돌면 왼쪽 바퀴가 앞으로
        Spin(leftWheels, forward + turn);
        Spin(rightWheels, forward - turn);
    }

    void Spin(Transform[] wheels, float speed)
    {
        float deg = speed / radius * Mathf.Rad2Deg * Time.deltaTime;
        foreach (Transform w in wheels)
            if (w != null) w.Rotate(deg, 0f, 0f, Space.Self);
    }
}
