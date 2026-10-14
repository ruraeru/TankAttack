using UnityEngine;

// 고른 탱크 번호 (TankCatalog.tanks의 인덱스). 다시 하기와 다음 실행에도 유지되게 PlayerPrefs에 저장한다.
public static class TankSelection
{
    const string Key = "SelectedTank";

    public static int Index
    {
        get => PlayerPrefs.GetInt(Key, 0);
        set
        {
            PlayerPrefs.SetInt(Key, value);
            PlayerPrefs.Save();
        }
    }
}
