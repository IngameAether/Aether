using System;
using UnityEngine;

public enum EBuffType
{
    ElementDamage,          // 특정 속성 타워 데미지 증가
    AllTowerAttackSpeed,    // 모든 타워 공격속도 증가
    ElementSoulRate         // 원소의 혼의 수급량 증가
}

[Serializable]
public class BuffData
{
    public EBuffType BuffType;
    public float Value;
    public ElementType ElementType; // ElementDamage일때만 사용

    public string GetDescription()
    {
        return BuffType switch
        {
            EBuffType.ElementDamage => $"모든 {GetElementTypeName(ElementType)} 속성 타워의 데미지 {Value}% 증가",
            EBuffType.AllTowerAttackSpeed => $"모든 타워의 연사 속도 {Value}% 증가",
            EBuffType.ElementSoulRate => $"적 처치로부터 얻는 원소의 혼의 수급량 {Value}% 증가",
            _ => null
        };
    }

    private string GetElementTypeName(ElementType elementType)
    {
        return elementType switch
        {
            ElementType.Fire => "화염",
            ElementType.Water => "물",
            ElementType.Earth => "땅",
            ElementType.Air => "공기",
            _ => null
        };
    }
}

[Serializable]
public class BuffValueRange
{
    public EBuffType BuffType;
    public float MinValue;
    public float MaxValue;
    [Tooltip("ElementDamage 타입일 때만 사용")]
    public ElementType[] AvailableElements;
}
