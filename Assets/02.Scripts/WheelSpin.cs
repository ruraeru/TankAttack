using UnityEngine;
using UnityEngine.AI;

// 이동 속도에 맞춰 바퀴를 굴린다. 제자리 회전할 때는 좌우 바퀴가 반대로 돈다.
// 플레이어는 Rigidbody, 적은 NavMeshAgent 속도를 쓴다.
public class WheelSpin : MonoBehaviour
{
    public Transform[] leftWheels;
    public Transform[] rightWheels;
    public float radius = 0.8f;      // 바퀴 반지름 (월드 단위)
    public float halfWidth = 2f;     // 좌우 바퀴 사이 거리의 절반

    Rigidbody rbody;
    NavMeshAgent agent;
    float lastYaw;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        lastYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (radius <= 0f || Time.deltaTime <= 0f) return;
        float yaw = transform.eulerAngles.y;
        float yawRate = Mathf.DeltaAngle(lastYaw, yaw) * Mathf.Deg2Rad / Time.deltaTime;
        lastYaw = yaw;
        Vector3 velocity = rbody != null ? rbody.linearVelocity : agent != null ? agent.velocity : Vector3.zero;
        float forward = Vector3.Dot(velocity, transform.forward);
        float turn = yawRate * halfWidth; // 오른쪽으로 돌면 왼쪽 바퀴가 앞으로
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
