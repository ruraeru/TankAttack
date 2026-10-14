using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ending_Win / Ending_Lose 씬에 빈 오브젝트로 붙인다. UI는 실행 시 코드로 만든다.
public class EndingUI : MonoBehaviour
{
    public bool win;
    public string gameSceneName = "MainScene";
    public string introSceneName = "IntroScene";

    static readonly Color Olive = new Color(0.33f, 0.38f, 0.2f, 1f);

    void Start()
    {
        Time.timeScale = 1f;
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = win ? new Color(0.12f, 0.16f, 0.1f) : new Color(0.18f, 0.08f, 0.08f);
        }

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObj = new GameObject("EndingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        Transform root = canvasObj.transform;
        MakeText(root, win ? "승리!" : "패배...", 140, new Vector2(0, 300), win ? new Color(0.9f, 0.95f, 0.5f) : new Color(0.95f, 0.45f, 0.4f));
        MakeText(root, "점수  " + ScoreBoard.Score, 80, new Vector2(0, 100), Color.white);
        MakeText(root, "최고 기록  " + ScoreBoard.Best, 56, new Vector2(0, 0), new Color(0.85f, 0.85f, 0.85f));
        if (ScoreBoard.NewRecord)
            MakeText(root, "NEW RECORD!", 64, new Vector2(0, -90), new Color(1f, 0.85f, 0.2f));

        MakeButton(root, "다시 하기", new Vector2(-220, -280), () => SceneManager.LoadScene(gameSceneName));
        MakeButton(root, "처음으로", new Vector2(220, -280), () => SceneManager.LoadScene(introSceneName));
    }

    static Text MakeText(Transform parent, string content, int size, Vector2 pos, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = content;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform rt = t.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1400, size * 1.4f);
        return t;
    }

    static void MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject("Button_" + label);
        go.transform.SetParent(parent, false);
        UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = Olive;
        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(onClick);
        RectTransform rt = img.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(380, 110);
        Text t = MakeText(go.transform, label, 48, Vector2.zero, Color.white);
        t.rectTransform.sizeDelta = rt.sizeDelta;
    }
}
