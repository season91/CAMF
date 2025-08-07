using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// stage 별 1회성 보상이 있어 관리하기 위해 필요
/// </summary>
[FirestoreData]
public class StageProgress
{
    [FirestoreProperty] public string stageCode { private get; set; }
    [FirestoreProperty] public bool isCleared { private get; set; }
    [FirestoreProperty] public bool rewardClaimed { private get; set; } // 1회성 보상 수령했는지 확인하는 컬럼
    public string StageCode => stageCode;
    public bool IsCleared => isCleared;
    public bool RewardClaimed => rewardClaimed;
}

[FirestoreData]
public class UserStage
{
    public List<StageProgress> progresses { private get; set; } = new List<StageProgress>();
    [FirestoreProperty] public bool initialized { get; set; } = true;

    public List<StageProgress> Progresses => progresses; 
    public async Task SaveStageProgressAsync(StageProgress newProgress)
    {
        // 기존에 해당 스테이지 진척이 있는 경우 
        MyDebug.Log("stageProgresses count : "  + progresses.Count);
        int index = progresses.FindIndex(p => p.StageCode == newProgress.StageCode);
        if (index >= 0)
        {
            MyDebug.Log("SaveStageProgress 있음");
            progresses[index] = newProgress;
        }
        else
        {
            MyDebug.Log("SaveStageProgress <신규생성>");
            progresses.Add(newProgress);
        }

        await FirestoreUploader.SaveUserStageProgress(FirebaseManager.Instance.DbUser.UserId, newProgress);
    }

    public bool IsStageUnlocked(int chapter, int stage)
    {
        if(chapter == 1 && stage == 1)
            return true;
        
        var stageDic = MasterData.StageDataDict;

        StageProgress progress = null;
        
        foreach (StageData stageData in stageDic.Values)
        {
            int prevStage;
            int curChapter;
            if (stage - 1 == 0)
            {
                prevStage = 3;
                curChapter = chapter - 1;
            }
            else
            {
                prevStage = stage - 1;
                curChapter = chapter;
            }
                
            if (stageData.ChapterNumber != curChapter || stageData.StageNumber != prevStage)
            {
                continue;
            }
            
            progress = Progresses.FirstOrDefault(p => p.StageCode == stageData.Code);
            break;
        }
        
        if (progress == null || !progress.IsCleared)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 전달 받은 챕터 1의 스테이지 1,2,3 을 모두 클리어 했는지 확인 
    /// </summary>
    public bool IsChapterAllCleared(int chapter)
    {
        var stageDic = MasterData.StageDataDict;
        
        // 해당 챕터에 속하는 스테이지 코드들을 모두 추출
        var chapterStages = stageDic.Values
                                    .Where(stage => stage.ChapterNumber == chapter)
                                    .Select(stage => stage.Code)
                                    .ToList();
    
        // 해당 챕터에 속하는 모든 스테이지 코드가 Progresses에 있어야 함
        bool allStagesCleared = true;

        // 해당 챕터의 모든 스테이지가 클리어되었는지 확인
        foreach (var stageCode in chapterStages)
        {
            // Progresses에서 해당 stageCode를 찾아서 확인
            var progress = Progresses.FirstOrDefault(p => p.StageCode == stageCode);
        
            // 해당 스테이지 코드가 Progresses에 없다면 false 반환
            if (progress == null || !progress.IsCleared)
            {
                allStagesCleared = false;
                break;
            }
        }

        return allStagesCleared;
    }
}