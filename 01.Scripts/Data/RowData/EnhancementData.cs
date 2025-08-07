using Firebase.Firestore;

[FirestoreData]
public class EnhancementData
{
    [FirestoreProperty] public int enhancementLevel { private get; set; }
    [FirestoreProperty] public int successRate { private get; set; }
    [FirestoreProperty] public int bonusStat { private get; set; }
    [FirestoreProperty] public int requiredFragment { private get; set; }
    [FirestoreProperty] public int requiredGold { private get; set; }
    [FirestoreProperty] public int totalStat { private get; set; }
    
    public int EnhancementLevel =>  enhancementLevel;
    
    public int SuccessRate =>  successRate;
    public int BonusStat =>  bonusStat;
    public int RequiredFragment =>  requiredFragment;
    public int RequiredGold =>  requiredGold;
    public int TotalStat =>  totalStat;
}
