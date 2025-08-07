using System.Collections.Generic;
using Firebase.Firestore;
public enum CollectionState
{
    Collected,      // 수집만 함
    RewardClaimed   // 보상 수령함
}

[FirestoreData]
public class CollectionStatus
{
    [FirestoreProperty] public string code { private get; set; } // collectionCode
    [FirestoreProperty] public CollectionState state { private get; set; } // 수집 or 보상 수령 상태
    [FirestoreProperty] public bool isRead { private get; set; } = false; // UI 알림 확인 여부

    public string Code => code;
    public CollectionState State => state;
    public bool IsRead => isRead;
}

[FirestoreData]
public class UserCollected
{
    public List<CollectionStatus> collects { private get; set; } = new();
    [FirestoreProperty] public bool initialized { get; set; } = true;
    public List<CollectionStatus> Collects => collects;

}
