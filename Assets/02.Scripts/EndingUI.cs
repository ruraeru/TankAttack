using System.Collections;
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
    public Sprite headerIcon;
    public Sprite retryIcon;
    public Sprite homeIcon;
    public Sprite recordIcon;

    static readonly Color Olive = new Color(0.33f, 0.38f, 0.2f, 1f);
    static readonly Color OliveDark = new Color(0.16f, 0.19f, 0.1f, 0.92f);

    CanvasGroup panelGroup;
    Text scoreText;

    void Start()
    {
        Build();
    }

    public Canvas Build()
    {
        Time.timeScale = 1f;
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = win ? new Color(0.08f, 0.11f, 0.07f) : new Color(0.14f, 0.06f, 0.06f);
        }

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        Color accent = win ? new Color(0.9f, 0.95f, 0.5f) : new Color(0.95f, 0.45f, 0.4f);

        GameObject canvasObj = new GameObject("EndingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        Transform root = canvasObj.transform;

        // 가장자리를 어둡게 깔고 가운데 패널을 둔다
        MakeImage(root, "Dim", null, new Color(0, 0, 0, 0.35f), Vector2.zero, new Vector2(4000, 3000));
        GameObject panel = MakeImage(root, "Panel", null, OliveDark, new Vector2(0, 0), new Vector2(1100, 760)).gameObject;
        panelGroup = panel.AddComponent<CanvasGroup>();
        panelGroup.alpha = 0f;
        MakeImage(panel.transform, "TopBar", null, accent, new Vector2(0, 370), new Vector2(1100, 20));
        MakeImage(panel.transform, "BottomBar", null, accent, new Vector2(0, -370), new Vector2(1100, 20));

        Transform p = panel.transform;
        if (headerIcon != null)
            MakeImage(p, "HeaderIcon", headerIcon, Color.white, new Vector2(0, 270), new Vector2(100, 100));
        MakeText(p, win ? "승리!" : "패배...", 88, new Vector2(0, 150), accent);
        MakeText(p, win ? "모든 웨이브를 막아냈다" : "전차가 파괴되었다", 34, new Vector2(0, 62), new Color(0.8f, 0.82f, 0.7f));
        MakeImage(p, "Divider", null, new Color(1, 1, 1, 0.2f), new Vector2(0, 10), new Vector2(800, 3));
        scoreText = MakeText(p, "점수  0", 64, new Vector2(0, -50), Color.white);
        MakeText(p, "최고 기록  " + ScoreBoard.Best, 40, new Vector2(0, -125), new Color(0.85f, 0.85f, 0.85f));
        if (ScoreBoard.NewRecord)
        {
            Text nr = MakeText(p, "NEW RECORD!", 44, new Vector2(0, -185), new Color(1f, 0.85f, 0.2f));
            StartCoroutine(Pulse(nr.rectTransform));
        }

        MakeButton(p, "다시 하기", retryIcon, new Vector2(-230, -290), () => SceneManager.LoadScene(gameSceneName));
        MakeButton(p, "처음으로", homeIcon, new Vector2(230, -290), () => SceneManager.LoadScene(introSceneName));

        StartCoroutine(Intro(panel.GetComponent<RectTransform>()));
        return canvas;
    }

    IEnumerator Intro(RectTransform panel)
    {
        float t = 0f;
        const float fade = 0.6f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / fade);
            panelGroup.alpha = k;
            panel.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, k);
            yield return null;
        }
        panelGroup.alpha = 1f;
        panel.localScale = Vector3.one;

        // 점수 올라가는 연출
        int target = ScoreBoard.Score;
        float dur = Mathf.Clamp(target / 1500f, 0.6f, 1.8f);
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            scoreText.text = "점수  " + Mathf.RoundToInt(Mathf.Lerp(0, target, t / dur));
            yield return null;
        }
        scoreText.text = "점수  " + target;
    }

    IEnumerator Pulse(RectTransform rt)
    {
        while (true)
        {
            float s = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 5f);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    static UnityEngine.UI.Image MakeImage(Transform parent, string name, Sprite sprite, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        img.rectTransform.anchoredPosition = pos;
        img.rectTransform.sizeDelta = size;
        return img;
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
        t.raycastTarget = false;
        RectTransform rt = t.rectTransform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1000, size * 1.4f);
        return t;
    }

    static void MakeButton(Transform parent, string label, Sprite icon, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        UnityEngine.UI.Image img = MakeImage(parent, "Button_" + label, null, Olive, pos, new Vector2(380, 100));
        img.raycastTarget = true;
        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        b.colors = cb;
        b.onClick.AddListener(onClick);

        float textX = 0f;
        if (icon != null)
        {
            MakeImage(img.transform, "Icon", icon, Color.white, new Vector2(-130, 0), new Vector2(56, 56));
            textX = 30f;
        }
        Text t = MakeText(img.transform, label, 40, new Vector2(textX, 0), Color.white);
        t.rectTransform.sizeDelta = new Vector2(260, 100);
    }
}
