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
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnPlayerDead()
    {
        ChangeScene("Ending_Lose");
    }

    public void OnWaveClear()
    {
        if (currentWave >= maxWave)
        {
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