using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefense/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("타워 기본 정보")]
    public string Name;
    public string ID;
    public ElementType ElementType;
    public int Level; // 이 데이터가 몇 단계용인지 표시
    public int MaxReinforce = 3; // 1~3단계 타워의 최대 강화 횟수

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
    public StatusEffectType effectType; 
    public float effectDuration;     
    public float effectValue;

    [Tooltip("공격 시 상태이상 게이지를 얼마나 깎을지에 대한 값입니다.")]
    public float effectBuildup = 20f; // 예: 공격마다 20씩 깎음

    [Header("발사체 정보")]
    public GameObject projectilePrefab;

    [Header("업그레이드 정보")]
    public TowerData nextUpgradeData; // 4단계 TowerData를 여기에 연결
    public GameObject upgradedPrefab;  // 4단계 타워 프리팹을 여기에 연결

    [Header("강화/진화 정보")]
    [Tooltip("진화에 필요한 강화 횟수")]
    public int reinforcementThreshold = 10;

    [Tooltip("Light 강화 10회 성공 시 진화할 타워 데이터")]
    public TowerData lightEvolutionData;

    [Tooltip("Dark 강화 10회 성공 시 진화할 타워 데이터")]
    public TowerData darkEvolutionData;
}

public enum AttackMode
{
    Straight,
    Parabolic,
    Projector,
    Flooring
}
