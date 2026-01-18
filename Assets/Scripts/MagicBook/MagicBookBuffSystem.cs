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
    
    // 시너지 효과 값 (달빛검 등)
    private float _synergyDamageBonusValue = 0f;

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
            case EBookEffectType.IncreaseAttackDuration:
            case EBookEffectType.IncreaseRange:
            case EBookEffectType.IncreaseCritChance:
            case EBookEffectType.IncreaseSelectableBookCount:
            case EBookEffectType.FireTowerSummonEther:
            case EBookEffectType.WaterTowerSummonEther:
            case EBookEffectType.AirTowerSummonEther:
            case EBookEffectType.EarthTowerSummonEther:
                ApplyGlobalBuff(effect.EffectType, value);
                break;

            case EBookEffectType.SetRange:
            case EBookEffectType.SetStatusEffectPotency:
                ApplyGlobalBuff(effect.EffectType, value);
                break;

            case EBookEffectType.IncreaseStatusEffectDamage:
            case EBookEffectType.IncreaseStatusEffectPotency:
            case EBookEffectType.IncreaseStatusEffectDuration:
            case EBookEffectType.ModifyStatusEffectValue:
                ApplyStatusEffectModifier(effect, value);
                break;

            case EBookEffectType.IncreaseDamagePerTowerCount:
            case EBookEffectType.IncreaseDurationPerTowerCount:
            case EBookEffectType.IncreaseAttackSpeedPerTowerCount:
            case EBookEffectType.IncreaseCritChancePerTowerCount:
            case EBookEffectType.IncreaseStatusEffectPotencyPerTowerCount:
                ApplyTowerCountBuff(effect, value);
                break;

            case EBookEffectType.SynergyDamageBoost:
                _synergyDamageBonusValue = value;
                ApplyUniqueEffect(effect.EffectType);
                break;

            default:
                // TargetElement가 설정된 경우 Element 버프로 처리
                if (effect.TargetElement != ElementType.None)
                {
                    string elementKey = $"element_{effect.TargetElement}";
                    ApplyTowerSpecificBuff(elementKey, effect.EffectType, value);
                }
                // TargetTowerCode가 설정된 경우 타워 특정 버프로 처리
                else if (!string.IsNullOrEmpty(effect.TargetTowerCode))
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
                int towerCount = GetTowerCount(buff.CountTargetTowerCode, buff.CountTargetElement);
                totalBonus += buff.ValuePerTower * towerCount;
            }
        }

        return totalBonus;
    }

    /// <summary>
    /// 타워 개수 기반 상태이상 수치 버프 계산
    /// </summary>
    public float GetTowerCountBasedStatusPotency(string towerCode, StatusEffectType statusType)
    {
        if (!_towerCountBuffs.TryGetValue(towerCode, out var buffs)) return 0f;

        float totalBonus = 0f;
        foreach (var buff in buffs)
        {
            if (buff.EffectType == EBookEffectType.IncreaseStatusEffectPotencyPerTowerCount &&
                buff.TargetStatusEffect == statusType)
            {
                int towerCount = GetTowerCount(buff.CountTargetTowerCode, buff.CountTargetElement);
                totalBonus += buff.ValuePerTower * towerCount;
            }
        }
        return totalBonus;
    }

    /// <summary>
    /// 특정 효과가 활성화되어 있는지 확인
    /// </summary>
    public bool HasUniqueEffect(EBookEffectType effectType) => _uniqueEffects.Contains(effectType);

    /// <summary>
    /// 전역 버프 값 조회 (예: 선택 가능한 마법도서 수)
    /// </summary>
    public float GetGlobalBuffValue(EBookEffectType effectType)
    {
        return _globalBuffs.GetValueOrDefault(effectType, 0f);
    }

    /// <summary>
    /// 상태이상 타입별 수정치 조회
    /// </summary>
    public StatusEffectModifier GetStatusEffectModifier(StatusEffectType type)
    {
        if (_statusEffectModifiers.TryGetValue(type, out var modifier))
        {
            return modifier;
        }
        return new StatusEffectModifier(); // 기본값 반환
    }

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

    // 이미 수정된 ApplyTowerCountBuff는 건너뜀

    private void ApplyTowerCountBuff(BookEffect effect, float value)
    {
        string targetTower = effect.TargetTowerCode;
        if (string.IsNullOrEmpty(targetTower) && effect.TargetElement == ElementType.None) return;

        if (string.IsNullOrEmpty(targetTower)) return;

        if (!_towerCountBuffs.ContainsKey(targetTower))
            _towerCountBuffs[targetTower] = new List<TowerCountBuff>();

        _towerCountBuffs[targetTower].Add(new TowerCountBuff
        {
            EffectType = effect.EffectType,
            CountTargetTowerCode = effect.TargetTowerCode,
            CountTargetElement = effect.TargetElement,
            ValuePerTower = value,
            TargetStatusEffect = effect.TargetStatusEffect
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
                if (effect.ValueType == 0) // Flat (고정값 합산)
                {
                    modifier.PotencyFlat += value;
                }
                else // Percentage (비율 곱연산/합연산)
                {
                    modifier.PotencyMultiplier += value / 100f;
                }
                break;
            case EBookEffectType.IncreaseStatusEffectDuration:
                modifier.DurationBonus += value; // 초 단위로 직접 추가
                break;
            case EBookEffectType.ModifyStatusEffectValue:
                 // 기절 수치 감소 등 (보통 %로 감소)
                 if (effect.ValueType == 0) modifier.PotencyFlat -= value;
                 else modifier.PotencyMultiplier -= value / 100f;
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

    public int GetTowerCount(string towerCode, ElementType element = ElementType.None)
    {
        Tower[] towers = FindObjectsOfType<Tower>();

        if (element != ElementType.None)
        {
            int count = 0;
            foreach (var t in towers)
            {
                if (t.towerData != null && t.towerData.ElementType == element) count++;
            }
            return count;
        }

        if (!string.IsNullOrEmpty(towerCode))
        {
            int count = 0;
            foreach (var t in towers)
            {
                if (t.TowerName == towerCode) count++;
            }
            return count;
        }

        return towers.Length;
    }

    public void InvalidateCache()
    {
        _cacheInvalidated = true;
        _towerBuffCache.Clear();
    }

    public TowerFinalStats CalculateFinalStats(Tower tower)
    {
        string towerCode = tower.towerData?.Name ?? "DefaultTower";
        ElementType element = tower.towerData.ElementType;

        var buffData = GetTowerBuffs(towerCode, element);

        float baseDamage = tower.Damage;
        float baseAttackSpeed = tower.AttackSpeed;
        float baseRange = tower.Range + tower.GetBonusRange();
        float baseCrit = tower.CriticalHit;

        var finalStats = new TowerFinalStats
        {
            Damage = CalculateFinalDamage(towerCode, baseDamage, buffData),
            AttackSpeed = CalculateFinalAttackSpeed(towerCode, baseAttackSpeed, buffData),
            Range = CalculateFinalRange(baseRange, buffData),
            CritChance = CalculateFinalCritChance(towerCode, baseCrit, buffData),
            ExcessCritDamageMultiplier = CalculateExcessCritDamageMultiplier(towerCode, baseCrit, buffData),
        };

        return finalStats;
    }

    private float CalculateFinalDamage(string towerCode, float baseDamage, TowerBuffData buffData)
    {
        float finalDamage = baseDamage * buffData.FinalDamageMultiplier;

        // 타워 개수 기반 동적 버프
        float dynamicBonus = GetTowerCountBasedBuff(towerCode, EBookEffectType.IncreaseDamagePerTowerCount);
        if (dynamicBonus > 0)
        {
            finalDamage *= (1f + dynamicBonus / 100f);
        }

        // 시너지 효과 (달빛검 등)
        if (buffData.HasUniqueEffects.Contains(EBookEffectType.SynergyDamageBoost))
        {
            float synergyBonus = GetSynergyBonus(towerCode);
            if (synergyBonus > 0)
            {
                finalDamage *= (1f + synergyBonus);
            }
        }

        return finalDamage;
    }

    private float GetSynergyBonus(string towerCode)
    {
        float bonus = 0f;
        float valuePercent = _synergyDamageBonusValue / 100f;

        // 달빛검 (BR5) 로직: 달빛타워(L3MOO) <-> 강철타워(L3MET)
        if (towerCode == "L3MOO")
        {
            int count = GetTowerCount("L3MET");
            bonus += count * valuePercent;
        }
        else if (towerCode == "L3MET")
        {
            int count = GetTowerCount("L3MOO");
            bonus += count * valuePercent;
        }

        return bonus;
    }

    private float CalculateFinalAttackSpeed(string towerCode, float baseAttackSpeed, TowerBuffData buffData)
    {
        float finalSpeed = baseAttackSpeed * buffData.AttackSpeedMultiplier;

        // 타워 개수 기반 동적 버프
        float dynamicBonus = GetTowerCountBasedBuff(towerCode, EBookEffectType.IncreaseAttackSpeedPerTowerCount);
        if (dynamicBonus > 0)
        {
            finalSpeed *= (1f + dynamicBonus / 100f);
        }

        return finalSpeed;
    }

    private float CalculateFinalRange(float baseRange, TowerBuffData buffData)
    {
        // 고정 사거리가 설정되어 있으면 우선 적용
        if (buffData.OverrideRange > 0)
        {
            return buffData.OverrideRange;
        }

        return baseRange * buffData.RangeMultiplier;
    }

    private float CalculateFinalCritChance(string towerCode, float baseCrit, TowerBuffData buffData)
    {
        float finalCrit = baseCrit + buffData.CritChanceBonus;

        // 타워 개수 기반 동적 버프
        float dynamicBonus = GetTowerCountBasedBuff(towerCode, EBookEffectType.IncreaseCritChancePerTowerCount);
        finalCrit += dynamicBonus;

        // 초과 치명타를 데미지로 변환하는 특수 효과가 있으면 100%로 제한
        if (buffData.HasUniqueEffects.Contains(EBookEffectType.ConvertExcessCritToDamage))
        {
            return Mathf.Min(finalCrit, 100f);
        }

        return finalCrit;
    }

    private float CalculateExcessCritDamageMultiplier(string towerCode, float baseCrit, TowerBuffData buffData)
    {
        if (!buffData.HasUniqueEffects.Contains(EBookEffectType.ConvertExcessCritToDamage))
            return 1f;

        float totalCrit = baseCrit + buffData.CritChanceBonus;
        float dynamicBonus = GetTowerCountBasedBuff(towerCode, EBookEffectType.IncreaseCritChancePerTowerCount);
        totalCrit += dynamicBonus;

        if (totalCrit > 100f)
        {
            float excessCrit = totalCrit - 100f;
            return 1f + (excessCrit / 100f);
        }

        return 1f;
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
    public float PotencyMultiplier = 1f;    // Slow, Stun 등의 효과 수치 배수 (기본 1f + a)
    public float PotencyFlat = 0f;          // 효과 수치 고정값 추가 (기본 0f)
    public float DurationBonus = 0f;        // 지속시간 추가
}

[Serializable]
public class TowerCountBuff
{
    public EBookEffectType EffectType;
    public string CountTargetTowerCode; // 개수를 세는 대상 타워
    public ElementType CountTargetElement; // 개수를 세는 대상 속성 (None이 아니면 우선 사용)
    public float ValuePerTower; // 타워 하나당 증가값
    public StatusEffectType TargetStatusEffect; // 상태이상 타겟
}

internal struct CachedTowerBuffs
{
    public float FinalDamageMultiplier;
    public float AttackSpeedMultiplier;
    public float RangeMultiplier;
    public float CritChanceBonus;
    public float OverrideRange;
}

[Serializable]
public struct TowerFinalStats
{
    public float Damage;
    public float AttackSpeed;
    public float Range;
    public float CritChance;
    public float ExcessCritDamageMultiplier;
}

#endregion
