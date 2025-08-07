using System;
using Firebase.Firestore;

public enum IncreaseType
{
    Add,
    Multiply
}
[FirestoreData]
public class LimitBreakData
{
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public string rarity { private get; set; }
    [FirestoreProperty] public int limitBreakLevel { private get; set; }
    [FirestoreProperty] public string statIncrease { private get; set; }
    [FirestoreProperty] public string statIncreaseType { private get; set; }
    [FirestoreProperty] public int requiredItemCount { private get; set; }
    [FirestoreProperty] public int requiredGold { private get; set; }
    [FirestoreProperty] public string totalStat { private get; set; } //최종 적용 스텟
    
    public string Code => code;
    public int LimitBreakLevel => limitBreakLevel;
    public int RequiredItemCount => requiredItemCount;
    public int RequiredGold => requiredGold;

    public ItemRarity? lazyRarity;
    public ItemRarity Rarity => lazyRarity ??= Enum.Parse<ItemRarity>(rarity);

    public IncreaseType? lazyStatIncreaseType;
    public IncreaseType StatIncreaseType => lazyStatIncreaseType ??= Enum.Parse<IncreaseType>(statIncreaseType);
    
    public float IncreaseStatRate = 0f;
    public int IncreaseStatValue = 0;
    
    public float TotalStatRate = 0f; // Multiply용 연산용
    public int TotalStatValue = 0; // Add용 연산용
    
    public void InitStatIncreaseValue()
    {
        switch (StatIncreaseType)
        {
            case IncreaseType.Add:
                IncreaseStatValue = int.Parse(statIncrease);
                TotalStatValue = int.Parse(totalStat);
                break;
            case IncreaseType.Multiply:
                IncreaseStatRate = float.Parse(statIncrease);
                TotalStatRate = float.Parse(totalStat);
                break;
        }
    }

    /// <summary>
    /// UI 출력용 스텟
    /// </summary>
    public int PrintIncreaseStat(ItemData itemData)
    {
        switch (StatIncreaseType)
        {
            case IncreaseType.Add:
                return IncreaseStatValue;
            case IncreaseType.Multiply:
                switch (itemData.Type)
                {
                    case ItemType.Armor:
                        return (int)(itemData.Stats[StatType.Def] * IncreaseStatRate);
                    case ItemType.Weapon:
                        return (int)(itemData.Stats[StatType.Atk] * IncreaseStatRate);
                    default:
                        MyDebug.LogWarning($"Unknown ItemType: {itemData.Type}");
                        return 0;
                }
        }
        return 0;
    }
}
