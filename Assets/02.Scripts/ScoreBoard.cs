using UnityEngine;

// 점수와 최고 기록. static이라 씬이 바뀌어도 유지된다. 최고 기록만 PlayerPrefs에 저장한다.
public static class ScoreBoard
{
    const string BestKey = "BestScore";

    public static int Score { get; private set; }
    public static int Best => PlayerPrefs.GetInt(BestKey, 0);
    public static bool LastWon { get; private set; }
    public static bool NewRecord { get; private set; }

    public static void Reset()
    {
        Score = 0;
        NewRecord = false;
    }

    public static void Add(int points)
    {
        Score += points;
    }

    public static void ResetBest()
    {
        PlayerPrefs.DeleteKey(BestKey);
        PlayerPrefs.Save();
    }

    // 엔딩으로 넘어가기 직전에 한 번 부른다.
    public static void Commit(bool won)
    {
        LastWon = won;
        NewRecord = Score > Best;
        if (NewRecord)
        {
            PlayerPrefs.SetInt(BestKey, Score);
            PlayerPrefs.Save();
        }
    }
}
