using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicBookBuffSystem : MonoBehaviour
{
    public static MagicBookBuffSystem Instance { get; private set; }

    public static event Action OnBuffsUpdated;

    private readonly Dictionary<EBookEffectType, float> _globalBuffs = new();
    private readonly Dictionary<string, Dictionary<EBookEffectType, float>> _towerSpecificBuffs = new();
    private readonly Dictionary<string, List<TowerCountBuff>> _towerCountBuffs = new();

    // 상태이상 관련 버프
    private readonly Dictionary<StatusEffectType, StatusEffectModifier> _statusEffectModifiers = new();

    // 특수 버프 플래그
    private readonly HashSet<EBookEffectType> _uniqueEffects = new();

    private readonly Dictionary<string, CachedTowerBuffs> _towerBuffCache = new();
    private bool _cacheInvalidated = true;

    private void Awake()
    {
        Instance = this;
    }

    #region Public API

    /// <summary>
    /// 마법도서 효과를 시스템에 등록
    /// </summary>
    public void ApplyBookEffect(BookEffect effect, float value)
    {
        switch (effect.EffectType)
        {
            case EBookEffectType.IncreaseFinalDamage:
            case EBookEffectType.IncreaseAttackSpeed:
            case EBookEffectType.IncreaseRange:
            case EBookEffectType.IncreaseCritChance:
                ApplyGlobalBuff(effect.EffectType, value);
                break;

            case EBookEffectType.SetRange:
            case EBookEffectType.SetStatusEffectPotency:
                ApplyGlobalBuff(effect.EffectType, value);
                break;

            case EBookEffectType.IncreaseStatusEffectDamage:
            case EBookEffectType.IncreaseStatusEffectPotency:
            case EBookEffectType.IncreaseStatusEffectDuration:
                ApplyStatusEffectModifier(effect, value);
                break;

            case EBookEffectType.IncreaseDamagePerTowerCount:
            case EBookEffectType.IncreaseDurationPerTowerCount:
            case EBookEffectType.IncreaseAttackSpeedPerTowerCount:
            case EBookEffectType.IncreaseCritChancePerTowerCount:
                ApplyTowerCountBuff(effect, value);
                break;

            default:
                if (!string.IsNullOrEmpty(effect.TargetTowerCode))
                {
                    ApplyTowerSpecificBuff(effect.TargetTowerCode, effect.EffectType, value);
                }
                else
                {
                    ApplyUniqueEffect(effect.EffectType);
                }
                break;
        }

        _cacheInvalidated = true;
        OnBuffsUpdated?.Invoke();
    }

    /// <summary>
    /// 타워가 설치될 때 호출하여 적용 가능한 모든 버프를 가져옴
    /// </summary>
    public TowerBuffData GetTowerBuffs(string towerCode, ElementType element)
    {
        string cacheKey = $"{towerCode}_{element}";
        if (_cacheInvalidated || !_towerBuffCache.TryGetValue(cacheKey, out var cached))
        {
            cached = CalculateTowerBuffs(towerCode, element);
            _towerBuffCache[cacheKey] = cached;
        }

        return new TowerBuffData
        {
            FinalDamageMultiplier = cached.FinalDamageMultiplier,
            AttackSpeedMultiplier = cached.AttackSpeedMultiplier,
            RangeMultiplier = cached.RangeMultiplier,
            CritChanceBonus = cached.CritChanceBonus,
            StatusEffectModifiers = new Dictionary<StatusEffectType, StatusEffectModifier>(_statusEffectModifiers),
            HasUniqueEffects = new HashSet<EBookEffectType>(_uniqueEffects),
            OverrideRange = cached.OverrideRange
        };
    }

    /// <summary>
    /// 타워 개수 기반 버프 계산
    /// </summary>
    public float GetTowerCountBasedBuff(string towerCode, EBookEffectType effectType)
    {
        if (!_towerCountBuffs.TryGetValue(towerCode, out var buffs)) return 0f;

        float totalBonus = 0f;
        foreach (var buff in buffs)
        {
            if (buff.EffectType == effectType)
            {
                int towerCount = GetTowerCount(buff.CountTargetTowerCode);
                totalBonus += buff.ValuePerTower * towerCount;
            }
        }

        return totalBonus;
    }

    /// <summary>
    /// 특정 효과가 활성화되어 있는지 확인
    /// </summary>
    public bool HasUniqueEffect(EBookEffectType effectType) => _uniqueEffects.Contains(effectType);

    #endregion

    #region Private Implementation

    private void ApplyGlobalBuff(EBookEffectType effectType, float value)
    {
        if (!_globalBuffs.TryAdd(effectType, value))
            _globalBuffs[effectType] += value;
    }

    private void ApplyTowerSpecificBuff(string towerCode, EBookEffectType effectType, float value)
    {
        if (!_towerSpecificBuffs.ContainsKey(towerCode))
            _towerSpecificBuffs[towerCode] = new Dictionary<EBookEffectType, float>();

        var towerBuffs = _towerSpecificBuffs[towerCode];
        if (!towerBuffs.TryAdd(effectType, value))
            towerBuffs[effectType] += value;
    }

    private void ApplyTowerCountBuff(BookEffect effect, float value)
    {
        string targetTower = effect.TargetTowerCode;
        if (string.IsNullOrEmpty(targetTower)) return;

        if (!_towerCountBuffs.ContainsKey(targetTower))
            _towerCountBuffs[targetTower] = new List<TowerCountBuff>();

        _towerCountBuffs[targetTower].Add(new TowerCountBuff
        {
            EffectType = effect.EffectType,
            CountTargetTowerCode = effect.TargetTowerCode,
            ValuePerTower = value
        });
    }

    private void ApplyStatusEffectModifier(BookEffect effect, float value)
    {
        if (!_statusEffectModifiers.ContainsKey(effect.TargetStatusEffect))
        {
            _statusEffectModifiers[effect.TargetStatusEffect] = new StatusEffectModifier();
        }

        var modifier = _statusEffectModifiers[effect.TargetStatusEffect];
        switch (effect.EffectType)
        {
            case EBookEffectType.IncreaseStatusEffectDamage:
                modifier.DamageMultiplier += value / 100f; // % 증가를 배수로 변환
                break;
            case EBookEffectType.IncreaseStatusEffectPotency:
                modifier.PotencyMultiplier += value / 100f; // % 증가를 배수로 변환
                break;
            case EBookEffectType.IncreaseStatusEffectDuration:
                modifier.DurationBonus += value; // 초 단위로 직접 추가
                break;
        }
    }

    private void ApplyUniqueEffect(EBookEffectType effectType)
    {
        _uniqueEffects.Add(effectType);
    }

    private CachedTowerBuffs CalculateTowerBuffs(string towerCode, ElementType element)
    {
        var result = new CachedTowerBuffs
        {
            FinalDamageMultiplier = 1f,
            AttackSpeedMultiplier = 1f,
            RangeMultiplier = 1f,
            CritChanceBonus = 0f,
            OverrideRange = -1f
        };

        ApplyGlobalBuffsToResult(result);

        if (_towerSpecificBuffs.TryGetValue(towerCode, out var specificBuffs))
        {
            ApplySpecificBuffsToResult(result, specificBuffs);
        }

        ApplyElementBuffsToResult(result, element);

        return result;
    }

    private void ApplyGlobalBuffsToResult(CachedTowerBuffs result)
    {
        foreach (var buff in _globalBuffs)
        {
            switch (buff.Key)
            {
                case EBookEffectType.IncreaseFinalDamage:
                    result.FinalDamageMultiplier += buff.Value / 100f;
                    break;
                case EBookEffectType.IncreaseAttackSpeed:
                    result.AttackSpeedMultiplier += buff.Value / 100f;
                    break;
                case EBookEffectType.IncreaseRange:
                    result.RangeMultiplier += buff.Value / 100f;
                    break;
                case EBookEffectType.IncreaseCritChance:
                    result.CritChanceBonus += buff.Value;
                    break;
                case EBookEffectType.SetRange:
                    result.OverrideRange = buff.Value;
                    break;
            }
        }
    }

    private void ApplySpecificBuffsToResult(CachedTowerBuffs result, Dictionary<EBookEffectType, float> specificBuffs)
    {
        foreach (var buff in specificBuffs)
        {
            switch (buff.Key)
            {
                case EBookEffectType.IncreaseFinalDamage:
                    result.FinalDamageMultiplier += buff.Value / 100f;
                    break;
                case EBookEffectType.IncreaseAttackSpeed:
                    result.AttackSpeedMultiplier += buff.Value / 100f;
                    break;
                case EBookEffectType.IncreaseRange:
                    result.RangeMultiplier += buff.Value / 100f;
                    break;
                case EBookEffectType.IncreaseCritChance:
                    result.CritChanceBonus += buff.Value;
                    break;
            }
        }
    }

    private void ApplyElementBuffsToResult(CachedTowerBuffs result, ElementType element)
    {
        string elementKey = $"element_{element}";
        if (_towerSpecificBuffs.TryGetValue(elementKey, out var elementBuffs))
        {
            ApplySpecificBuffsToResult(result, elementBuffs);
        }
    }


    private int GetTowerCount(string towerCode)
    {
        // 바빠서 그냥 Find 사용함 나중에 타워 매니저 만들어야할듯
        Tower[] towers = FindObjectsOfType<Tower>();
        return towers.Length - 1;
    }

    public void InvalidateCache()
    {
        _cacheInvalidated = true;
        _towerBuffCache.Clear();
    }

    #endregion
}

#region Data Structures

[Serializable]
public struct TowerBuffData
{
    public float FinalDamageMultiplier;
    public float AttackSpeedMultiplier;
    public float RangeMultiplier;
    public float CritChanceBonus;
    public float OverrideRange; // -1이면 무시, 양수면 해당 값으로 고정

    public Dictionary<StatusEffectType, StatusEffectModifier> StatusEffectModifiers;
    public HashSet<EBookEffectType> HasUniqueEffects;
}

[Serializable]
public class StatusEffectModifier
{
    public float DamageMultiplier = 1f;     // Burn, Rot, Bleed용 데미지 배수
    public float PotencyMultiplier = 1f;    // Slow, Stun 등의 효과 수치 배수
    public float DurationBonus = 0f;        // 지속시간 추가
}

[Serializable]
public class TowerCountBuff
{
    public EBookEffectType EffectType;
    public string CountTargetTowerCode; // 개수를 세는 대상 타워
    public float ValuePerTower; // 타워 하나당 증가값
}

internal struct CachedTowerBuffs
{
    public float FinalDamageMultiplier;
    public float AttackSpeedMultiplier;
    public float RangeMultiplier;
    public float CritChanceBonus;
    public float OverrideRange;
}

#endregion
