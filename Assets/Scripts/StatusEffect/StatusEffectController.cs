using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    public static StatusEffectController Instance { get; private set; }

    private void Awake()
    {
        // 인스턴스가 이미 있는지 확인해서 중복 피함
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private float slowCoef;
    private float stunCoef;
    private float burnCoef;
    private float bleedCoef;

    private void Start()
    {
        // 초기화 및 첫 업데이트
        UpdateModifiers();
    }

    private void OnEnable()
    {
        MagicBookBuffSystem.OnBuffsUpdated += UpdateModifiers;
    }

    private void OnDisable()
    {
        MagicBookBuffSystem.OnBuffsUpdated -= UpdateModifiers;
    }

    private void ResetModifiers()
    {
        slowCoef = 0.5f;
        stunCoef = 1f;
        burnCoef = 1f;
        bleedCoef = 0f;
    }

    private void UpdateModifiers()
    {
        ResetModifiers();

        if (MagicBookBuffSystem.Instance == null) return;

        // 1. Slow (둔화)
        // BR7 (서리 내림): 둔화율 75% 고정 (SetStatusEffectPotency)
        float fixedSlowPotency = MagicBookBuffSystem.Instance.GetGlobalBuffValue(EBookEffectType.SetStatusEffectPotency);
        if (fixedSlowPotency > 0)
        {
            // 둔화율이 75%라면 이동속도는 25% (0.25)가 되어야 함.
            slowCoef = 1f - (fixedSlowPotency / 100f);
        }
        else
        {
            // 둔화 효과 증가 버프 처리
            var slowMod = MagicBookBuffSystem.Instance.GetStatusEffectModifier(StatusEffectType.Slow);
            if (slowMod.PotencyMultiplier > 1f)
            {
                // 기본 둔화율(0.5)을 기준으로 효과 증가
                float baseSlowEffect = 1f - slowCoef; // 0.5
                float buffedSlowEffect = baseSlowEffect * slowMod.PotencyMultiplier;
                slowCoef = 1f - Mathf.Clamp01(buffedSlowEffect);
            }
        }

        // 2. Stun (기절)
        // 기절 요구치(Threshold)에 대한 계수. 
        // BR9(대규모 혼란): 기절 수치 1/2로 감소 -> 적을 기절시키기 쉬워짐 -> 요구량 0.5배
        var stunMod = MagicBookBuffSystem.Instance.GetStatusEffectModifier(StatusEffectType.Stun);
        if (stunMod.PotencyMultiplier < 1f)
        {
             stunCoef *= stunMod.PotencyMultiplier;
        }

        // 3. Burn (화상)
        // 데미지 배율 적용
        var burnMod = MagicBookBuffSystem.Instance.GetStatusEffectModifier(StatusEffectType.Burn);
        burnCoef *= burnMod.DamageMultiplier;

        // 4. Bleed (출혈)
        // 출혈 계수(추가 게이지)에 고정값 보너스 합산
        var bleedMod = MagicBookBuffSystem.Instance.GetStatusEffectModifier(StatusEffectType.Bleed);
        bleedCoef += bleedMod.PotencyFlat;
    }

    public void SetSlowCoef(float coef) => slowCoef = coef;
    public void SetStunCoef(float coef) => stunCoef = coef;
    public void SetBurnCoef(float coef) => burnCoef = coef;
    public void SetBleedCoef(float coef) => bleedCoef = coef;

    public float GetSlowSpeed(float speed)
    {
        return speed * slowCoef;
    }

    public float GetStunthresholdCoef(float threshold)
    {
        return threshold * stunCoef;
    }

    public float GetBurnDamage()
    {
        return (float)(GameManager.Instance.CurrentWave * 5) * burnCoef;
    }

    public float GetAdditionalBleedGauge()
    {
        return bleedCoef;
    }
}
