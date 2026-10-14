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
    public float buttonSpacing = 185f;
    public Image shellIcon;
    public Image airStrikeFill; // Su57 아이콘 (쿨다운이 지날수록 아래에서 차오른다)
    public Text airStrikeTime;
    public Image boostFill;     // Shift 가속 게이지 (가속 배율이 오른 만큼 찬다)
    public Text boostText;

    AirStrike airStrike;
    TankMove tankMove;
    Rigidbody playerBody;

    static readonly Color selectedColor = new Color(0.95f, 0.75f, 0.2f, 1f);
    static readonly Color normalColor = new Color(0.62f, 0.58f, 0.4f, 1f);
    static readonly Color hpHighColor = new Color(0.45f, 0.62f, 0.25f, 1f);
    static readonly Color boostLowColor = new Color(0.95f, 0.75f, 0.2f, 1f);
    static readonly Color boostHighColor = new Color(0.95f, 0.35f, 0.1f, 1f);
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
            tankMove = player.GetComponent<TankMove>();
            playerBody = player.GetComponent<Rigidbody>();
        }
        if (fireCannon == null)
        {
            enabled = false;
            return;
        }

        // 탱크마다 포탄 수가 달라서, 있는 포탄만큼 버튼을 켜고 가운데 정렬한다
        int count = Mathf.Min(shellButtons.Length, fireCannon.shells.Length);
        for (int i = 0; i < shellButtons.Length; i++)
        {
            bool used = i < count;
            shellButtons[i].gameObject.SetActive(used);
            if (!used) continue;
            RectTransform rt = (RectTransform)shellButtons[i].transform;
            rt.anchoredPosition = new Vector2((i - (count - 1) * 0.5f) * buttonSpacing, rt.anchoredPosition.y);

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

        if (tankMove != null)
        {
            float range = tankMove.boostMultiplier - 1f;
            float boost = range > 0f ? Mathf.Clamp01((tankMove.Boost - 1f) / range) : 0f;
            if (boostFill != null)
            {
                boostFill.fillAmount = boost;
                boostFill.color = Color.Lerp(boostLowColor, boostHighColor, boost);
            }
            if (boostText != null)
            {
                int speed = playerBody != null ? Mathf.RoundToInt(new Vector3(playerBody.linearVelocity.x, 0f, playerBody.linearVelocity.z).magnitude) : 0;
                boostText.text = (boost > 0.01f ? "가속 x" + tankMove.Boost.ToString("0.0") : "가속 [Shift]") + "    속도 " + speed;
            }
        }

        for (int i = 0; i < shellButtons.Length && i < fireCannon.shells.Length; i++)
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
