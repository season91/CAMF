using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class JsonParser
{
    [MenuItem("Tools/[Json] Json → DB/Gacha")]
    public static void LoadJsonByGachaTable()
    {
        string path =  "Assets/08.Documents/GachaTable.json";
        
        if (!File.Exists(path))
        {
            Debug.LogError("GachaWeight.json 파일을 찾을 수 없음: " + path);
            return;
        }
        
        // JSON 파일을 라인 단위로 읽고 합침
        string[] lines = File.ReadAllLines(path);
        string json = string.Join("", lines);

        GachaTable table = JsonConvert.DeserializeObject<GachaTable>(json);
        
        FirestoreUploader.UploadMasterDataToFirestore(table, FirestoreDocument.Gacha);
    }

    
    /// <summary>
    /// JSON 파일을 읽어서, 여러 static 클래스를 하나의 .cs 파일(StringAdrXXX들로)로 생성해주는 유틸
    /// </summary>
    [MenuItem("Tools/[Json] Json -> AdrStringNamespace")]
    public static void GenerateNamespaceKeys()
    {
        string jsonPath = "Assets/08.Documents/Address.json";
        string outputPath = "Assets/01.Scripts/Static/AdrStringNamespace.cs";
        
        // enum으로 변환할 key 목록 (ItemRarity로 매핑)
        HashSet<string> EnumItemRarityKeys = new()
        {
            "GoldCardBg",
            "DiaCardBg"
        };

        if (!File.Exists(jsonPath))
        {
            Debug.LogError("Address.json 파일 없음!");
            return;
        }

        // JSON 전체 파싱
        JObject root = JObject.Parse(File.ReadAllText(jsonPath));
        
        // 최종 생성될 모든 라인을 저장할 리스트
        var fileLines = new List<string>
        {
            "using System.Collections.Generic;",
            ""
        };

        // 1. AdrJsonKeys 클래스 먼저 생성
        fileLines.Add("public static class AdrJsonKeys");
        fileLines.Add("{");
        foreach (var category in root)
        {
            string topKey = category.Key;
            fileLines.Add($"    public const string {topKey} = \"{topKey}\";");
        }
        fileLines.Add("}");
        fileLines.Add("");
        
        // 2. 나머지 각 static class 생성
        foreach (var category in root)
        {
            // string className = "String" + category.Key;
            string className = category.Key;
            fileLines.Add($"public static class {className}");
            fileLines.Add("{");

            if (category.Value is not JObject innerObject)
                continue;

            foreach (var field in innerObject)
            {
                string key = field.Key;
                var value = field.Value;

                switch (value.Type)
                {
                    // "Key": "StringValue" → const string
                    case JTokenType.String:
                        fileLines.Add($"    public const string {key} = \"{value}\";");
                        break;
                    // "Key": ["v1", "v2"] → List<string>
                    case JTokenType.Array:
                        var arr = (JArray)value;
                        fileLines.Add($"    public static readonly List<string> {key} = new()");
                        fileLines.Add("    {");
                        for (int i = 0; i < arr.Count; i++)
                        {
                            string val = arr[i].ToString();
                            string comment = i == 0 ? " // Default" : "";
                            fileLines.Add($"        \"{val}\",{comment}");
                        }
                        fileLines.Add("    };");
                        break;
                    // "Key": { ... } → Dictionary<string, string> 또는 중첩 구조
                    case JTokenType.Object:
                        var obj = (JObject)value;

                        if (IsFlatStringDict(obj))
                        {
                            // "GoldCardBg": { "Common": ... } → Dictionary<ItemRarity, string>
                            if (EnumItemRarityKeys.Contains(key))
                            {
                                fileLines.Add($"    public static readonly Dictionary<ItemRarity, string> {key}Dict = new()");
                                fileLines.Add("    {");
                                foreach (var sub in obj)
                                    fileLines.Add($"        {{ ItemRarity.{sub.Key}, \"{sub.Value}\" }},");
                                fileLines.Add("    };");
                            }
                            else
                            {
                                // 일반 딕셔너리
                                fileLines.Add($"    public static readonly Dictionary<string, string> {key}Dict = new()");
                                fileLines.Add("    {");
                                foreach (var sub in obj)
                                    fileLines.Add($"        {{ \"{sub.Key}\", \"{sub.Value}\" }},");
                                fileLines.Add("    };");
                            }
                        }
                        else
                        {
                            // 중첩 구조 처리 (AdrEntity.Unit.MON_0001 등)
                            foreach (var sub in obj)
                            {
                                if (sub.Value is not JObject subObj) continue;

                                fileLines.Add($"    public static readonly Dictionary<string, string> {sub.Key}Dict = new()");
                                fileLines.Add("    {");
                                foreach (var kv in subObj)
                                    fileLines.Add($"        {{ \"{kv.Key}\", \"{kv.Value}\" }},");
                                fileLines.Add("    };");
                            }
                        }
                        break;
                }
            }

            fileLines.Add("}"); // 클래스 닫기
            fileLines.Add("");
        }

        // 폴더 자동 생성 (디렉토리 경로만 따로 추출해서!)
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // 최종 작성
        File.WriteAllLines(outputPath, fileLines);

        AssetDatabase.Refresh();
        Debug.Log("AdrStringNamespace 클래스 생성 완료!");
    }
    
    /// <summary>
    /// Dictionary<string, string> 으로 쓸 수 있는 구조인지 확인
    /// (모든 value가 string이면 true)
    /// </summary>
    private static bool IsFlatStringDict(JObject obj)
    {
        foreach (var prop in obj.Properties())
        {
            if (prop.Value.Type != JTokenType.String)
                return false;
        }
        return true;
    }
}
#endif