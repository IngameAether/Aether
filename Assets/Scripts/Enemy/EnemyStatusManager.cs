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
    // private readonly Dictionary<StatusEffectType, StatusEffect> _activeEffects = new Dictionary<StatusEffectType, StatusEffect>();
    private readonly Dictionary<StatusEffectType, float> _effectCooldowns = new Dictionary<StatusEffectType, float>();
    private readonly Dictionary<StatusEffectType, Coroutine> _effectCoroutines = new Dictionary<StatusEffectType, Coroutine>();

    // 상태이상 게이지 관리 변수 
    private Dictionary<StatusEffectType, float> _currentGauges = new Dictionary<StatusEffectType, float>();
    private Dictionary<StatusEffectType, float> _maxGauges = new Dictionary<StatusEffectType, float>();

    // 공개 프로퍼티 (부패 효과용)
    public float DamageTakenMultiplier { get; private set; } = 1.0f;

    private void Awake()
    {
        _enemyMovement = GetComponent<EnemyMovement>();
        _normalEnemy = GetComponent<NormalEnemy>();
    }

    // Start() 함수에서 게이지 초기화 호출
    private void Start()
    {
        InitializeGauges();
    }
    //private void Update()
    //{
    //    HandleEffectTimers();
    //}

    ///// 함수의 이름과 역할 변경: 상태이상 '누적치'를 받아 게이지를 깎는 역할
    //public void AddBuildup(StatusEffectType statusEffect)
    //{
    //    // 이 적이 해당 상태이상에 대한 게이지를 가지고 있는지 확인
    //    if (!_currentGauges.ContainsKey(type)) return;

    //    // 게이지를 깎습니다.
    //    _currentGauges[type] -= buildupValue;
    //    Debug.Log($"{_normalEnemy.name}의 {type} 게이지: {_currentGauges[type]}/{_maxGauges[type]} (-{buildupValue})");

    //    // 게이지가 0 이하가 되었을 때만 아래 로직을 실행
    //    if (_currentGauges[type] <= 0)
    //    {
    //        // 기존 TryApplyStatusEffect의 로직을 이곳으로 가져옵니다 

    //        // 쿨타임 중이면 발동하지 않고 게이지도 초기화하지 않음 (다음에 다시 시도)
    //        if (_effectCooldowns.ContainsKey(type)) return;

    //        // 이미 걸린 효과와 비교해서 더 강할 때만 덮어쓰기
    //        if (_activeEffects.TryGetValue(type, out StatusEffect existingEffect))
    //        {
    //            bool isNewEffectStronger = effect.Value > existingEffect.Value ||
    //                                     (Mathf.Approximately(effect.Value, existingEffect.Value) && effect.Duration > existingEffect.Duration);
    //            if (isNewEffectStronger)
    //            {
    //                RemoveStatusEffect(type); // 기존 효과 제거
    //            }
    //            else
    //            {
    //                _currentGauges[type] = _maxGauges[type]; // 기존 효과가 더 강하면 발동 안하고 게이지만 초기화
    //                return;
    //            }
    //        }

    //        // CC 지속시간 감소 적용
    //        float finalDuration = effect.Duration;
    //        if (type == StatusEffectType.Slow || type == StatusEffectType.Stun || type == StatusEffectType.Fear || type == StatusEffectType.Paralyze)
    //        {
    //            finalDuration = _normalEnemy.CalculateReducedCCDuration(effect.Duration);
    //        }

    //        // 최종 효과 적용
    //        var effectToApply = new StatusEffect(type, finalDuration, effect.Value, effect.SourcePosition);
    //        _activeEffects.Add(type, effectToApply);
    //        ApplyEffectLogic(effectToApply);
    //        Debug.Log($"★★★ [{type}] 효과 발동! (지속시간: {finalDuration:F2}초) ★★★");

    //        // 게이지를 다시 최대로 초기화
    //        _currentGauges[type] = _maxGauges[type];
    //    }
    //}

    // EnemyData로부터 게이지 값을 읽어오는 함수
    private void InitializeGauges()
    {
        if (_normalEnemy.enemyData == null) return;
        foreach (var gaugeInfo in _normalEnemy.enemyData.statusGauges)
        {
            _maxGauges[gaugeInfo.type] = gaugeInfo.maxValue;
            _currentGauges[gaugeInfo.type] = gaugeInfo.maxValue;
        }
    }

    ///// <summary>
    ///// 타워가 이 함수를 호출하여 상태 이상을 적용합니다.
    ///// </summary>
    //public void TryApplyStatusEffect(StatusEffect newEffect)
    //{
    //    var type = newEffect.Type;

    //    // 쿨타임 중인지 먼저 확인
    //    if (_effectCooldowns.ContainsKey(type))
    //    {
    //        // 쿨타임 감소 로직 (원하시면 유지)
    //        // _effectCooldowns[type] = Mathf.Max(0, _effectCooldowns[type] - 1.0f);
    //        return;
    //    }

    //    // 플로우차트 로직 적용
    //    // 동일한 상태이상이 이미 활성화되어 있는지 확인
    //    if (_activeEffects.TryGetValue(type, out StatusEffect existingEffect))
    //    {
    //        // 새로 들어온 효과가 더 강한지 확인 (Value > Duration 순)
    //        bool isNewEffectStronger = newEffect.Value > existingEffect.Value ||
    //                                 (Mathf.Approximately(newEffect.Value, existingEffect.Value) && newEffect.Duration > existingEffect.Duration);

    //        if (isNewEffectStronger)
    //        {
    //            // 기존 효과를 제거하고 새로운 효과로 교체
    //            RemoveStatusEffect(type);
    //        }
    //        else
    //        {
    //            // 기존 효과가 더 강하므로 무시
    //            return;
    //        }
    //    }

    //    // CC 지속시간 감소 적용
    //    float finalDuration = newEffect.Duration;
    //    if (type == StatusEffectType.Slow || type == StatusEffectType.Stun || type == StatusEffectType.Fear || type == StatusEffectType.Paralyze)
    //    {
    //        finalDuration = _normalEnemy.CalculateReducedCCDuration(newEffect.Duration);
    //    }

    //    // 새로운 효과 적용
    //    var effectToApply = new StatusEffect(type, finalDuration, newEffect.Value, newEffect.SourcePosition);
    //    _activeEffects.Add(type, effectToApply);
    //    ApplyEffectLogic(effectToApply);
    //    Debug.Log($"[{type}] 효과 적용 (최종 지속시간: {finalDuration:F2}초)");
    //}

    ///// <summary>
    ///// 상태 이상의 실제 효과를 적용하는 내부 함수
    ///// </summary>
    //private void ApplyEffectLogic(StatusEffect effect)
    //{
    //    switch (effect.Type)
    //    {
    //        case StatusEffectType.Slow:
    //            _enemyMovement.ChangeSpeedMultiplier(1.0f - (effect.Value / 100f));
    //            break;
    //        case StatusEffectType.Stun:
    //            _enemyMovement.SetStun(true);
    //            break;
    //        case StatusEffectType.Burn:
    //            var burnCoroutine = StartCoroutine(BurnCoroutine(effect));
    //            _effectCoroutines.Add(effect.Type, burnCoroutine);
    //            break;
    //        case StatusEffectType.Rot:
    //            DamageTakenMultiplier += (effect.Value / 100f);
    //            break;
    //        case StatusEffectType.Fear:
    //            _enemyMovement.ApplyFear(effect.SourcePosition, effect.Duration);
    //            break;
    //        case StatusEffectType.Bleed:
    //            StartCoroutine(BleedCoroutine(effect));
    //            break;
    //    }
    //}

    /// <summary>
    /// 상태 이상 효과를 제거하는 내부 함수
    /// </summary>
    //private void RemoveStatusEffect(StatusEffectType type)
    //{
    //    if (!_activeEffects.ContainsKey(type)) return;

    //    StatusEffect effect = _activeEffects[type];

    //    // 적용됐던 효과 되돌리기
    //    switch (type)
    //    {
    //        case StatusEffectType.Slow:
    //            _enemyMovement.ResetSpeedMultiplier();
    //            break;
    //        case StatusEffectType.Stun:
    //            _enemyMovement.SetStun(false);
    //            break;
    //        case StatusEffectType.Rot:
    //            DamageTakenMultiplier -= (effect.Value / 100f);
    //            break;
    //        case StatusEffectType.Paralyze:
    //            // _enemyAbilities.SetParalyze(false);
    //            break;
    //        case StatusEffectType.Fear:
    //            _enemyMovement.RemoveFear();
    //            break;
    //        case StatusEffectType.Burn:
    //        case StatusEffectType.Bleed:
    //            if (_effectCoroutines.ContainsKey(type))
    //            {
    //                StopCoroutine(_effectCoroutines[type]);
    //                _effectCoroutines.Remove(type);
    //            }
    //            break;
    //    }

    //    _activeEffects.Remove(type);
    //    _effectCooldowns[type] = 5.0f; // 5초 쿨타임 시작
    //    // HideStatusIcon(type); // UI 아이콘 숨김 로직
    //    Debug.Log($"[{type}] 효과가 종료. 5초 쿨타임 시작.");
    //}

    /// <summary>
    /// 사망 시 모든 효과를 정리하는 공개 함수 (NormalEnemy에서 호출)
    /// </summary>
    //public void ClearAllEffectsOnDeath()
    //{
    //    foreach (var coroutine in _effectCoroutines.Values)
    //    {
    //        StopCoroutine(coroutine);
    //    }
    //    _effectCoroutines.Clear();
    //    _activeEffects.Clear();
    //    _effectCooldowns.Clear();
    //}

    // --- 시간 기반 로직 처리 ---
    //private void HandleEffectTimers()
    //{
    //    // 활성화된 효과의 남은 시간 감소
    //    if (_activeEffects.Count > 0)
    //    {
    //        foreach (var type in _activeEffects.Keys.ToList())
    //        {
    //            var effect = _activeEffects[type];
    //            effect.RemainingTime -= Time.deltaTime;
    //            if (effect.RemainingTime <= 0)
    //            {
    //                // 출혈은 코루틴이 스스로를 제거하므로 예외
    //                if (type != StatusEffectType.Bleed)
    //                {
    //                    RemoveStatusEffect(type);
    //                }
    //            }
    //        }
    //    }

    //    // 쿨타임 감소
    //    if (_effectCooldowns.Count > 0)
    //    {
    //        foreach (var type in _effectCooldowns.Keys.ToList())
    //        {
    //            _effectCooldowns[type] -= Time.deltaTime;
    //            if (_effectCooldowns[type] <= 0)
    //            {
    //                _effectCooldowns.Remove(type);
    //            }
    //        }
    //    }
    //}

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

    //private IEnumerator BurnCoroutine(StatusEffect effect)
    //{
    //    float tickInterval = 0.5f; // 0.5초마다 데미지
    //    int tickCount = Mathf.FloorToInt(effect.Duration / tickInterval); // 총 몇 번의 데미지를 줄지 계산
    //    if (tickCount <= 0) yield break;

    //    // [핵심] GameManager에서 현재 웨이브 정보를 가져와 총 데미지를 계산
    //    int currentWave = GameManager.Instance.CurrentWave;
    //    float totalDamage = currentWave * effect.Value; // 예: 5웨이브 * 10 = 50의 총 데미지

    //    // 한 틱당 입힐 데미지
    //    float damagePerTick = totalDamage / tickCount;

    //    for (int i = 0; i < tickCount; i++)
    //    {
    //        // 이 효과가 아직 유효한지 매 틱마다 확인
    //        if (!_activeEffects.ContainsKey(effect.Type)) yield break;

    //        _normalEnemy.TakeDamage(damagePerTick);
    //        Debug.Log($"화상 데미지! {damagePerTick}");
    //        yield return new WaitForSeconds(tickInterval);
    //    }
    //}

    //// BleedCoroutine 코루틴 수정
    //private IEnumerator BleedCoroutine(StatusEffect effect)
    //{
    //    // effect.Duration(1.5초) 만큼 기다립니다.
    //    yield return new WaitForSeconds(effect.Duration);

    //    // 이 효과가 아직 유효하다면 데미지를 적용합니다.
    //    if (_activeEffects.ContainsKey(effect.Type))
    //    {
    //        // effect.Value(10)를 이용해 최대 체력의 10% 데미지 계산
    //        float damage = _normalEnemy.maxHealth * (effect.Value / 100f);
    //        _normalEnemy.TakeDamage(damage);
    //        Debug.Log($"출혈 데미지! {damage}");

    //        // 데미지를 준 후에는 즉시 효과를 제거합니다.
    //        RemoveStatusEffect(effect.Type);
    //    }
    //}
}
