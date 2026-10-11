using UnityEngine;

public enum ItemType { HP, Speed, Power }

public class Item : MonoBehaviour
{
    public ItemType type;
    public float value = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (type)
            {
                case ItemType.HP:
                    GameManager.instance.playerHP += (int)value;
                    break;
                case ItemType.Speed:
                    other.GetComponent<TankMove>().moveSpeed += value;
                    break;
            }
            Destroy(gameObject);
        }
    }
}