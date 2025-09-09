using System;
using UnityEngine;

[Serializable]
public class TowerInformation
{
    [Header("Tower Stats")]
    public string Name;
    public string Description;
    public ElementType Type;
    public int Rank;
    public int reinforceLevel;
    public int MaxReinforce;
    public float Damage;
    public float AttackSpeed;
    public float Range;
    public float CriticalHit;

    public StatusEffectType effectType = StatusEffectType.None; // 적용할 상태 이상 종류
    public float effectDuration = 3f; // 지속 시간
    public float effectValue = 20f; // 효과 값 (데미지, 둔화율 등)
}
