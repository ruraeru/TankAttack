using UnityEngine;
using UnityStandardAssets.Utility;

// 게임이 시작되면 아군 기지의 PlayerSpawn 위치에 플레이어 탱크를 생성하고 카메라를 붙인다.
// 다른 스크립트가 Start에서 Player 태그를 찾으므로 Awake에서 만든다.
public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public SmoothFollow cameraFollow;
    public string camPivotName = "CamPivot";
    public float groundOffset = 0.1f;

    public GameObject Player { get; private set; }

    void Awake()
    {
        if (playerPrefab == null || spawnPoint == null) return;

        Vector3 pos = spawnPoint.position;
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + groundOffset;

        Player = Instantiate(playerPrefab, pos, Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f));
        Player.name = "Tank";

        if (cameraFollow != null)
        {
            Transform pivot = FindDeep(Player.transform, camPivotName);
            cameraFollow.target = pivot != null ? pivot : Player.transform;
        }
    }

    static Transform FindDeep(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
            if (t.name == name) return t;
        return null;
    }
}
