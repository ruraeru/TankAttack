using System.Collections;
using UnityEngine;

// 필드에 아이템을 일정 간격으로 떨어뜨린다. 지형 위 무작위 지점에 지면 높이를 맞춰 생성한다.
public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;
    public Terrain terrain;
    public Transform player;
    public float firstDelay = 10f;
    public float interval = 15f;
    public int maxAlive = 5;
    public float edgeMargin = 30f;
    public float minPlayerDistance = 20f;
    public float maxPlayerDistance = 120f;
    public float maxSlope = 25f;
    public float heightOffset = 1.5f;

    int alive;

    IEnumerator Start()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0) yield break;
        if (terrain == null) terrain = Terrain.activeTerrain;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        yield return new WaitForSeconds(firstDelay);
        while (true)
        {
            if (CountAlive() < maxAlive) TrySpawn();
            yield return new WaitForSeconds(interval);
        }
    }

    int CountAlive() => FindObjectsByType<Item>().Length;

    void TrySpawn()
    {
        Vector3 origin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(edgeMargin, size.x - edgeMargin);
            float z = Random.Range(edgeMargin, size.z - edgeMargin);
            Vector3 pos = new Vector3(origin.x + x, 0f, origin.z + z);

            if (terrain.terrainData.GetSteepness(x / size.x, z / size.z) > maxSlope) continue;
            if (player != null)
            {
                float d = Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(player.position.x, player.position.z));
                if (d < minPlayerDistance || d > maxPlayerDistance) continue;
            }

            pos.y = terrain.SampleHeight(pos) + origin.y + heightOffset;
            Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Length)], pos, Quaternion.identity);
            return;
        }
    }
}
