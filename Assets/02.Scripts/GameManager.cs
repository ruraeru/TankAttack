using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int playerHP = 100;
    public int playerMaxHP = 100;
    public int currentWave = 1;
    public int maxWave = 3;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            ScoreBoard.Reset();
        }
        else Destroy(gameObject);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 적 포탄과 자폭이 플레이어에게 피해를 줄 때 쓴다
    public void DamagePlayer(int amount)
    {
        if (playerHP <= 0) return;
        playerHP -= amount;
        if (playerHP <= 0)
        {
            playerHP = 0;
            OnPlayerDead();
        }
    }

    public void OnPlayerDead()
    {
        ScoreBoard.Commit(false);
        ChangeScene("Ending_Lose");
    }

    public void OnWaveClear()
    {
        ScoreBoard.Add(200 * currentWave);
        if (currentWave >= maxWave)
        {
            ScoreBoard.Commit(true);
            ChangeScene("Ending_Win");
        }
        else
        {
            currentWave++;
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}