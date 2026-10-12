using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public FireCannon fireCannon;
    public Image hpFill;
    public Text hpText;
    public Button[] shellButtons;
    public Image[] cooldownOverlays;

    static readonly Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f);
    static readonly Color normalColor = new Color(1f, 1f, 1f, 0.6f);

    void Start()
    {
        for (int i = 0; i < shellButtons.Length; i++)
        {
            int index = i; // 클로저 캡처용
            shellButtons[i].onClick.AddListener(() => fireCannon.SelectShell(index));
            Text label = shellButtons[i].GetComponentInChildren<Text>();
            if (label != null) label.text = (i + 1) + ". " + fireCannon.shells[i].name;
        }
    }

    void Update()
    {
        GameManager gm = GameManager.instance;
        if (gm != null)
        {
            float ratio = gm.playerMaxHP > 0 ? (float)gm.playerHP / gm.playerMaxHP : 0f;
            hpFill.fillAmount = Mathf.Clamp01(ratio);
            hpText.text = "HP " + gm.playerHP + " / " + gm.playerMaxHP;
        }

        for (int i = 0; i < shellButtons.Length; i++)
        {
            bool selected = i == fireCannon.CurrentIndex;
            shellButtons[i].image.color = selected ? selectedColor : normalColor;
            cooldownOverlays[i].fillAmount = selected ? fireCannon.CooldownRatio : 0f;
        }
    }
}
