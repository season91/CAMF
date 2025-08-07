
using System;
using Firebase.Firestore;

[FirestoreData]
public class ItemDismantleData
{
    [FirestoreProperty] public string rarity { private get; set; }
    [FirestoreProperty] public int amount { private get; set; }
    [FirestoreProperty] public string resourceType { private get; set; }

    private ItemRarity? lazyRarity;
    public ItemRarity Rarity => lazyRarity ??= Enum.Parse<ItemRarity>(rarity);
    
    public int Amount => amount;
    
    private ResourceType? lazyResourceType;
    public ResourceType ResourceType => lazyResourceType ??= Enum.Parse<ResourceType>(resourceType);
}