using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class ShellType
{
    public string name;
    public float speed;
    public float damage;
    public float blastRadius;
    public float cooldown;
    public GameObject shellPrefab;      // 비우면 Resources/Cannon 사용
    public GameObject explosionPrefab;  // 비우면 포탄 프리팹의 expEffect 사용
    public Sprite icon;
}

public class FireCannon : MonoBehaviour
{
    public GameObject cannon;
    public Transform firePos;
    public ShellType[] shells =
    {
        new ShellType { name = "일반탄", speed = 6000f, damage = 30f, blastRadius = 3f, cooldown = 0.8f },
        new ShellType { name = "고속탄", speed = 12000f, damage = 15f, blastRadius = 1.5f, cooldown = 0.4f },
        new ShellType { name = "중포탄", speed = 4000f, damage = 60f, blastRadius = 6f, cooldown = 2f },
    };
    public float damageMultiplier = 1f; // Power 아이템이 올린다
    public int CurrentIndex { get; private set; }
    public ShellType Current => shells[CurrentIndex];
    public float CooldownRatio
    {
        get
        {
            float cd = Current.cooldown;
            return cd <= 0f ? 0f : Mathf.Clamp01((nextFireTime - Time.time) / cd);
        }
    }

    AudioClip fireSfx;
    AudioSource sfx;
    float nextFireTime;

    private void Awake()
    {
        cannon = (GameObject)Resources.Load("Cannon");
        fireSfx = Resources.Load<AudioClip>("CannonFire");
        sfx = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 숫자 키 1~9로 포탄 선택 (탱크마다 포탄 수가 다르다)
        for (int i = 0; i < shells.Length && i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SelectShell(i);

        if (Input.GetMouseButtonDown(0) && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            Fire();
        }
    }

    public void SelectShell(int index)
    {
        if (index < 0 || index >= shells.Length) return;
        CurrentIndex = index;
    }

    void Fire()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + Current.cooldown;

        sfx.PlayOneShot(fireSfx, 1.0f);
        GameObject prefab = Current.shellPrefab != null ? Current.shellPrefab : cannon;
        GameObject obj = Instantiate(prefab, firePos.position, firePos.rotation);
        Cannon c = obj.GetComponent<Cannon>();
        if (Current.explosionPrefab != null) c.expEffect = Current.explosionPrefab;
        c.speed = Current.speed;
        c.damage = Current.damage * damageMultiplier;
        c.blastRadius = Current.blastRadius;
    }
}
