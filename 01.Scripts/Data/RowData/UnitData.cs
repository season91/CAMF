using System;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class UnitData
{
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public string name { private get; set; }
    [FirestoreProperty] public string icon { private get; set; }
    [FirestoreProperty] public int hp { private get; set; }
    [FirestoreProperty] public int atk { private get; set; }
    [FirestoreProperty] public int def { private get; set; }
    [FirestoreProperty] public int eva { private get; set; }
    [FirestoreProperty] public string atkType { private get; set; }
    [FirestoreProperty] public string description { private get; set; }
    [FirestoreProperty] public string prefab { private get; set; }
    
    public string Code => code;
    public string Name => name;
    
    public Sprite Icon => ResourceManager.Instance.GetResource<Sprite>(icon);
    
    public int Hp => hp;
    public int Atk => atk;
    public int Def => def;
    public int Eva => eva;
    public AttackType? lazyAtkType;
    public AttackType AtkType => lazyAtkType??= Enum.Parse<AttackType>(atkType);
    public string Description => description;
    public string Prefab => prefab;
}
