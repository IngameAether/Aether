public enum EBookEffectType
{
    // Growing 관련 타입
    WaveAether,                     // GN1 (자연의 은혜: 웨이브 클리어 시 에테르 획득)
    SellBonus,                      // GN2 (완전 연소: 타워 판매 시 에테르 획득)
    BossRewardDouble,               // GN3 (계단식 성장: 보스 웨이브 에테르 2배)
    ExtraLife,                      // GN4 (자연 치유: 웨이브 클리어 시 Life 회복)
    FullLife,                       // GR5 (강인한 신체: 최대/현재 Life 20 고정)
    IncreaseSelectableBookCount,    // GN6 (책벌레: 선택지 +1)
    SupportFund,                    // GN5 (지원금 도착: 에테르 획득)
    KillAetherBonus,                // GE1 (폭식: 적 격퇴 시 에테르 +5)

    // Battle 관련 타입
    // --- 기본 & 범용 효과 ---
    FireTowerSummonEther,     // GR1 (원소응용학: 불)
    WaterTowerSummonEther,    // GR2 (원소응용학: 물)
    AirTowerSummonEther,      // GR3 (원소응용학: 대기)
    EarthTowerSummonEther,    // GR4 (원소응용학: 대지)

    IncreaseFinalDamage,      // BN1~4(기초원소학), BR2(과냉각), BR3(악천후): 최종 데미지 증가
    IncreaseAttackSpeed,      // BR1 (쾌청: 태양/생명 타워 공속 증가)
    IncreaseAttackDuration,   // 공격 지속시간 증가
    IncreaseRange,            // 사거리 (반경) 증가
    IncreaseCritChance,       // 치명타 확률 증가
    SetRange,                 // 사거리 '고정값'으로 설정 (무한대 표현용)

    // --- 상태이상(Status Effect) 관련 효과 ---
    IncreaseStatusEffectDamage,     // BR6 (꺼지지 않는 화염: 화상 데미지 2배)
    IncreaseStatusEffectPotency,    // BR2(과냉각), BR4(부패한 세계수), BR8(예리한 검기): 상태이상 수치 증가
    SetStatusEffectPotency,         // BR7 (서리 내림: 둔화율 75% 고정)
    IncreaseStatusEffectDuration,   // 상태이상 지속시간 증가
    SetStatusEffectTickRate, // 안씀       // 상태이상 데미지 적용 주기 '고정값'으로 설정 (예: 0.5초)
    ModifyStatusEffectValue,        // BR9 (대규모 혼란: 기절 수치 감소)

    // --- 조건부 & 특수 효과 ---
    IncreaseDamagePerTowerCount,      // BE1 (잿빛 세계: 불 속성 타워 비례 데미지 증가)
    IncreaseDurationPerTowerCount,    // 특정 타워 개수당 지속시간 증가
    IncreaseAttackSpeedPerTowerCount, // BE3 (태풍의 분노: 대기 속성 타워 비례 공속 증가)
    IncreaseCritChancePerTowerCount,  // 특정 타워 개수당 치명타 확률 증가
    IncreaseStatusEffectPotencyPerTowerCount, // BE2 (바닥 없는 심해: 타워 개수당 상태이상 수치 증가)

    // --- 고유(Unique) 효과 ---
    // 여러 효과가 복잡하게 얽혀있어 하나의 enum으로 관리하는게 편한 경우
    ConvertExcessCritToDamage,    // BE4 (지구 던지기: 과잉 치명타 -> 데미지 전환)
    SynergyDamageBoost,           // BR5 (달빛검: 달빛/강철 타워 시너지)
    AutoPlaceTowerOnBossKill, // 안씀     // 보스 처치 시 특정 타워 자동 설치
}
