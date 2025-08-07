using System.Threading.Tasks;

public static class UserDataHandler
{
    #region 조회

    /// <summary>
    /// user data 조회
    /// </summary>
    public static async Task LoadToUserData(string uid)
    {
        await FirestoreHelper.GetUserDataByUserId(uid);
        await FirestoreHelper.GetAccountDataByUserId(uid);
        InventoryManager.Instance.Initialize();
    }

    #endregion
    
    #region 저장

    /// <summary>
    /// info 경험치 반영 후 저장 
    /// </summary>
    public static async Task<bool> ChangeExp(int amount)
    {
        return await UserData.info.GainExpAsync(amount);
    }
    /// <summary>
    /// 스테이지 진척도 검증 후 저장 
    /// </summary>
    public static async Task SaveStageProgress(StageProgress stageProgress)
    {
        await UserData.stage.SaveStageProgressAsync(stageProgress);
    }

    #endregion
}
