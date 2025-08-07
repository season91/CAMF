#nullable enable
using System;
using System.Collections.Generic;

public static class MasterData
{
    // 가챠 테이블
    private static readonly Dictionary<ResourceType, Dictionary<ItemRarity, float>> gachaTableByType = new ();
    private static readonly Dictionary<ResourceType, float> gachaPityLegendChances = new ();
    private static readonly Dictionary<ResourceType, float> gachaPityIncreases = new();
    private static readonly Dictionary<ResourceType, int> gachaPityThresholds = new();
    private static readonly Dictionary<ResourceType, int> gachaCosts = new();
    
    // 언락 테이블
    private static UnlockTable unlockTable = new ();
    
    // key: Code
    private static readonly Dictionary<string, ItemData> itemDataDict = new ();
    private static readonly Dictionary<string, ItemData> gachaItemDataDict = new ();
    private static readonly Dictionary<string, StageData> stageDataDict = new();  // Code: S_0000
    private static readonly Dictionary<string, SkillData> skillDataDict = new();
    private static readonly Dictionary<string, MonsterData> monsterDataDict = new();
    private static readonly Dictionary<string, UnitData> unitDataDict = new();
    private static readonly Dictionary<string, CollectionData> collectionDataDict = new();
    private static readonly Dictionary<int, LevelUpExpData> levelUpExpDataDict = new();
    private static readonly Dictionary<int, EnhancementData> enhancementDataDict = new();
    private static readonly List<LimitBreakData> limitBreakData = new();
    private static readonly Dictionary<ItemRarity, ItemDismantleData> itemDismantleDataDic = new();
    
    // 가챠 테이블
    public static IReadOnlyDictionary<ResourceType, Dictionary<ItemRarity, float>> GachaTableByType => gachaTableByType;
    public static IReadOnlyDictionary<ResourceType, float> GachaPityLegendChances => gachaPityLegendChances;
    public static IReadOnlyDictionary<ResourceType, float> GachaPityIncreases => gachaPityIncreases;
    public static IReadOnlyDictionary<ResourceType, int> GachaPityThresholds => gachaPityThresholds;
    public static IReadOnlyDictionary<ResourceType, int> GachaCosts => gachaCosts;
    
    // 언락 테이블
    public static UnlockTable UnlockTable => unlockTable;
    
    // key: Code
    public static IReadOnlyDictionary<string, ItemData> ItemDataDict => itemDataDict;
    public static IReadOnlyDictionary<string, ItemData> GachaItemDataDict => gachaItemDataDict;
    public static IReadOnlyDictionary<string, StageData> StageDataDict => stageDataDict;  // Code: S_0000
    public static IReadOnlyDictionary<string, SkillData> SkillDataDict => skillDataDict;
    public static IReadOnlyDictionary<string, MonsterData> MonsterDataDict => monsterDataDict;
    public static IReadOnlyDictionary<string, UnitData> UnitDataDict => unitDataDict;
    public static IReadOnlyDictionary<string, CollectionData> CollectionDataDict => collectionDataDict;
    public static IReadOnlyDictionary<int, LevelUpExpData> LevelUpExpDataDict => levelUpExpDataDict;
    public static IReadOnlyDictionary<int, EnhancementData> EnhancementDataDict => enhancementDataDict;
    public static IReadOnlyList<LimitBreakData> LimitBreakData => limitBreakData;

    public static IReadOnlyDictionary<ItemRarity, ItemDismantleData> ItemDismantleDataDic => itemDismantleDataDic;
    
    internal static void SetGachaTable(Dictionary<ResourceType, Dictionary<ItemRarity, float>> table)
    {
        gachaTableByType.Clear();
        foreach (var kvp in table)
            gachaTableByType[kvp.Key] = kvp.Value;
    }

    internal static void SetGachaPityLegendChances(Dictionary<ResourceType, float> data)
    {
        gachaPityLegendChances.Clear();
        foreach (var kvp in data)
            gachaPityLegendChances[kvp.Key] = kvp.Value;
    }

    internal static void SetGachaPityIncreases(Dictionary<ResourceType, float> data)
    {
        gachaPityIncreases.Clear();
        foreach (var kvp in data)
            gachaPityIncreases[kvp.Key] = kvp.Value;
    }

    internal static void SetGachaPityThresholds(Dictionary<ResourceType, int> data)
    {
        gachaPityThresholds.Clear();
        foreach (var kvp in data)
            gachaPityThresholds[kvp.Key] = kvp.Value;
    }

    internal static void SetGachaCosts(Dictionary<ResourceType, int> data)
    {
        gachaCosts.Clear();
        foreach (var kvp in data)
            gachaCosts[kvp.Key] = kvp.Value;
    }
    
    internal static void SetUnlockTable(UnlockTable unlock)
    {
        unlockTable = unlock;
    }
    
    internal static void SetItemData(IEnumerable<ItemData> items) =>
        ReplaceDict(itemDataDict, items, item => item.Code, item => item.InitParsedStats());

    internal static void SetGachaItemData(IEnumerable<ItemData> items) =>
        ReplaceDict(gachaItemDataDict, items, item => item.Code);

    internal static void SetStageData(IEnumerable<StageData> stages) =>
        ReplaceDict(stageDataDict, stages, stage => stage.Code, stage => stage.InitParsedFirstClearRewards());

    internal static void SetSkillData(IEnumerable<SkillData> skills) =>
        ReplaceDict(skillDataDict, skills, skill => skill.EntityCode);

    internal static void SetMonsterData(IEnumerable<MonsterData> monsters) =>
        ReplaceDict(monsterDataDict, monsters, monster => monster.Code);

    internal static void SetUnitData(IEnumerable<UnitData> units) =>
        ReplaceDict(unitDataDict, units, unit => unit.Code);

    internal static void SetCollectionData(IEnumerable<CollectionData> collections) =>
        ReplaceDict(collectionDataDict, collections, col => col.Code);

    internal static void SetLevelUpExpData(IEnumerable<LevelUpExpData> levels) =>
        ReplaceDict(levelUpExpDataDict, levels, lvl => lvl.Level);

    internal static void SetEnhancementData(IEnumerable<EnhancementData> enhancements) =>
        ReplaceDict(enhancementDataDict, enhancements, enh => enh.EnhancementLevel);
    
    internal static void SetLimitBreakData(IEnumerable<LimitBreakData> limitBreaks)
    {
        ReplaceList(limitBreakData, limitBreaks);
    }

    // key가 enum이라 다른 방식으로 초기화
    internal static void SetItemDismantleData(IEnumerable<ItemDismantleData> itemDismantles)
    {
        itemDismantleDataDic.Clear();
        foreach (var data in itemDismantles)
        {
            itemDismantleDataDic[data.Rarity] = data;
        }
    }
    
    private static void ReplaceDict<TKey, TValue>(
        Dictionary<TKey, TValue> target,
        IEnumerable<TValue> values,
        Func<TValue, TKey> keySelector,
        Action<TValue>? initializer = null)
    {
        target.Clear();
        foreach (var value in values)
        {
            initializer?.Invoke(value);
            target[keySelector(value)] = value;
        }
    }
    
    private static void ReplaceList<TValue>(
        List<TValue> target, 
        IEnumerable<TValue> values,
        Action<TValue>? initializer = null)
    {
        target.Clear();  // 기존 리스트 내용 삭제
        foreach (var value in values)
        {
            initializer?.Invoke(value);  // 초기화 작업이 필요하면 실행
            target.Add(value);  // 리스트에 새로운 항목 추가
        }
    }
}
