using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaveSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int baseCount = 3;
    public int countPerWave = 2;
    public float spawnInterval = 1.5f;
    public float waveDelay = 5f;

    readonly List<EnemyAI> alive = new List<EnemyAI>();
    bool spawning;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(waveDelay);
        while (GameManager.instance != null)
        {
            yield return SpawnWave(GameManager.instance.currentWave);

            while (alive.Exists(e => e != null)) yield return new WaitForSeconds(0.5f);
            alive.Clear();

            int wave = GameManager.instance.currentWave;
            GameManager.instance.OnWaveClear();
            if (wave >= GameManager.instance.maxWave) yield break;
            yield return new WaitForSeconds(waveDelay);
        }
    }

    IEnumerator SpawnWave(int wave)
    {
        spawning = true;
        int count = baseCount + countPerWave * (wave - 1);
        for (int i = 0; i < count; i++)
        {
            Transform sp = spawnPoints[i % spawnPoints.Length];
            Vector3 pos = sp.position;
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas)) pos = hit.position;

            GameObject go = Instantiate(enemyPrefab, pos, sp.rotation);
            EnemyAI ai = go.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.enemyType = PickType(wave);
                alive.Add(ai);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        spawning = false;
    }

    EnemyType PickType(int wave)
    {
        float r = Random.value;
        if (wave >= 3 && r < 0.3f) return EnemyType.Heavy;
        if (wave >= 2 && r < 0.6f) return EnemyType.Speed;
        return EnemyType.Normal;
    }
}
