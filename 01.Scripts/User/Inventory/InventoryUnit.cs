

using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class InventoryUnit
{
    [FirestoreProperty] public string unitUid { private get; set; }  = Guid.NewGuid().ToString(); // 고유 인스턴스 ID
    [FirestoreProperty] public string unitCode { private get; set; } // 마스터 유닛 코드
    [FirestoreProperty] public List<string> equippedItemUids { get; set; } = new List<string>(); // 장착 item Uid 리스트

    public string UnitUid => unitUid;
    public string UnitCode => unitCode;
    
    // 정렬로 인해 캐싱 필요
    private Dictionary<StatType, int> unitOriginStats = new Dictionary<StatType, int>();
    private Dictionary<StatType, int> unitSumStats = new Dictionary<StatType, int>();
    
    public IReadOnlyDictionary<StatType, int> UnitSumStats => unitSumStats;
    /// <summary>
    /// DB 가져올때 1번만 호출
    /// 유닛 기본

    /// </summary>
    public void BindingOriginStatSum()
    {
        // 유닛 기본 정보 가져오기
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            GetStatFromUnitData(stat);
        }

        BindingItemSumStats();
    }

    // inventory item uid 기준으로 치환하여 리턴
    public List<InventoryItem> EquippedItems
    {
        get
        {
            return UserData.inventory.Items
                           .Where(item => equippedItemUids.Contains(item.ItemUid))
                           .ToList();
        }
    }

    /// 아이템 기본, 강화, 돌파 총 적용
    /// 아이템 강화나 돌파 후 수치 변경될때 재적용 할때도 호출
    public void BindingItemSumStats()
    {
        // 또 얕은복사 문제
        // unitSumStats = unitOriginStats;
        unitSumStats = new Dictionary<StatType, int>(unitOriginStats);
        // 장착 아이템 정보 가져오기. 기본, 강화, 돌파 적용
        foreach (var inventoryItem in EquippedItems)
        {
            if (inventoryItem.EquippedUnitUid == unitUid)
            {
                switch (inventoryItem.ItemType)
                {
                    case ItemType.Armor:
                        unitSumStats[StatType.Def] += inventoryItem.SumDef;
                        break;
                    case ItemType.Weapon:
                        unitSumStats[StatType.Atk] += inventoryItem.SumAtk;
                        break;
                }
            }
        }
        
        // MyDebug.Log($"BindingItemSumStats {EquippedItems.Count} : Atk {unitSumStats[StatType.Atk]} / Def {unitSumStats[StatType.Def]}");
    }

    /// <summary>
    /// 기본 유닛 스탯 + 장착 아이템 스탯 병합 (UI용)
    /// </summary>
    public Dictionary<StatType, int> GetMergeStatsUi()
    {
        BindingItemSumStats();
        MyDebug.Log($"GetMergeStatsUi : Atk {unitSumStats[StatType.Atk]} / Def {unitSumStats[StatType.Def]}");
        return  unitSumStats;
    }

    //
    /// <summary>
    /// StatType value 조회
    /// </summary>
    private void GetStatFromUnitData(StatType type)
    {
        UnitData uniData = MasterData.UnitDataDict[unitCode];
        
        switch (type)
        {
            case StatType.Hp :
                unitOriginStats[StatType.Hp] =  uniData.Hp;
                break;
            case StatType.Atk :
                unitOriginStats[StatType.Atk] = uniData.Atk;
                break;
            case StatType.Def :
                unitOriginStats[StatType.Def] =  uniData.Def;
                break;
            case StatType.Evasion :
                unitOriginStats[StatType.Evasion] = uniData.Eva;
                break;
        }
    }
    
    // Firestore 기본 생성자 필요
    public InventoryUnit() { }

    // 기본 유닛 지급하고 시작  
    public InventoryUnit(string unitCode)
    {
        this.unitCode = unitCode;
    }
}

