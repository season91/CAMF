using System;
using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;

[FirestoreData]
public class GachaTable
{
    // key = resourceType
    
    [FirestoreProperty] public Dictionary<string, Dictionary<string, string>> gachaRates { private get; set; }
    [FirestoreProperty] public Dictionary<string, string> gachaPityLegendChances { private get; set; }
    [FirestoreProperty] public Dictionary<string, string> gachaPityIncreases { private get; set; }
    [FirestoreProperty] public Dictionary<string, int> gachaPityThresholds { private get; set; }
    [FirestoreProperty] public Dictionary<string, int> gachaCosts { private get; set; }
    
    public Dictionary<ResourceType, Dictionary<ItemRarity, float>> GachaRates
    {
        get
        {
            var newGachaDatas = new Dictionary<ResourceType, Dictionary<ItemRarity, float>>();

            foreach (var gachaDic in gachaRates)
            {
                if (!Enum.TryParse<ResourceType>(gachaDic.Key, out var resourceType))
                {
                    MyDebug.LogWarning($"ResourceType 파싱 실패: {gachaDic.Key}");
                    continue;
                }

                var newRarityDic = new Dictionary<ItemRarity, float>();
                foreach (var rarityDic in gachaDic.Value)
                {
                    if (!Enum.TryParse<ItemRarity>(rarityDic.Key, out var rarity))
                    {
                        MyDebug.LogWarning($"ItemRarity 파싱 실패: {rarityDic.Key}");
                        continue;
                    }

                    if (!float.TryParse(rarityDic.Value, out var rate))
                    {
                        MyDebug.LogWarning($"확률 Rate 파싱 실패: {rarityDic.Value}");
                        continue;
                    }
                    newRarityDic[rarity] = rate;
                }
                newGachaDatas[resourceType] = newRarityDic;
            }

            return newGachaDatas;
        }
    }
    
    public Dictionary<ResourceType, float> GachaPityLegendChances
    {
        get
        {
            var newGachaChances = new Dictionary<ResourceType, float>();

            foreach (var chance in gachaPityLegendChances)
            {
                if (!Enum.TryParse<ResourceType>(chance.Key, out var resourceType))
                {
                    MyDebug.LogWarning($"ResourceType 파싱 실패: {chance.Key}");
                    continue;
                }
                if (!float.TryParse(chance.Value, out var rate))
                {
                    MyDebug.LogWarning($"반천장 비중 파싱 실패: {chance.Value}");
                    continue;
                }

                newGachaChances[resourceType] = rate;
            }

            return newGachaChances;
        }
    }

    public Dictionary<ResourceType, float> GachaPityIncreases
    {
        get
        {
            var newGachaPityIncreases = new Dictionary<ResourceType, float>();
            foreach (var chance in gachaPityIncreases)
            {
                if (!Enum.TryParse<ResourceType>(chance.Key, out var resourceType))
                {
                    MyDebug.LogWarning($"ResourceType 파싱 실패: {chance.Key}");
                    continue;
                }
                if (!float.TryParse(chance.Value, out var rate))
                {
                    MyDebug.LogWarning($"증가율 파싱 실패: {chance.Value}");
                    continue;
                }

                newGachaPityIncreases[resourceType] = rate;
            }

            return newGachaPityIncreases;
        }
    }

    public Dictionary<ResourceType, int> GachaPityThresholds
    {
        get
        {
            var newGachaPityThresholds = new Dictionary<ResourceType, int>();
            foreach (var chance in gachaPityThresholds)
            {
                if (!Enum.TryParse<ResourceType>(chance.Key, out var resourceType))
                {
                    MyDebug.LogWarning($"ResourceType 파싱 실패: {chance.Key}");
                    continue;
                }
  
                newGachaPityThresholds[resourceType] = chance.Value;
            }

            return newGachaPityThresholds;
        }
    }

    public Dictionary<ResourceType, int> GachaCosts
    {
        get
        {
            var newGachaCosts = new Dictionary<ResourceType, int>();
            foreach (var chance in gachaCosts)
            {
                if (!Enum.TryParse<ResourceType>(chance.Key, out var resourceType))
                {
                    MyDebug.LogWarning($"ResourceType 파싱 실패: {chance.Key}");
                    continue;
                }
                
                newGachaCosts[resourceType] = chance.Value;
            }

            return newGachaCosts;
        }
    }
}