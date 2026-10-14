using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveHUD : MonoBehaviour
{
    public Text waveText;
    public Text enemyText;
    public Text scoreText;
    public Text bannerText;          // 화면 가운데 웨이브 안내 ("WAVE 2", "WAVE 1 클리어!")
    public float bannerTime = 2.5f;

    float nextPoll;
    int enemyCount;

    void Update()
    {
        GameManager gm = GameManager.instance;
        if (gm != null && waveText != null)
            waveText.text = "WAVE " + gm.currentWave + " / " + gm.maxWave;

        if (scoreText != null)
            scoreText.text = "점수 " + ScoreBoard.Score + "  (최고 " + ScoreBoard.Best + ")";

        if (enemyText != null && Time.unscaledTime >= nextPoll)
        {
            nextPoll = Time.unscaledTime + 0.5f;
            enemyCount = FindObjectsByType<EnemyAI>().Length;
            enemyText.text = "적 " + enemyCount;
        }
    }

    // WaveSpawner가 웨이브 시작과 클리어 때 부른다
    public void ShowBanner(string text)
    {
        if (bannerText == null) return;
        StopAllCoroutines();
        StartCoroutine(Banner(text));
    }

    IEnumerator Banner(string text)
    {
        bannerText.text = text;
        bannerText.gameObject.SetActive(true);
        Color c = bannerText.color;
        for (float t = 0f; t < bannerTime; t += Time.deltaTime)
        {
            // 0.3초 동안 나타났다가 마지막 0.6초 동안 사라진다
            float a = Mathf.Min(1f, t / 0.3f) * Mathf.Clamp01((bannerTime - t) / 0.6f);
            bannerText.color = new Color(c.r, c.g, c.b, a);
            bannerText.transform.localScale = Vector3.one * (1f + 0.15f * Mathf.Clamp01(1f - t / 0.3f));
            yield return null;
        }
        bannerText.gameObject.SetActive(false);
        bannerText.color = new Color(c.r, c.g, c.b, 1f);
    }
}
