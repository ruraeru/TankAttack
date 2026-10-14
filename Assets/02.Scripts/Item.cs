using UnityEngine;

public enum ItemType { HP, Speed, Power }

public class Item : MonoBehaviour
{
    public ItemType type;
    public float value = 20f; // Power는 데미지 배율 증가분 (0.25 = +25%)
    public float spinSpeed = 90f;

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (type)
            {
                case ItemType.HP:
                    GameManager gm = GameManager.instance;
                    gm.playerHP = Mathf.Min(gm.playerHP + (int)value, gm.playerMaxHP);
                    break;
                case ItemType.Speed:
                    other.GetComponent<TankMove>().moveSpeed += value;
                    break;
                case ItemType.Power:
                    other.GetComponent<FireCannon>().damageMultiplier += value;
                    break;
            }
            Destroy(gameObject);
        }
    }
}