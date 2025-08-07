using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;

[FirestoreData]
public class UnlockTable
{
    [FirestoreProperty] public List<int> Catsite {private set; get;}
    private List<string> stageTitle;
    public List<string> StageTitles => stageTitle;

    /// <summary>
    /// MasterData.StageDataDict를 기준으로 Catsite 챕터의 보스 스테이지 타이틀 초기화
    /// </summary>
    public void InitStageTitle()
    {
        stageTitle = new List<string>();

        // Dictionary<int, StageData>: ChapterNumber가 key, IsBossStage인 StageData가 value로 줄여놓기
        var bossStageDict = MasterData.StageDataDict.Values
                                      .Where(stage => stage.IsBossStage)
                                      .ToDictionary(stage => stage.ChapterNumber, stage => stage);

        // Catsite에서 챕터 번호를 순회하면서 title 목록 구성
        foreach (int chapter in Catsite)
        {
            if (bossStageDict.TryGetValue(chapter, out var stageData))
            {
                stageTitle.Add(stageData.StageTitle);
            }
        }
    }

}