using UnityEngine;

// 탱크 프리팹 루트에 붙이는 표시 정보와 탱크별 능력치.
// 이동 속도는 TankMove.moveSpeed, 포탄은 FireCannon.shells에 있다.
public class TankInfo : MonoBehaviour
{
    public string displayName = "Tank";
    [TextArea] public string description;
    public Sprite icon;
    public int maxHP = 100;
}
