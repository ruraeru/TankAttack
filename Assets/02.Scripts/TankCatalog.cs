using UnityEngine;

// 플레이어가 고를 수 있는 탱크 프리팹 목록. Intro 선택 화면과 PlayerSpawner가 같은 에셋을 쓴다.
// 새 탱크는 프리팹 루트에 TankInfo를 붙이고 이 목록에 추가하면 된다.
[CreateAssetMenu(fileName = "TankCatalog", menuName = "TankAttack/Tank Catalog")]
public class TankCatalog : ScriptableObject
{
    public GameObject[] tanks;

    public GameObject Get(int index)
    {
        if (tanks == null || tanks.Length == 0) return null;
        if (index < 0 || index >= tanks.Length) index = 0;
        return tanks[index];
    }
}
