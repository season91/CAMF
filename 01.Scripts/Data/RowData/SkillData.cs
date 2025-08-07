using System;
using Firebase.Firestore;
using UnityEngine;

public enum SkillType
{
    Melee,
    Range,
}

public enum SkillTargetType
{
    None,
    Single,
    All,
    MultiTarget,
    RandomSingle,
}

public enum TargetFilter
{
    None,
    Monster,
    Unit,
    LowestHp,
}
[FirestoreData]
public class SkillData
{
    [FirestoreProperty] public string code { private get; set; }
    [FirestoreProperty] public string name { private get; set; }
    [FirestoreProperty] public string icon { private get; set; }
    [FirestoreProperty] public string entityCode { private get; set; }
    [FirestoreProperty] public string skillType { private get; set; } // 지금은 안쓰지만  확장 고려
    [FirestoreProperty] public bool isSelectable { private get; set; }
    [FirestoreProperty] public string damageType { private get; set; } // 지금은 안쓰지만  확장 고려
    [FirestoreProperty] public int cooldown { private get; set; }
    [FirestoreProperty] public string description { private get; set; }
    [FirestoreProperty] public string targetType { private get; set; }
    [FirestoreProperty] public string targetFilter { private get; set; }
    [FirestoreProperty] public float damageMultiplier { private get; set; }
    [FirestoreProperty] public int duration { private get; set; }
    [FirestoreProperty] public float effectValue { private get; set; }
    [FirestoreProperty] public int hitCount { private get; set; }
    public string Code => code;
    public string Name => name;
    public Sprite Icon => ResourceManager.Instance.GetResource<Sprite>(icon);
    public string EntityCode => entityCode;
    public bool IsSelectable => isSelectable;
    public int Cooldown => cooldown;
    public string Description => description;
    
    public SkillTargetType TargetType => Enum.TryParse<SkillTargetType>(targetType, out SkillTargetType result) ? result : SkillTargetType.None;
    public TargetFilter TargetFilter => Enum.TryParse<TargetFilter>(targetFilter, out TargetFilter result) ? result : TargetFilter.None;
    public float DamageMultiplier => damageMultiplier;
    public int Duration => duration;
    public float EffectValue => effectValue;
    public int HitCount => hitCount;
    public SkillType SkillType => Enum.TryParse<SkillType>(skillType, out SkillType result) ? result : SkillType.Melee;
}
