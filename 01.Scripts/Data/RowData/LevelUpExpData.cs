using Firebase.Firestore;

[FirestoreData]
public class LevelUpExpData
{
    [FirestoreProperty] public int level { private get; set; }
    [FirestoreProperty] public int expToNextLevel { private get; set; }
    [FirestoreProperty] public int totalExp { private get; set; }
    
    public int Level => level;
    public int ExpToNextLevel => expToNextLevel;
    public int TotalExp => totalExp;
}
