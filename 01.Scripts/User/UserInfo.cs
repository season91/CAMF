using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Player 전용 Stat
/// </summary>
///
[FirestoreData]
public class UserInfo
{
    // 레벨존재
    [FirestoreProperty] public int level { private get; set; } = 1;

    [FirestoreProperty] public int currentExp {private get; set;}

    private int ExpToNextLevel => MasterData.LevelUpExpDataDict[level].ExpToNextLevel;
    private bool CanLevelUp => currentExp >= ExpToNextLevel;
    
    public int Level => level;
    
    // 경험치를 더하고, 레벨업 여부 및 보상을 결과로 반환
    public async Task<bool> GainExpAsync(int exp)
    {
        AddExp(exp);

        bool isLevelUp = false;

        if (CanLevelUp)
        {
            DoLevelUp();
            isLevelUp = true;
        }

        // DB저장은 한군데서
        await FirestoreUploader.SaveDataUserInfo(FirebaseManager.Instance.DbUser.UserId, this);
        return isLevelUp;
    }
    
    //경험치 획득
    private void AddExp(int exp)
    {
        currentExp += exp;
        // await LevelUp();
        // await FirestoreUploader.SaveDataUserInfo(FirebaseManager.Instance.DbUser.UserId, this);
    }
    
    public void DoLevelUp()
    {
        MyDebug.Log(+level+ " ! DoLevelUp " + ExpToNextLevel );
        currentExp -= ExpToNextLevel;
        level++;
    }

    // 현재 레벨에서 다음 레벨까지의 경험치 진행 비율을 계산하는 함수
    public float GetExpRatio()
    {
        if(MasterData.LevelUpExpDataDict.TryGetValue(Level, out LevelUpExpData levelUpExpData))
        {
            return (float)currentExp / levelUpExpData.ExpToNextLevel;
        }
        return 0f;
    }
    
    // public async Task LevelUp()
    // {
    //     if (currentExp >= GetExpToNextLevel(level))
    //     {
    //         currentExp -= GetExpToNextLevel(level);
    //         level++;
    //         
    //         MyDebug.Log($"레벨 상승! 레벨 : {level}");
    //         MyDebug.Log($"현재 경험치 :{currentExp} 필요 경험치 : {GetExpToNextLevel(level)}");
    //         await RewardManager.Instance.GiveLevelUpReward();
    //         UIManager.Instance.UpdateUserExpGauge();
    //     }
    // }
    
    // private int GetExpToNextLevel(int currentLevel)
    // {
    //     return 48*(currentLevel*currentLevel)+ 100 * currentLevel + -48;
    // }
    
}
