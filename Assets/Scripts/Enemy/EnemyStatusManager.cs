using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 모든 상태 이상의 적용, 해제, 쿨타임, 효과 발동을 총괄하는 컨트롤 타워 스크립트입니다.
/// </summary>
public class EnemyStatusManager : MonoBehaviour
{
    // 외부 컴포넌트 참조
    private EnemyMovement _enemyMovement;
    private NormalEnemy _normalEnemy;

    // 상태 이상 관리 변수
    private readonly Dictionary<StatusEffectType, StatusEffect> _activeEffects = new Dictionary<StatusEffectType, StatusEffect>();
    private readonly Dictionary<StatusEffectType, float> _effectCooldowns = new Dictionary<StatusEffectType, float>();
    private readonly Dictionary<StatusEffectType, Coroutine> _effectCoroutines = new Dictionary<StatusEffectType, Coroutine>();

    // 공개 프로퍼티 (부패 효과용)
    public float DamageTakenMultiplier { get; private set; } = 1.0f;

    private void Awake()
    {
        _enemyMovement = GetComponent<EnemyMovement>();
        _normalEnemy = GetComponent<NormalEnemy>();
    }

    private void Update()
    {
        HandleEffectTimers();
    }

    /// <summary>
    /// 타워가 이 함수를 호출하여 상태 이상을 적용합니다.
    /// </summary>
    public void TryApplyStatusEffect(StatusEffect newEffect)
    {
        var type = newEffect.Type;

        // 쿨타임 중인지 먼저 확인
        if (_effectCooldowns.ContainsKey(type))
        {
            // 쿨타임 감소 로직 (원하시면 유지)
            // _effectCooldowns[type] = Mathf.Max(0, _effectCooldowns[type] - 1.0f);
            return;
        }

        // 플로우차트 로직 적용
        // 동일한 상태이상이 이미 활성화되어 있는지 확인
        if (_activeEffects.TryGetValue(type, out StatusEffect existingEffect))
        {
            // 새로 들어온 효과가 더 강한지 확인 (Value > Duration 순)
            bool isNewEffectStronger = newEffect.Value > existingEffect.Value ||
                                     (Mathf.Approximately(newEffect.Value, existingEffect.Value) && newEffect.Duration > existingEffect.Duration);

            if (isNewEffectStronger)
            {
                // 기존 효과를 제거하고 새로운 효과로 교체
                RemoveStatusEffect(type);
            }
            else
            {
                // 기존 효과가 더 강하므로 무시
                return;
            }
        }

        // CC 지속시간 감소 적용
        float finalDuration = newEffect.Duration;
        if (type == StatusEffectType.Slow || type == StatusEffectType.Stun || type == StatusEffectType.Fear || type == StatusEffectType.Paralyze)
        {
            finalDuration = _normalEnemy.CalculateReducedCCDuration(newEffect.Duration);
        }

        // 새로운 효과 적용
        var effectToApply = new StatusEffect(type, finalDuration, newEffect.Value, newEffect.SourcePosition);
        _activeEffects.Add(type, effectToApply);
        ApplyEffectLogic(effectToApply);
        Debug.Log($"[{type}] 효과 적용 (최종 지속시간: {finalDuration:F2}초)");
    }

    /// <summary>
    /// 상태 이상의 실제 효과를 적용하는 내부 함수
    /// </summary>
    private void ApplyEffectLogic(StatusEffect effect)
    {
        switch (effect.Type)
        {
            case StatusEffectType.Slow:
                _enemyMovement.ChangeSpeedMultiplier(1.0f - (effect.Value / 100f));
                break;
            case StatusEffectType.Stun:
                _enemyMovement.SetStun(true);
                break;
            case StatusEffectType.Burn:
                var burnCoroutine = StartCoroutine(BurnCoroutine(effect));
                _effectCoroutines.Add(effect.Type, burnCoroutine);
                break;
            case StatusEffectType.Rot:
                DamageTakenMultiplier += (effect.Value / 100f);
                break;
            case StatusEffectType.Paralyze:
                // _enemyAbilities.SetParalyze(true); // 적 스킬 스크립트가 있다면 연동
                break;
            case StatusEffectType.Fear:
                _enemyMovement.ApplyFear(effect.SourcePosition, effect.Duration);
                break;
            case StatusEffectType.Bleed:
                var bleedCoroutine = StartCoroutine(BleedCoroutine(effect));
                _effectCoroutines.Add(effect.Type, bleedCoroutine);
                break;
        }
    }

    /// <summary>
    /// 상태 이상 효과를 제거하는 내부 함수
    /// </summary>
    private void RemoveStatusEffect(StatusEffectType type)
    {
        if (!_activeEffects.ContainsKey(type)) return;

        StatusEffect effect = _activeEffects[type];

        // 적용됐던 효과 되돌리기
        switch (type)
        {
            case StatusEffectType.Slow:
                _enemyMovement.ResetSpeedMultiplier();
                break;
            case StatusEffectType.Stun:
                _enemyMovement.SetStun(false);
                break;
            case StatusEffectType.Rot:
                DamageTakenMultiplier -= (effect.Value / 100f);
                break;
            case StatusEffectType.Paralyze:
                // _enemyAbilities.SetParalyze(false);
                break;
            case StatusEffectType.Fear:
                _enemyMovement.RemoveFear();
                break;
            case StatusEffectType.Burn:
            case StatusEffectType.Bleed:
                if (_effectCoroutines.ContainsKey(type))
                {
                    StopCoroutine(_effectCoroutines[type]);
                    _effectCoroutines.Remove(type);
                }
                break;
        }

        _activeEffects.Remove(type);
        _effectCooldowns[type] = 5.0f; // 5초 쿨타임 시작
        // HideStatusIcon(type); // UI 아이콘 숨김 로직
        Debug.Log($"[{type}] 효과가 종료. 5초 쿨타임 시작.");
    }

    /// <summary>
    /// 사망 시 모든 효과를 정리하는 공개 함수 (NormalEnemy에서 호출)
    /// </summary>
    public void ClearAllEffectsOnDeath()
    {
        foreach (var coroutine in _effectCoroutines.Values)
        {
            StopCoroutine(coroutine);
        }
        _effectCoroutines.Clear();
        _activeEffects.Clear();
        _effectCooldowns.Clear();
    }

    // --- 시간 기반 로직 처리 ---
    private void HandleEffectTimers()
    {
        // 활성화된 효과의 남은 시간 감소
        if (_activeEffects.Count > 0)
        {
            foreach (var type in _activeEffects.Keys.ToList())
            {
                var effect = _activeEffects[type];
                effect.RemainingTime -= Time.deltaTime;
                if (effect.RemainingTime <= 0)
                {
                    // 출혈은 코루틴이 스스로를 제거하므로 예외
                    if (type != StatusEffectType.Bleed)
                    {
                        RemoveStatusEffect(type);
                    }
                }
            }
        }

        // 쿨타임 감소
        if (_effectCooldowns.Count > 0)
        {
            foreach (var type in _effectCooldowns.Keys.ToList())
            {
                _effectCooldowns[type] -= Time.deltaTime;
                if (_effectCooldowns[type] <= 0)
                {
                    _effectCooldowns.Remove(type);
                }
            }
        }
    }

    private void HandleEffectCooldowns()
    {
        if (_effectCooldowns.Count == 0) return;

        foreach (var type in _effectCooldowns.Keys.ToList())
        {
            _effectCooldowns[type] -= Time.deltaTime;
            if (_effectCooldowns[type] <= 0)
            {
                _effectCooldowns.Remove(type);
            }
        }
    }

    // --- 상태 이상별 코루틴 ---
    private IEnumerator BurnCoroutine(StatusEffect effect)
    {
        float tickCount = effect.Duration / 0.5f;
        if (tickCount <= 0) yield break;

        float damagePerTick = effect.Value / tickCount;

        while (_activeEffects.ContainsKey(effect.Type))
        {
            _normalEnemy.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator BleedCoroutine(StatusEffect effect)
    {
        yield return new WaitForSeconds(1.0f);

        if (_activeEffects.ContainsKey(effect.Type))
        {
            _normalEnemy.TakeDamage(effect.Value);
            _activeEffects.Remove(effect.Type);
            _effectCooldowns[effect.Type] = 5.0f;
            // HideStatusIcon(effect.Type);
        }
        _effectCoroutines.Remove(effect.Type);
    }
}
