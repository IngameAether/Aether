public enum EBookEffectType
{
    // Growing 관련 타입
    WaveAether,
    SellBonus,
    LightElementChance,
    DarkElementChance,
    BossRewardDouble,
    LightElementRate,
    DarkElementRate,
    KillAetherBonus,
    ElementMaxUpgrade,
    DirectTowerPlace,
    ExtraLife,

    // Battle 관련 타입
    // --- 기본 & 범용 효과 ---
    IncreaseFinalDamage,      // 최종 데미지 증가
    IncreaseAttackSpeed,      // 공격 속도 증가
    IncreaseAttackDuration,   // 공격 지속시간 증가
    IncreaseRange,            // 사거리 (반경) 증가
    IncreaseCritChance,       // 치명타 확률 증가
    SetRange,                 // 사거리 '고정값'으로 설정 (무한대 표현용)

    // --- 상태이상(Status Effect) 관련 효과 ---
    IncreaseStatusEffectDamage,     // 상태이상 데미지 % 증가 (예: 화상 데미지 2배 -> Value: 100, ValueType: Percentage)
    IncreaseStatusEffectPotency,    // 상태이상 수치/확률 % 증가 (예: 둔화율, 출혈 수치)
    SetStatusEffectPotency,         // 상태이상 수치/확률 '고정값'으로 설정 (예: 둔화율 75%)
    IncreaseStatusEffectDuration,   // 상태이상 지속시간 증가
    SetStatusEffectTickRate,        // 상태이상 데미지 적용 주기 '고정값'으로 설정 (예: 0.5초)
    ModifyStatusEffectValue,        // 상태이상 수치 변경 (적 기절 수치 감소 등)

    // --- 조건부 & 특수 효과 ---
    IncreaseDamagePerTowerCount,      // 특정 타워 개수당 데미지 증가
    IncreaseDurationPerTowerCount,    // 특정 타워 개수당 지속시간 증가
    IncreaseAttackSpeedPerTowerCount, // 특정 타워 개수당 공격속도 증가
    IncreaseCritChancePerTowerCount,  // 특정 타워 개수당 치명타 확률 증가

    // --- 고유(Unique) 효과 ---
    // 여러 효과가 복잡하게 얽혀있어 하나의 enum으로 관리하는게 편한 경우
    ConvertExcessCritToDamage,    // 초과된 치명타 확률을 최종 데미지로 전환
    SynergyDamageBoost,           // 특정 타워 주변의 다른 타워 데미지 증가 (달빛+강철)
    AutoPlaceTowerOnBossKill,     // 보스 처치 시 특정 타워 자동 설치
}
