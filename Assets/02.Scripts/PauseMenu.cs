using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    public string introSceneName = "IntroScene";

    void Start()
    {
        Time.timeScale = 1f;
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SetPaused(!panel.activeSelf);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    public void SetPaused(bool paused)
    {
        panel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void Pause() { SetPaused(true); }

    public void Resume() { SetPaused(false); }

    public void Restart()
    {
        Time.timeScale = 1f;
        if (GameManager.instance != null) GameManager.instance.RestartGame();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToIntro()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(introSceneName);
    }
}
