
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class InventoryItem
{
    [FirestoreProperty] public string itemUid { private get; set;} = Guid.NewGuid().ToString();
    [FirestoreProperty] public string itemCode { private get; set; }
    [FirestoreProperty] public ItemType itemType { private get; set; }
    [FirestoreProperty] public int limitBreakLevel { private get; set; }
    [FirestoreProperty] public int enhancementLevel { private get; set; }
    [FirestoreProperty] public string equippedUnitUid { private get; set; }
    [FirestoreProperty] public Timestamp obtainedAt { private get; set; } = Timestamp.GetCurrentTimestamp();

    public string ItemUid => itemUid;
    public string ItemCode => itemCode;
    public ItemType ItemType => itemType;
    public int LimitBreakLevel => limitBreakLevel;
    public int EnhancementLevel => enhancementLevel;
    public string EquippedUnitUid => equippedUnitUid;
    public Timestamp ObtainedAt => obtainedAt;

    public ItemRarity rarity;
    public ItemRarity Rarity => rarity;

    // 장착 중인 아이템 여부를 equippedUnitUid의 여부에 따라 결정
    public bool IsEquipped => !string.IsNullOrEmpty(equippedUnitUid);

    public int SumAtk { get; private set; }
    public int SumDef { get; private set; }

    // UI에서 보여줄 Item Stat 값
    private Dictionary<StatType, int> statForUi = new();

    /// <summary>
    /// DB 가져올때 1번만 호출
    /// 강화나 돌파 후 수치 변경될때 재적용 할때도 호출함
    /// </summary>
    public void BindingSumStat()
    {
        // 기본 item 스탯 반영
        ApplyBaseStat();

        // 강화 스탯 반영
        if (EnhancementLevel > 0)
        {
            ApplyEnhancementStat();
        }

        // 돌파 스탯 반영
        if (LimitBreakLevel > 0)
        {
            ApplyLimitBreakStat();
        }
    }

    public void BindingItemData(ItemData itemData)
    {
        rarity = itemData.Rarity;
    }

    /// <summary>
    /// 아이템의 스탯
    /// 아이템 기본 스탯 + 강화와 돌파 수치에 맞게 계산하여 반환
    /// </summary>
    public IReadOnlyDictionary<StatType, int> GetStatsForUi()
    {
        statForUi[StatType.Def] = SumDef;
        statForUi[StatType.Atk] = SumAtk;
        
        MyDebug.Log($"statForUi[StatType.Atk] {statForUi[StatType.Atk]} : statForUi[StatType.Def] {statForUi[StatType.Def]}");
        return statForUi;
    }
    
    /// <summary>
    /// 아이템을 장착한 유닛 Data
    /// UI에서 보여줄때. 
    /// </summary>
    public Sprite GetUnitEquippedUnitIcon()
    {
        if (!InventoryManager.Instance.Cache.ItemUidToUnitDic.TryGetValue(ItemUid, out var inventoryUnit))
        {
            return null;
        }
        
        // 해당 ItemUid를 장착한 Unit의 UnitData를 가져와야함
        if (!MasterData.UnitDataDict.TryGetValue(inventoryUnit.UnitCode, out UnitData unitData))
        {
            return null;
        }

        return unitData.Icon;
    }

    public InventoryItem(){ }
    public InventoryItem(ItemData itemData)
    {
        this.itemCode = itemData.Code;
        // 변환 결과 저장
        this.itemType = itemData.Type;
    }
    
    #region 스탯 반영 처리

    
    // 기본 item 스탯 반영
    private void ApplyBaseStat()
    {
        if (MasterData.ItemDataDict.TryGetValue(ItemCode, out var itemData))
        {
            switch (itemType)
            {
                case ItemType.Armor:
                    SumDef = itemData.Stats[StatType.Def];
                    break;
                case ItemType.Weapon:
                    SumAtk = itemData.Stats[StatType.Atk];
                    break;
            }
        }
    }

    // 강화 스탯 반영
    private void ApplyEnhancementStat()
    {
        if (MasterData.EnhancementDataDict.TryGetValue(enhancementLevel, out var enhancementData))
        {
            switch (itemType)
            {
                case ItemType.Armor:
                    SumDef += enhancementData.TotalStat;
                    break;
                case ItemType.Weapon:
                    SumAtk += enhancementData.TotalStat;
                    break;
            }
        }
    }

    // 돌파 스탯 반영
    private void ApplyLimitBreakStat()
    {
        // 조건에 맞는 데이터를 찾기
        var limitBreakData = MasterData.LimitBreakData
                                       .FirstOrDefault(data => data.LimitBreakLevel == LimitBreakLevel && data.Rarity == Rarity);

        if (limitBreakData != null)
        {
            switch (limitBreakData.StatIncreaseType)
            {
                case IncreaseType.Add:
                    ApplyStatIncrease(limitBreakData.TotalStatValue);
                    break;
                case IncreaseType.Multiply:
                    ApplyStatIncrease(limitBreakData.TotalStatRate);
                    break;
            }
        }
        else
        {
            MyDebug.Log("돌파 아이템이 null입니다. 조건에 맞는 데이터를 찾을 수 없습니다.");
            // 추가 디버깅 정보 출력
            MyDebug.LogWarning($"찾는 조건 - LimitBreakLevel: {LimitBreakLevel}, Rarity: {rarity}");
        }
    }

    // 스탯 증가 처리 더하기
    private void ApplyStatIncrease(int statIncrease)
    {
        switch (itemType)
        {
            case ItemType.Armor:
                SumDef += statIncrease;
                break;
            case ItemType.Weapon:
                SumAtk += statIncrease;
                break;
        }
    }

    // 스탯 증가 처리 곱하기
    private void ApplyStatIncrease(float statIncrease)
    {
        if (MasterData.ItemDataDict.TryGetValue(ItemCode, out var itemData))
        {
            switch (itemType)
            {
                case ItemType.Armor:
                    SumDef += (int)(itemData.Stats[StatType.Def] * statIncrease);
                    break;
                case ItemType.Weapon:
                    SumAtk += (int)(itemData.Stats[StatType.Atk] * statIncrease);
                    break;
            }
        }
    }

    #endregion
}