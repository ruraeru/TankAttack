using UnityEngine;

// 사용자 설정 (지금은 전체 볼륨). PlayerPrefs에 저장하고 게임이 켜질 때 적용한다.
public static class GameSettings
{
    const string VolumeKey = "MasterVolume";

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, 1f);
        set
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, v);
            PlayerPrefs.Save();
            AudioListener.volume = v;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Apply()
    {
        AudioListener.volume = Volume;
    }
}
