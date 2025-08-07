#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class MasterDataHandler
{
    public static async Task AddToMasterData()
    {
        // Gacha Table
        GachaTable gachaTable = await FirestoreHelper.GetTableDocumentByDocid<GachaTable>(FirestoreDocument.Gacha);
        MasterData.SetGachaTable(gachaTable.GachaRates);
        MasterData.SetGachaPityLegendChances(gachaTable.GachaPityLegendChances);
        MasterData.SetGachaPityIncreases(gachaTable.GachaPityIncreases);
        MasterData.SetGachaPityThresholds(gachaTable.GachaPityThresholds);
        MasterData.SetGachaCosts(gachaTable.GachaCosts);
        
        // UnLock Table
        UnlockTable unlockTable = await FirestoreHelper.GetTableDocumentByDocid<UnlockTable>(FirestoreDocument.UnLock);
        MasterData.SetUnlockTable(unlockTable);
        
        // ItemData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<ItemData>(FirestoreCollection.Item),
            MasterData.SetItemData,
            GetItemCode,
            InitItemParsedStats
        );

        // GachaItemData
        await DictLoadAndSet(
            FirestoreHelper.GetGachaEquipItems,
            MasterData.SetGachaItemData,
            item => item.Code
        );

        // StageData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<StageData>(FirestoreCollection.Stage),
            MasterData.SetStageData,
            stage => stage.Code,
            stage => stage.InitParsedFirstClearRewards()
        );

        // SkillData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<SkillData>(FirestoreCollection.Skill),
            MasterData.SetSkillData,
            skill => skill.EntityCode
        );

        // MonsterData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<MonsterData>(FirestoreCollection.Monster),
            MasterData.SetMonsterData,
            monster => monster.Code
        );

        // UnitData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<UnitData>(FirestoreCollection.Unit),
            MasterData.SetUnitData,
            unit => unit.Code
        );

        // CollectionData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<CollectionData>(FirestoreCollection.Collection),
            MasterData.SetCollectionData,
            col => col.Code
        );

        // LevelUpExpData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<LevelUpExpData>(FirestoreCollection.LevelUpExp),
            MasterData.SetLevelUpExpData,
            lvl => lvl.Level
        );

        // EnhancementData
        await DictLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<EnhancementData>(FirestoreCollection.Enhancement),
            MasterData.SetEnhancementData,
            enh => enh.EnhancementLevel
        );

        // LimitBreakData
        await ListLoadAndSet(
            () => FirestoreHelper.GetRowDataDocumentByColl<LimitBreakData>(FirestoreCollection.LimitBreak),
            MasterData.SetLimitBreakData,
            limitBreak => limitBreak.LimitBreakLevel, // LimitBreakData의 LimitBreakLevel을 키로 사용,
            InitStatIncreaseValue
        );
        
        // ItemDismatleData
        MasterData.SetItemDismantleData(await FirestoreHelper.GetRowDataDocumentByColl<ItemDismantleData>(FirestoreCollection.ItemDismantle));
        
        // 후처리
        InitUnlockTable(unlockTable);
    }
    
    private static async Task DictLoadAndSet<TSource, TKey>(
        Func<Task<List<TSource>>> fetchFunc,
        Action<IEnumerable<TSource>> setter,
        Func<TSource, TKey> keySelector,
        Action<TSource>? initializer = null)
    {
        List<TSource> dataList = await fetchFunc();
        foreach (var data in dataList)
            initializer?.Invoke(data);
        setter(dataList);
    }

    private static async Task ListLoadAndSet<TSource>(
        Func<Task<List<TSource>>> fetchFunc,         // 데이터를 가져오는 비동기 함수
        Action<IEnumerable<TSource>> setter,         // 데이터를 설정하는 함수
        Func<TSource, object> keySelector,           // 각 항목의 키를 추출하는 함수
        Action<TSource>? initializer = null)         // 각 항목에 대한 초기화 함수
    {
        List<TSource> dataList = await fetchFunc();  // 데이터 비동기적으로 가져오기
        foreach (var data in dataList)
            initializer?.Invoke(data);  // 초기화 작업이 필요하면 실행
        setter(dataList);  // 데이터를 설정
    }
    
    private static string GetItemCode(ItemData item) => item.Code;
    private static void InitItemParsedStats(ItemData item) => item.InitParsedStats();
    private static void InitStatIncreaseValue(LimitBreakData limitBreak) => limitBreak.InitStatIncreaseValue();

    private static void InitUnlockTable(UnlockTable unlock)
    {
        MyDebug.Log("InitUnlockTable");
        unlock.InitStageTitle();
    }
}
