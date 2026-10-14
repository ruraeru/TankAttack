using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Intro의 탱크 선택 패널. 카탈로그의 탱크마다 카드 템플릿을 복제해 만들고, 카드를 누르면 게임 씬을 연다.
public class TankSelectUI : MonoBehaviour
{
    public TankCatalog catalog;
    public GameObject panel;
    public Button cardTemplate;   // 자식: Icon (Image), Name (Text), Stats (Text)
    public Transform cardParent;
    public string gameSceneName = "MainScene";

    static readonly Color selectedColor = new Color(0.95f, 0.75f, 0.2f, 1f);
    static readonly Color normalColor = new Color(0.62f, 0.58f, 0.4f, 1f);

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (catalog == null || cardTemplate == null) return;

        cardTemplate.gameObject.SetActive(false);
        int selected = TankSelection.Index;
        for (int i = 0; i < catalog.tanks.Length; i++)
        {
            GameObject prefab = catalog.tanks[i];
            if (prefab == null) continue;

            Button card = Instantiate(cardTemplate, cardParent);
            card.gameObject.SetActive(true);
            card.name = "Card_" + prefab.name;
            card.image.color = i == selected ? selectedColor : normalColor;

            TankInfo info = prefab.GetComponent<TankInfo>();
            TankMove move = prefab.GetComponent<TankMove>();
            SetText(card.transform, "Name", info != null ? info.displayName : prefab.name);
            FireCannon fire = prefab.GetComponent<FireCannon>();
            SetText(card.transform, "Stats", "HP " + (info != null ? info.maxHP : 100) + "   속도 " + (move != null ? move.moveSpeed : 0f) + "\n포탄 " + (fire != null ? fire.shells.Length : 0) + "종");
            Transform icon = card.transform.Find("Icon");
            if (icon != null && info != null && info.icon != null) icon.GetComponent<Image>().sprite = info.icon;

            int index = i; // 클로저 캡처용
            card.onClick.AddListener(() => Select(index));
        }
    }

    static void SetText(Transform card, string child, string value)
    {
        Transform t = card.Find(child);
        if (t != null) t.GetComponent<Text>().text = value;
    }

    public void Open()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Select(int index)
    {
        TankSelection.Index = index;
        SceneManager.LoadScene(gameSceneName);
    }
}
