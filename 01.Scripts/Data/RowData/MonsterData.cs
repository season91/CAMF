using System;
using Firebase.Firestore;

public enum AttackType
{
    Melee,
    Range
}

[FirestoreData]
public class MonsterData 
{
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public string name { private get; set; }
    [FirestoreProperty] public int hp { private get; set; }
    [FirestoreProperty] public int atk { private get; set; }
    [FirestoreProperty] public int def { private get; set; }
    [FirestoreProperty] public string atkType { private get; set; }
    [FirestoreProperty] public bool isBoss { private get; set; }
    [FirestoreProperty] public string prefab { private get; set; }
    
    public string Code => code;
    public string Name => name;
    public int Hp => hp;
    public int Atk => atk;
    public int Def => def;
    public bool IsBoss => isBoss;
    public AttackType? lazyAtkType;
    public AttackType AtkType => lazyAtkType ??= Enum.Parse<AttackType>(atkType);
    public string Prefab => prefab;
}
