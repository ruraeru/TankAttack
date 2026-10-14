using UnityEngine;
using UnityEngine.UI;

// Intro 메인 메뉴: 출격(탱크 선택), 조작법, 설정, 종료. 패널은 하나씩만 연다.
public class IntroMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject controlsPanel;
    public GameObject settingsPanel;
    public TankSelectUI tankSelect;
    public Text bestText;
    public Slider volumeSlider;
    public Text volumeText;

    void Start()
    {
        ShowMain();
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(GameSettings.Volume);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        RefreshTexts();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ShowMain();
    }

    void CloseAll()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tankSelect != null) tankSelect.Close();
    }

    public void ShowMain()
    {
        CloseAll();
        if (mainMenu != null) mainMenu.SetActive(true);
        RefreshTexts();
    }

    public void OpenTankSelect()
    {
        CloseAll();
        if (tankSelect != null) tankSelect.Open();
    }

    public void OpenControls()
    {
        CloseAll();
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAll();
        if (settingsPanel != null) settingsPanel.SetActive(true);
        RefreshTexts();
    }

    public void OnVolumeChanged(float v)
    {
        GameSettings.Volume = v;
        RefreshTexts();
    }

    public void ResetBest()
    {
        ScoreBoard.ResetBest();
        RefreshTexts();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void RefreshTexts()
    {
        if (bestText != null) bestText.text = "최고 기록  " + ScoreBoard.Best;
        if (volumeText != null) volumeText.text = Mathf.RoundToInt(GameSettings.Volume * 100f) + "%";
    }
}
