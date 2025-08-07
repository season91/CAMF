using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;

[FirestoreData]
public class StageData
{
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public int chapterNumber { private get; set; }
    [FirestoreProperty] public int stageNumber { private get; set; }
    [FirestoreProperty] public string chapterTitle { private get; set; }
    [FirestoreProperty] public string stageTitle { private get; set; }
    [FirestoreProperty] public Dictionary<string, int> monsterSpawn { private get; set; }  //<monsterCode, Count>
    [FirestoreProperty] public Dictionary<string, int> rewards { private get; set; } // <rewardType, amount>
    [FirestoreProperty] public Dictionary<string, float> randomRewards { private get; set; } // <rewardType, rate>
    [FirestoreProperty] public Dictionary<string, string> firstClearRewards { private get; set; } // <rewardType, itemcode또는amount>
    [FirestoreProperty] public bool isBossStage {private get; set; }

    public string Code => code;
    public string StageTitle => stageTitle;
    public string ChapterTitle => chapterTitle;
    public int ChapterNumber => chapterNumber;
    public int StageNumber => stageNumber;
    public Dictionary<string, int> MonsterSpawn => monsterSpawn;
    public Dictionary<string, int> Rewards => rewards;
    public Dictionary<string, float> RandomRewards => randomRewards;
    public Dictionary<RewardType, string> FirstClearRewards;
    public bool IsBossStage => isBossStage;

    // 정렬된 첫 번째 몬스터 코드 캐시용 필드
    public string ChapterSelectMonsterCode { get; private set; }

    // 로딩 직후 호출해서 캐싱
    public void CacheFirstMonsterCode()
    {
        if (monsterSpawn != null && monsterSpawn.Count > 0)
        {
            var ordered = monsterSpawn.OrderBy(pair => pair.Key).ToList();
            ChapterSelectMonsterCode = ordered[0].Key;

            // monsterSpawn을 정렬된 Dictionary로 덮어쓰기 
            monsterSpawn = ordered.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        else
        {
            ChapterSelectMonsterCode = null;
        }
    }
    
    public void InitParsedFirstClearRewards()
    {
        FirstClearRewards = new Dictionary<RewardType, string>();
        foreach (var kvp in firstClearRewards)
        {
            if (!Enum.TryParse(kvp.Key, out RewardType rewardType))
            {
                MyDebug.LogError($"[RewardType] 잘못된 enum 문자열: {kvp.Key}");
                continue;
            }
            
            FirstClearRewards[rewardType] = kvp.Value;
        }
    }

    /// <summary>
    ///  해당 보상 목록의 Item 개수
    /// </summary>
    public int GetRewardItemCount()
    {
        int count = 0;

        if (FirstClearRewards.ContainsKey(RewardType.Item))
        {
            count++;
        }

        return count;
    }
}
