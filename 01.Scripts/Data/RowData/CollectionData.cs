using System;
using Firebase.Firestore;

[FirestoreData]
public class CollectionData
{
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public string type { private get; set; }
    [FirestoreProperty] public string itemCode { private get; set; }
    [FirestoreProperty] public string rewardType { private get; set; }
    [FirestoreProperty] public int amount { private get; set; }

    public string Code => code;
    
    private RewardType? lazyRewardType;
    public RewardType RewardType => lazyRewardType ??= Enum.Parse<RewardType>(rewardType);
    public string ItemCode => itemCode;
    public int Amount => amount;
}
