using UnityEngine;

// 포구(FireCannon.firePos)가 향한 방향으로 형광 분홍 레이저를 그린다. PlayerSpawner가 탱크를 만들 때 붙인다.
// 포탄은 중력으로 처질 수 있지만 레이저는 포신 방향의 직선이다.
[RequireComponent(typeof(FireCannon))]
public class AimLaser : MonoBehaviour
{
    public Color color = new Color(1f, 0.2f, 0.75f, 0.8f); // 형광 분홍 (초록 지형, 파란 하늘과 겹치지 않는 색, 반투명이라 포탄이 비친다)
    public float width = 0.12f;
    public float maxLength = 150f;
    public bool showHitDot = true;
    public float hitDotSize = 0.3f;

    Transform firePos;
    LineRenderer line;
    Transform hitDot;
    Material material;
    readonly RaycastHit[] hits = new RaycastHit[16];

    void LateUpdate()
    {
        Refresh();
    }

    // 포탑이 Update에서 돈 뒤에 호출된다 (편집 모드 검증에서도 직접 부른다)
    public void Refresh()
    {
        if (!Init()) return;
        Vector3 start = firePos.position;
        Vector3 dir = firePos.forward;
        Vector3 end = start + dir * maxLength;
        bool hit = false;

        int count = Physics.RaycastNonAlloc(start, dir, hits, maxLength, ~0, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider c = hits[i].collider;
            if (c.transform.IsChildOf(transform) || c.CompareTag("Player")) continue; // 자기 탱크는 무시
            if (hits[i].distance < best)
            {
                best = hits[i].distance;
                end = hits[i].point;
                hit = true;
            }
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);
        if (hitDot != null)
        {
            hitDot.gameObject.SetActive(showHitDot && hit);
            hitDot.position = end;
        }
    }

    bool Init()
    {
        if (firePos == null)
        {
            FireCannon fire = GetComponent<FireCannon>();
            firePos = fire != null ? fire.firePos : null;
            if (firePos == null) return false;
        }
        if (line != null) return true;

        // 밝은 하늘에서 Additive는 허옇게 날아가서 Unlit(Sprites/Default, 정점 색)을 쓰고, 알파로 연하게 한다
        Shader shader = Shader.Find("Sprites/Default");
        material = new Material(shader);

        GameObject lineObj = new GameObject("AimLaser");
        lineObj.transform.SetParent(transform, false);
        line = lineObj.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.material = material;
        line.startWidth = width;
        line.endWidth = width * 0.6f;
        line.startColor = color;
        line.endColor = color;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.numCapVertices = 4;

        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "AimLaserDot";
        dot.transform.SetParent(transform, false);
        dot.transform.localScale = Vector3.one * hitDotSize;
        Collider col = dot.GetComponent<Collider>();
        if (col != null)
        {
            if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
        }
        Renderer r = dot.GetComponent<Renderer>();
        r.sharedMaterial = material;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        // 라인과 같은 밝기가 되도록 점 전용 머티리얼에 색을 넣는다
        Material dotMat = new Material(shader);
        dotMat.color = color;
        r.sharedMaterial = dotMat;
        hitDot = dot.transform;
        return true;
    }

    void OnDestroy()
    {
        if (material != null) Destroy(material);
    }
}
