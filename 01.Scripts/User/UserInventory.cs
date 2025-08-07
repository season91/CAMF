using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;

[FirestoreData]
public class UserInventory
{
    [FirestoreProperty] public int gold { private get; set; }
    [FirestoreProperty] public int diamond { private get; set; }
    [FirestoreProperty] public int piece { private get; set; }
    [FirestoreProperty] public int goldPityCount { private get; set; }
    [FirestoreProperty] public int diamondPityCount { private get; set; }
    
    public int Gold => gold;
    public int Diamond => diamond;
    public int Piece => piece;
    public int GoldPityCount => goldPityCount;
    public int DiamondPityCount => diamondPityCount;

    public List<InventoryItem> items { private get; set; } = new List<InventoryItem>(); // DB 적재용 
    public List<InventoryUnit> units { private get; set; } = new List<InventoryUnit>(); // DB 적재용 
    
    public IReadOnlyList<InventoryItem> Items => items;
    public IReadOnlyList<InventoryUnit> Units => units;
    
    // 재화 틀 만들어두기
    public Dictionary<ResourceType, int> currencyResourceTemplate = new Dictionary<ResourceType, int>
    {
        { ResourceType.Gold , 0},
        { ResourceType.Diamond , 0},
        { ResourceType.Piece , 0}
    };
    
    // 천장 틀 만들어두기
    public Dictionary<ResourceType, int> pityCountTemplate = new Dictionary<ResourceType, int>
    {
        { ResourceType.Gold , 0},
        { ResourceType.Diamond , 0},
    };

    
    // resourceTemplate의 값을 갱신
    public void SaveCurrencyToTemplate()
    {
        currencyResourceTemplate[ResourceType.Gold] = gold;
        currencyResourceTemplate[ResourceType.Diamond] = diamond;
        currencyResourceTemplate[ResourceType.Piece] = piece;
    }
    
    public void SavePityCountToTemplate()
    {
        pityCountTemplate[ResourceType.Gold] = goldPityCount;
        pityCountTemplate[ResourceType.Diamond] = diamondPityCount;
    }
    
    // 밖에서는 딕셔너리로 자원 관리 하기 때문에
    public void LoadCurrencyFromTemplate()
    {
        gold = currencyResourceTemplate[ResourceType.Gold];
        diamond = currencyResourceTemplate[ResourceType.Diamond];
        piece = currencyResourceTemplate[ResourceType.Piece];
    }
    
    public void LoadPityCountFromTemplate()
    {
        goldPityCount = pityCountTemplate[ResourceType.Gold];
        diamondPityCount = pityCountTemplate[ResourceType.Diamond];
    }

    
    public Dictionary<ResourceType, int> CurrencyResource => currencyResourceTemplate;
    public Dictionary<ResourceType, int> PityCountDic => pityCountTemplate;
    
    
    // Firestore 기본 생성자 필요
    public UserInventory() { }

    public UserInventory(bool isJoined)
    {
        gold = 2000;
        diamond = 200;
        goldPityCount = 0;
        diamondPityCount = 0;
    }

    public void AddItem(InventoryItem item)
    {
        items.Add(item);
    }
    public void AddUnit(InventoryUnit unit)
    {
        units.Add(unit);
    }
    public void RemoveItem(InventoryItem item)
    {
        items.Remove(item);
    }
    public void RemoveUnit(InventoryUnit unit)
    {
        units.Remove(unit);
    }
    
    #region DB 적재

    public async Task UpdateInventoryResource(ResourceType type)
    {
        LoadCurrencyFromTemplate();
        string uid = FirebaseManager.Instance.DbUser.UserId;
        int value = CurrencyResource.TryGetValue(type, out int v) ? v : 0;   
        await FirestoreUploader.UpdateInventoryCurrency(uid, type.ToString().ToLower(), value);
    }

    public async Task UpdateInventoryPityCount(ResourceType type)
    {
        LoadPityCountFromTemplate();
        string uid = FirebaseManager.Instance.DbUser.UserId;
        string fieldName = $"{type.ToString().ToLower()}PityCount";
        int value = PityCountDic.TryGetValue(type, out int v) ? v : 0; 
        await FirestoreUploader.UpdateInventoryCurrency(uid, fieldName, value);
    }

    #endregion
}
