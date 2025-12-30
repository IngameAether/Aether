using UnityEngine;

public class TowerStatManager : MonoBehaviour
{
    public static TowerStatManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // MagicBookManager의 방송을 구독합니다.
        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
        }
    }

    private void OnDisable()
    {
        // 구독을 해제합니다.
        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied -= HandleBookEffectApplied;
        }
    }

    /// 마법책 효과 방송을 받았을 때 실행될 메인 함수
    private void HandleBookEffectApplied(BookEffect effect, float finalValue)
    {
        // 효과 종류에 따라 다른 함수를 호출
        switch (effect.EffectType)
        {
            case EBookEffectType.IncreaseStatusEffectPotency:
                ApplyStatusEffectPotencyBuff(effect, finalValue); 
                break;

            case EBookEffectType.IncreaseDurationPerTowerCount:
                ApplyDurationPerTowerCountBuff(effect, finalValue); 
                break;
        }
    }

    /// 특정 속성 타워 개수에 비례하여 특정 타워의 지속시간을 늘려주는 함수
    private void ApplyDurationPerTowerCountBuff(BookEffect effect, float finalValue)
    {
        // 필요한 정보 가져오기
        ElementType elementToCount = effect.TargetElement; // 개수를 셀 타워의 속성 (Water)
        string targetTowerCode = effect.TargetTowerCode; // 버프를 받을 타워의 ID (초월타워)

        // 씬에 있는 모든 타워를 찾음
        Tower[] allTowers = FindObjectsOfType<Tower>();

        // '다른' 물 속성 타워의 개수를 셈
        int waterTowerCount = 0;
        foreach (Tower tower in allTowers)
        {
            // 타워의 속성이 'Water'이고, 자기 자신이 '초월타워'가 아닐 경우
            if (tower.towerData.ElementType == elementToCount && tower.towerData.ID != targetTowerCode)
            {
                waterTowerCount++;
            }
        }

        if (waterTowerCount == 0) return; // 셀 타워가 없으면 종료

        // 최종 보너스 값 계산
        float totalDurationBonus = waterTowerCount * finalValue; // 예: 3개 * 0.2초 = 0.6초

        // 버프를 받을 '초월타워'를 찾음
        foreach (Tower tower in allTowers)
        {
            if (tower.towerData.ID == targetTowerCode)
            {
                // 6. 해당 타워에 보너스 적용
                tower.AddBonusEffectDuration(totalDurationBonus); // Tower.cs에 이 함수가 필요합니다.
                break; // 초월타워는 하나뿐이므로 찾으면 루프 종료
            }
        }
    }

    /// 특정 상태이상의 위력(누적치)을 강화하는 로직
    private void ApplyStatusEffectPotencyBuff(BookEffect effect, float finalValue)
    {
        // 마법책이 강화할 특정 상태이상을 지정했는지 확인 (예: Slow, Stun)
        if (effect.TargetStatusEffect == StatusEffectType.None)
        {
            // 지정하지 않았다면 아무것도 하지 않음
            return;
        }

        // 씬에 있는 모든 타워를 찾습니다.
        Tower[] allTowers = FindObjectsOfType<Tower>();

        foreach (Tower tower in allTowers)
        {
            // [핵심] 타워의 상태이상 타입이 마법책이 지정한 타겟 상태이상과 일치하는지 확인합니다.
            //if (tower.towerData.effectType == effect.TargetStatusEffect)
            //{
            //    // % 증가 로직 (finalValue가 20이면 20% 증가)
            //    if (effect.ValueType == EValueType.Percentage)
            //    {
            //        // 타워의 '기본' 누적치를 기준으로 증가량을 계산
            //        float increaseAmount = tower.towerData.effectBuildup * (finalValue / 100f);
            //        tower.AddBonusBuildup(increaseAmount);
            //    }
            //    // 고정값 증가 로직
            //    else if (effect.ValueType == EValueType.Flat)
            //    {
            //        tower.AddBonusBuildup(finalValue);
            //    }
            //}
        }
    }
}
