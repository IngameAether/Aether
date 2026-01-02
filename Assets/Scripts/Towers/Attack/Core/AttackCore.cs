// 공격이 날아가는 방식
public enum ProjectileMotionType
{
    None,       // 화염방사기 같은 근접
    Straight,   // 직선형
    Parabola    // 포물선형
}

// 데미지 모션 방식
public enum DamageEffectType
{
    SingleTarget,   // 단일 대상 (화살)
    Explosion,      // 원형 범위
    Cone,           // 부채꼴
    Zone            // 장판 (지속)
}
