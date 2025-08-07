using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

/// <summary>
/// [공통] csv import
/// </summary>

public class CsvParser
{
#if UNITY_EDITOR
    #region Item CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/ItemData")]
    public static void ImportItemDataCsv()
    {
        // string path = EditorUtility.OpenFilePanel("CSV 선택", "", "csv");
        string path =  "Assets/08.Documents/ItemData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // Item Data csv -> json으로 parse
        ParseItemDataCsvImport(path);
        
    }

    public static void ParseItemDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);
        // var items = new List<ItemData>();

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            string statNameRaw = fields[7].Trim();
            string statValueRaw = fields[8].Trim();

            string[] statNames = statNameRaw.Split(new[] { "||" }, StringSplitOptions.None);
            string[] statValues = statValueRaw.Split(new[] { "||" }, StringSplitOptions.None);

            Dictionary<string, int> stats = new();
            
            for (int j = 0; j < statNames.Length && j < statValues.Length; j++)
            {
                if (int.TryParse(statValues[j], out int value))
                {
                    stats.Add(statNames[j], value);
                }
                else
                {
                    MyDebug.LogWarning($"stat 변환 실패: {statNames[j]} → '{statValues[j]}'");
                }
            }

            ItemData item = new ItemData
            {
                code = fields[0],
                rarity = fields[1],
                type = fields[2],
                name = fields[3],
                icon = fields[4],
                description = fields[5],
                isEquipable = bool.Parse(fields[6]),
                stats = stats,
                isGacha = bool.Parse(fields[9]),
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(item, FirestoreCollection.Item);
        }
    }

    #endregion

    #region Monster CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/MonsterData")]
    public static void ImportMonsterDataCsv()
    {
        string path =  "Assets/08.Documents/MonsterData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        ParseMonsterDataCsvImport(path);
    }

    public static void ParseMonsterDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            MonsterData monster = new MonsterData
            {
                code = fields[0],
                name = fields[1],
                hp = int.Parse(fields[2]),
                atk = int.Parse(fields[3]),
                def = int.Parse(fields[4]),
                atkType = fields[5],
                isBoss = bool.Parse(fields[6])
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(monster, FirestoreCollection.Monster);
        }
    }

    #endregion

    #region Stage CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/StageData")]
    public static void ImportStageDataCsv()
    {
        string path =  "Assets/08.Documents/StageData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        ParseStageDataCsvImport(path);
    }

    public static void ParseStageDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');
            
            string rewardRaw = fields[7].Trim();
            string rewardValueRaw = fields[8].Trim();

            string[] rewardType = rewardRaw.Split(new[] { "||" }, StringSplitOptions.None);
            string[] rewardValues = rewardValueRaw.Split(new[] { "||" }, StringSplitOptions.None);

            // 고정 보상, 
            Dictionary<string, int> rewards = new();
            
            for (int j = 0; j < rewardType.Length && j < rewardValues.Length; j++)
            {
                if (int.TryParse(rewardValues[j], out int value))
                {
                    rewards.Add(rewardType[j], value);
                }
                else
                {
                    MyDebug.LogWarning($"reward 변환 실패: {rewardType[j]} → '{rewardValues[j]}'");
                }
            }
            
            string randomRewardRaw = fields[9].Trim();
            string randomRewardValueRaw = fields[10].Trim();

            string[] randomRewardType = randomRewardRaw.Split(new[] { "||" }, StringSplitOptions.None);
            string[] randomRewardValues = randomRewardValueRaw.Split(new[] { "||" }, StringSplitOptions.None);
            
            // 확률 보상
            Dictionary<string, float> randomRewards = new();
            for (int j = 0; j < randomRewardType.Length && j < randomRewardValues.Length; j++)
            {
                if (float.TryParse(randomRewardValues[j], out float value))
                {
                    randomRewards.Add(randomRewardType[j], value);
                }
                else
                {
                    MyDebug.LogWarning($"reward 변환 실패: {randomRewardType[j]} → '{randomRewardValues[j]}'");
                }
            }
            
            //1회성 보상
            string firstRewardRaw = fields[11].Trim();
            string firstRewardValueRaw = fields[12].Trim();
            Dictionary<string, string> firstRewards = new();
            MyDebug.Log($"firstReward <UNK> <UNK>: {firstRewardRaw}");
            if (firstRewardRaw != "-")
            {
                string[] firstRewardType = firstRewardRaw.Split(new[] { "||" }, StringSplitOptions.None);
                string[] firstRewardValues = firstRewardValueRaw.Split(new[] { "||" }, StringSplitOptions.None);
            
                for (int j = 0; j < firstRewardType.Length && j < firstRewardValues.Length; j++)
                {
                    firstRewards.Add(firstRewardType[j], firstRewardValues[j]);
                }
            }
            
            StageData stage = new StageData
            {
                code = fields[0],
                chapterNumber = int.Parse(fields[1]),
                stageNumber = int.Parse(fields[2]),
                chapterTitle = fields[3],
                stageTitle = fields[4],
                // monsterCode = fields[5],
                // monsterCount = int.Parse(fields[6]),
                rewards = rewards,
                randomRewards = randomRewards,
                firstClearRewards = firstRewards
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(stage, FirestoreCollection.Stage);
        }
    }

    #endregion
    
    #region Unit CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/UnitData")]
    public static void ImportUnitDataCsv()
    {
        string path =  "Assets/08.Documents/UnitData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // csv -> class 로 parsing -> db insert
        ParseUnitDataCsvImport(path);
    }

    public static void ParseUnitDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            UnitData unit = new UnitData
            {
                code = fields[0],
                name = fields[1],
                icon = fields[2],
                hp = int.Parse(fields[3]),
                atk = int.Parse(fields[4]),
                def = int.Parse(fields[5]),
                eva = int.Parse(fields[6]),
                atkType = fields[7],
                description = fields[8],
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(unit, FirestoreCollection.Unit);
        }
    }
    
    #endregion
    
    #region Skill CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/SkillData")]
    public static void ImportSkillDataCsv()
    {
        string path =  "Assets/08.Documents/SkillData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // csv -> class 로 parsing -> db insert
        ParseSkillDataCsvImport(path);
    }

    public static void ParseSkillDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            SkillData skill = new SkillData
            {
                code = fields[0],
                name = fields[1],
                icon = fields[2],
                entityCode = fields[3],
                skillType = fields[4],
                isSelectable = bool.Parse(fields[5]),
                damageType = fields[6],
                cooldown = int.Parse(fields[7]),
                description = fields[8],
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(skill, FirestoreCollection.Skill);
        }
    }
    
    #endregion
    
    #region Collection CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/CollectionData")]
    public static void ImportCollectionDataCsv()
    {
        string path =  "Assets/08.Documents/CollectionData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // csv -> class 로 parsing -> db insert
        ParseCollectionDataCsvImport(path);
    }

    public static void ParseCollectionDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            CollectionData collection = new CollectionData
            {
                code = fields[0],
                type = fields[1],
                itemCode = fields[2],
                rewardType = fields[3],
                amount = int.Parse(fields[4]),
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(collection, FirestoreCollection.Collection);
        }
    }
    
    #endregion
    
    #region LevelUpExpData CSV -> db create
    [MenuItem("Tools/[DB] CSV → DB/LevelUpExpData")]
    public static void ImportLevelExpDataCsv()
    {
        string path =  "Assets/08.Documents/LevelUpExpData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // csv -> class 로 parsing -> db insert
        ParseLevelExpDataCsvImport(path);
    }

    public static void ParseLevelExpDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            LevelUpExpData levelUpExp = new LevelUpExpData
            {
                level = int.Parse(fields[0]),
                expToNextLevel = int.Parse(fields[1]),
                totalExp = int.Parse(fields[2]),
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(levelUpExp, FirestoreCollection.LevelUpExp);
        }
    }
    
    #endregion
    
    #region 강화 EnhancementData CSV -> db create
    
    [MenuItem("Tools/[DB] CSV → DB/EnhancementData")]
    public static void ImportEnhancementDataCsv()
    {
        string path =  "Assets/08.Documents/EnhancementData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // csv -> class 로 parsing -> db insert
        ParseEnhancementDataCsvImport(path);
    }

    public static void ParseEnhancementDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            EnhancementData enhancement = new EnhancementData
            {
                enhancementLevel =  int.Parse(fields[0]),
                successRate = int.Parse(fields[1]),
                bonusStat = int.Parse(fields[2]),
                requiredFragment = int.Parse(fields[3]),
                requiredGold = int.Parse(fields[4]),
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(enhancement, FirestoreCollection.Enhancement);
        }
    }
    
    #endregion
    
    #region 돌파 LimitBreakData CSV -> db create
    
    [MenuItem("Tools/[DB] CSV → DB/LimitBreakData")]
    public static void ImportLimitBreakDataCsv()
    {
        string path =  "Assets/08.Documents/LimitBreakData.csv";
        if (string.IsNullOrEmpty(path)) return;
        
        // csv -> class 로 parsing -> db insert
        ParseLimitBreakDataCsvImport(path);
    }

    public static void ParseLimitBreakDataCsvImport(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var fields = lines[i].Split(',');

            LimitBreakData limitBreak = new LimitBreakData
            {
                code = fields[0],
                rarity = fields[1],
                limitBreakLevel = int.Parse(fields[2]),
                statIncrease = fields[3],
                requiredItemCount = int.Parse(fields[4]),
                requiredGold = int.Parse(fields[5]),
            };
            
            // firebase create
            FirestoreUploader.UploadMasterDataToFirestore(limitBreak, FirestoreCollection.LimitBreak);
        }
    }
    
    #endregion
    
    // #region Gacha CSV -> db create
    // [MenuItem("Tools/[DB] CSV → DB/Gacha")]
    // public static void GachaImport()
    // {
    //     string path =  "Assets/08.Documents/GachaData.csv";
    //     if (string.IsNullOrEmpty(path)) return;
    //
    //     ParseGachaDataCsvImport(path);
    // }
    //
    // public static void ParseGachaDataCsvImport(string csvPath)
    // {
    //     var lines = File.ReadAllLines(csvPath);
    //     Dictionary<string, Dictionary<string, float>> dict = new();
    //
    //     for (int i = 1; i < lines.Length; i++) // 첫 줄은 헤더니까 스킵
    //     {
    //         if (string.IsNullOrWhiteSpace(lines[i])) continue;
    //         var fields = lines[i].Split(',');
    //
    //         string currency = fields[0].Trim();   // "Gold" or "Diamond"
    //         string rarity = fields[1].Trim();     // "Common" ...
    //         float weight = float.Parse(fields[2].Trim()); 
    //
    //         if (!dict.ContainsKey(currency))
    //             dict[currency] = new Dictionary<string, float>();
    //
    //         dict[currency][rarity] = weight;
    //     }
    //
    //     GachaTable table = new GachaTable { gachaDatas = dict };
    //
    //     // firebase create
    //     FirestoreUploader.UploadMasterDataToFirestore(table, FirestoreDocument.Gacha);
    // }
    // #endregion
#endif
}
