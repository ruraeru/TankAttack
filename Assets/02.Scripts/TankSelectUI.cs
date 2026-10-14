using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Intro의 탱크 선택 패널. 카탈로그의 탱크마다 카드를 만들고, 카드를 누르면 정보와 3D 미리보기(IntroShowcase)를 바꾼다.
// 출격 버튼을 누르면 고른 탱크를 저장하고 게임 씬을 연다.
public class TankSelectUI : MonoBehaviour
{
    public TankCatalog catalog;
    public GameObject panel;
    public Button cardTemplate;   // 자식: Icon (Image), Name (Text)
    public Transform cardParent;
    public IntroShowcase showcase;
    public string gameSceneName = "MainScene";

    [Header("정보 패널")]
    public Text nameText;
    public Text descText;
    public Image hpBar;
    public Text hpValue;
    public Image speedBar;
    public Text speedValue;
    public Image powerBar;
    public Text powerValue;
    public Text weaponText;

    [Header("막대 최댓값")]
    public float hpScale = 200f;
    public float speedScale = 30f;
    public float powerScale = 100f; // 가장 센 포탄의 피해량 기준

    static readonly Color selectedColor = new Color(0.95f, 0.75f, 0.2f, 1f);
    static readonly Color normalColor = new Color(0.62f, 0.58f, 0.4f, 1f);

    Button[] cards;
    int current;

    void Start()
    {
        if (catalog == null || cardTemplate == null) return;

        cardTemplate.gameObject.SetActive(false);
        cards = new Button[catalog.tanks.Length];
        for (int i = 0; i < catalog.tanks.Length; i++)
        {
            GameObject prefab = catalog.tanks[i];
            if (prefab == null) continue;

            Button card = Instantiate(cardTemplate, cardParent);
            card.gameObject.SetActive(true);
            card.name = "Card_" + prefab.name;
            TankInfo info = prefab.GetComponent<TankInfo>();
            Transform nameT = card.transform.Find("Name");
            if (nameT != null) nameT.GetComponent<Text>().text = info != null ? info.displayName : prefab.name;
            Transform icon = card.transform.Find("Icon");
            if (icon != null && info != null && info.icon != null) icon.GetComponent<Image>().sprite = info.icon;

            int index = i; // 클로저 캡처용
            card.onClick.AddListener(() => Preview(index));
            cards[i] = card;
        }
        current = Mathf.Clamp(TankSelection.Index, 0, catalog.tanks.Length - 1);
        Preview(current);
    }

    public void Open()
    {
        if (panel != null) panel.SetActive(true);
        if (cards != null) Preview(current);
    }

    public void Close()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Preview(int index)
    {
        if (catalog == null) return;
        current = index;
        for (int i = 0; cards != null && i < cards.Length; i++)
            if (cards[i] != null) cards[i].image.color = i == index ? selectedColor : normalColor;

        GameObject prefab = catalog.Get(index);
        if (prefab == null) return;
        TankInfo info = prefab.GetComponent<TankInfo>();
        TankMove move = prefab.GetComponent<TankMove>();
        FireCannon fire = prefab.GetComponent<FireCannon>();

        int hp = info != null ? info.maxHP : 100;
        float speed = move != null ? move.moveSpeed : 0f;
        float power = 0f;
        string weapons = "";
        if (fire != null)
            foreach (ShellType s in fire.shells)
            {
                power = Mathf.Max(power, s.damage);
                weapons += (weapons.Length > 0 ? ", " : "") + s.name;
            }

        if (nameText != null) nameText.text = info != null ? info.displayName : prefab.name;
        if (descText != null) descText.text = info != null ? info.description : "";
        SetBar(hpBar, hpValue, hp, hpScale);
        SetBar(speedBar, speedValue, speed, speedScale);
        SetBar(powerBar, powerValue, power, powerScale);
        if (weaponText != null) weaponText.text = "포탄  " + weapons + "\n공습  [G]";
        if (showcase != null) showcase.Show(index);
    }

    static void SetBar(Image bar, Text value, float v, float scale)
    {
        if (bar != null) bar.fillAmount = Mathf.Clamp01(v / scale);
        if (value != null) value.text = Mathf.RoundToInt(v).ToString();
    }

    public void StartGame()
    {
        TankSelection.Index = current;
        SceneManager.LoadScene(gameSceneName);
    }
}
