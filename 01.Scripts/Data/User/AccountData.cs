using Firebase.Firestore;

[FirestoreData]
public class AccountData
{
    [FirestoreProperty] public string uid { get; set; }
    [FirestoreProperty] public string email { get; set; }
    [FirestoreProperty] public string nickname { get; set; }
    [FirestoreProperty] public Timestamp createdAt { get; set; }
    [FirestoreProperty] public Timestamp lastLoginAt { get; set; }
    
    // Firestore용 기본 생성자 (필수)
    public AccountData() { }
    
    public AccountData(string uid, string email, string nickname)
    {
        this.uid = uid;
        this.email = email;
        this.nickname = nickname;
        this.createdAt = Timestamp.GetCurrentTimestamp();
        this.lastLoginAt = Timestamp.GetCurrentTimestamp();
    }
    
    public string Nickname => nickname;
}
