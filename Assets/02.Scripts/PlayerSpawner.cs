using UnityEngine;
using UnityStandardAssets.Utility;

// 게임이 시작되면 아군 기지의 PlayerSpawn 위치에 고른 탱크 (TankCatalog[TankSelection.Index])를 생성하고 카메라를 붙인다.
// 다른 스크립트가 Start에서 Player 태그를 찾으므로 Awake에서 만든다.
public class PlayerSpawner : MonoBehaviour
{
    public TankCatalog catalog;
    public GameObject playerPrefab; // 카탈로그가 비었을 때 쓰는 기본 탱크
    public Transform spawnPoint;
    public SmoothFollow cameraFollow;
    public string camPivotName = "CamPivot";
    public float groundOffset = 0.1f;

    public GameObject Player { get; private set; }

    void Awake()
    {
        GameObject prefab = catalog != null ? catalog.Get(TankSelection.Index) : null;
        if (prefab == null) prefab = playerPrefab;
        if (prefab == null || spawnPoint == null) return;

        Vector3 pos = spawnPoint.position;
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + groundOffset;

        Player = Instantiate(prefab, pos, Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f));
        Player.name = "Tank";

        TankInfo info = Player.GetComponent<TankInfo>();
        // Awake 순서가 정해져 있지 않아 GameManager.instance가 아직 비어 있을 수 있다
        GameManager gm = GameManager.instance != null ? GameManager.instance : FindAnyObjectByType<GameManager>();
        if (info != null && gm != null)
        {
            gm.playerMaxHP = info.maxHP;
            gm.playerHP = info.maxHP;
        }

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
