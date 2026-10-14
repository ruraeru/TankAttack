using UnityEngine;

// 자주포 착탄 경고 원판. 떨어질 때가 가까울수록 빠르게 깜빡인다. EnemyAI가 비행 시간 뒤에 지운다.
public class ArtilleryMarker : MonoBehaviour
{
    public Color color = new Color(1f, 0.15f, 0.1f, 0.5f);
    Material mat;
    float start;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        start = Time.time;
    }

    void Update()
    {
        float t = Time.time - start;
        float blink = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(t * (4f + t * 4f)));
        mat.color = new Color(color.r, color.g, color.b, color.a * blink);
    }
}
