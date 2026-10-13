using UnityEngine;
using UnityEngine.UI;

public class WaveHUD : MonoBehaviour
{
    public Text waveText;
    public Text enemyText;

    float nextPoll;
    int enemyCount;

    void Update()
    {
        GameManager gm = GameManager.instance;
        if (gm != null && waveText != null)
            waveText.text = "WAVE " + gm.currentWave + " / " + gm.maxWave;

        if (enemyText != null && Time.unscaledTime >= nextPoll)
        {
            nextPoll = Time.unscaledTime + 0.5f;
            enemyCount = FindObjectsByType<EnemyAI>().Length;
            enemyText.text = "적 " + enemyCount;
        }
    }
}
