using UnityEngine;

// 상태 이상 종류를 정의하는 열거형 (Enum)
public enum StatusEffectType
{
    None, Slow, Stun, Burn, Rot, Paralyze, Fear, Bleed
}

/// <summary>
/// 상태 이상 정보를 담는 순수 데이터 클래스입니다.
/// </summary>
public class StatusEffect
{
    public StatusEffectType Type { get; private set; }
    public float Duration { get; private set; } // 지속 시간 (x)
    public float Value { get; private set; }    // 효과 값 (y, 데미지 또는 퍼센트)
    public Vector3 SourcePosition { get; private set; } // 공포 효과를 위한 공격 타워 위치

    public float RemainingTime; // 남은 시간을 추적하기 위한 내부 변수

    public StatusEffect(StatusEffectType type, float duration, float value, Vector3 sourcePosition = default)
    {
        Type = type;
        Duration = duration;
        Value = value;
        SourcePosition = sourcePosition;
        RemainingTime = duration;
    }
}
