using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public enum ItemRarity
{
    None,
    Common,
    Uncommon,
    Rare,
    SuperRare,
    Legendary,
}


public enum ItemType
{
    None,
    Weapon,
    Armor,
}

public enum StatType
{
    Hp,
    Atk,
    Def,
    SkillCoolTime,
    SkillDamage,
    Evasion,
}

[FirestoreData]
public class ItemData
{
    [FirestoreProperty] public string rarity { private get; set; }
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public string name { private get; set; }
    [FirestoreProperty] public string type { private get; set; }
    [FirestoreProperty] public string icon { private get; set; }
    
    [FirestoreProperty] public string description { private get; set; }
    [FirestoreProperty] public bool isEquipable { private get; set; }

    [FirestoreProperty] public Dictionary<string, int> stats { private get; set; } 
    [FirestoreProperty] public bool isGacha { private get; set; }

    private ItemRarity? lazyRarity;
    public ItemRarity Rarity => lazyRarity ??= Enum.Parse<ItemRarity>(rarity);
    public string Code => code;
    public string Name => name;
    public string Description => description;
    private ItemType? lazyType;
    public ItemType Type => lazyType ??= Enum.Parse<ItemType>(type);
    public Sprite Icon => ResourceManager.Instance.GetResource<Sprite>(icon);

    public Dictionary<StatType, int> Stats;
    
    public void InitParsedStats()
    {
        Stats = new Dictionary<StatType, int>(stats.Count);
        foreach (var kvp in stats)
        {
            if (!Enum.TryParse(kvp.Key, out StatType statType))
            {
                MyDebug.LogError($"[StatType] 잘못된 enum 문자열: {kvp.Key}");
                continue;
            }
            Stats[statType] = kvp.Value;
        }
    }
}