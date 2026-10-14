using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public FireCannon fireCannon;
    public Image hpFill;
    public Text hpText;
    public Button[] shellButtons;
    public Image[] cooldownOverlays;
    public Text shellInfoText;
    public Image shellIcon;
    public Image airStrikeFill; // Su57 아이콘 (쿨다운이 지날수록 아래에서 차오른다)
    public Text airStrikeTime;

    AirStrike airStrike;

    static readonly Color selectedColor = new Color(0.95f, 0.75f, 0.2f, 1f);
    static readonly Color normalColor = new Color(0.62f, 0.58f, 0.4f, 1f);
    static readonly Color hpHighColor = new Color(0.45f, 0.62f, 0.25f, 1f);
    static readonly Color hpLowColor = new Color(0.8f, 0.2f, 0.15f, 1f);

    void Start()
    {
        // 플레이어가 동적으로 생성되므로 씬에 저장된 참조(프리팹 에셋 등) 대신 생성된 탱크의 FireCannon을 쓴다
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FireCannon fc = player.GetComponent<FireCannon>();
            if (fc != null) fireCannon = fc;
            airStrike = player.GetComponent<AirStrike>();
        }
        if (fireCannon == null)
        {
            enabled = false;
            return;
        }

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
            hpFill.color = Color.Lerp(hpLowColor, hpHighColor, Mathf.Clamp01(ratio * 2f)); // 절반 이하부터 붉게
            hpText.text = "HP " + gm.playerHP + " / " + gm.playerMaxHP;
        }

        if (airStrike != null)
        {
            bool ready = airStrike.RemainingTime <= 0f;
            if (airStrikeFill != null)
            {
                airStrikeFill.fillAmount = 1f - airStrike.CooldownRatio;
                airStrikeFill.color = ready ? Color.white : new Color(0.75f, 0.75f, 0.75f, 1f);
            }
            if (airStrikeTime != null) airStrikeTime.text = ready ? "G" : Mathf.CeilToInt(airStrike.RemainingTime).ToString();
        }

        for (int i = 0; i < shellButtons.Length; i++)
        {
            bool selected = i == fireCannon.CurrentIndex;
            shellButtons[i].image.color = selected ? selectedColor : normalColor;
            cooldownOverlays[i].fillAmount = selected ? fireCannon.CooldownRatio : 0f;
        }

        ShellType shell = fireCannon.Current;
        if (shellInfoText != null)
            shellInfoText.text = shell.name + "\n피해 " + shell.damage + "  범위 " + shell.blastRadius + "m";
        if (shellIcon != null && shell.icon != null) shellIcon.sprite = shell.icon; // 없으면 씬에 지정한 기본 아이콘 유지
    }
}
