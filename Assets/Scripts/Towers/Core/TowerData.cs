using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefense/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("타워 기본 정보")]
    public string Name;
    public string ID;
    public int Level;
    public ElementType ElementType;
    public int MaxReinforce;

    [Header("조합 정보")]
    public List<string> RequiredTowerIds = new List<string>(); // 조합에 필요한 타워 ID들

    [Header("스탯 정보")]
    public StatValue Damage;
    public StatValue AttackSpeed;
    public StatValue CriticalRate;
    public float BaseRange;

    public float GetDamage(int reinforceLevel) => Damage.CalculateStat(reinforceLevel);
    public float GetAttackSpeed(int reinforceLevel) => AttackSpeed.CalculateStat(reinforceLevel);
    public float GetCriticalRate(int reinforceLevel) => CriticalRate.CalculateStat(reinforceLevel);

    [Header("공격 정보")]
    public AttackMode AttackMode;
    public float radiant;
    public float TimeDuration;
    public bool Guided;
    public bool Multi;

    [Header("사운드 설정")]
    public SfxType attackSound; // 이 타워가 사용할 공격 사운드
    public SfxType impactSound; // 적과 부딪힐 때 나는 소리

    [Header("상태 이상 설정")]
    public StatusEffectType effectType; // <-- 이 부분이 필요합니다.
    public float effectDuration;      // <-- 이 부분이 필요합니다.
    public float effectValue;         // <-- 이 부분이 필요합니다.
}

public enum AttackMode
{
    Straight,
    Parabolic,
    Projector,
    Flooring
}
