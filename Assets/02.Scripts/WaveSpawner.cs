using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemyEntry
{
    public GameObject prefab;
    public int minWave = 1;     // 이 웨이브부터 나온다
    public float weight = 1f;   // 열린 종류 중 뽑힐 비율
}

public class WaveSpawner : MonoBehaviour
{
    public EnemyEntry[] enemies;
    public Transform[] spawnPoints;
    public int baseCount = 3;
    public int countPerWave = 2;
    public float spawnInterval = 1.5f;
    public float waveDelay = 5f;

    readonly List<EnemyAI> alive = new List<EnemyAI>();
    bool spawning;
    WaveHUD hud;

    IEnumerator Start()
    {
        hud = FindAnyObjectByType<WaveHUD>();
        yield return new WaitForSeconds(waveDelay);
        while (GameManager.instance != null)
        {
            int current = GameManager.instance.currentWave;
            if (hud != null) hud.ShowBanner(current >= GameManager.instance.maxWave ? "최종 WAVE " + current : "WAVE " + current);
            yield return SpawnWave(GameManager.instance.currentWave);

            while (alive.Exists(e => e != null)) yield return new WaitForSeconds(0.5f);
            alive.Clear();

            int wave = GameManager.instance.currentWave;
            if (hud != null && wave < GameManager.instance.maxWave) hud.ShowBanner("WAVE " + wave + " 클리어!");
            GameManager.instance.OnWaveClear();
            if (wave >= GameManager.instance.maxWave) yield break;
            yield return new WaitForSeconds(waveDelay);
        }
    }

    IEnumerator SpawnWave(int wave)
    {
        spawning = true;
        int count = baseCount + countPerWave * (wave - 1);
        List<GameObject> order = BuildOrder(wave, count);
        for (int i = 0; i < order.Count; i++)
        {
            Transform sp = spawnPoints[i % spawnPoints.Length];
            Vector3 pos = sp.position;
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas)) pos = hit.position;

            GameObject go = Instantiate(order[i], pos, sp.rotation);
            EnemyAI ai = go.GetComponent<EnemyAI>();
            if (ai != null) alive.Add(ai);
            yield return new WaitForSeconds(spawnInterval);
        }
        spawning = false;
    }

    // 이번 웨이브에 처음 열린 종류는 1대씩 먼저 넣고, 나머지는 열린 종류 중 가중치로 뽑아 섞는다
    List<GameObject> BuildOrder(int wave, int count)
    {
        List<EnemyEntry> open = new List<EnemyEntry>();
        foreach (EnemyEntry e in enemies)
            if (e.prefab != null && e.minWave <= wave) open.Add(e);
        List<GameObject> order = new List<GameObject>();
        if (open.Count == 0) return order;

        foreach (EnemyEntry e in open)
            if (e.minWave == wave && order.Count < count) order.Add(e.prefab);

        float total = 0f;
        foreach (EnemyEntry e in open) total += e.weight;
        while (order.Count < count)
        {
            float r = Random.value * total;
            EnemyEntry pick = open[open.Count - 1];
            foreach (EnemyEntry e in open)
            {
                if (r < e.weight) { pick = e; break; }
                r -= e.weight;
            }
            order.Add(pick.prefab);
        }
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        return order;
    }
}
