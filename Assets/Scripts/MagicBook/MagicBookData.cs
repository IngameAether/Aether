using UnityEngine;
using System.Collections.Generic;

// 마법책의 개별 효과를 정의하는 구조체
[System.Serializable]
public struct BookEffect
{
    public EBookEffectType EffectType;
    public EValueType ValueType;
    public float Value; // EffectValue 대신 단일 값으로 변경. 스택별 값은 MagicBookData의 리스트로 관리
    public ElementType TargetElement; // ElementType enum이 별도로 정의되어 있다고 가정
    public string TargetTowerCode;

    [Tooltip("IncreaseStatus... 타입의 효과가 어떤 상태이상에 적용될지 지정합니다.")]
    public StatusEffectType TargetStatusEffect;
}

// 마법책 데이터 ScriptableObject
[CreateAssetMenu(fileName = "NewMagicBook", menuName = "TowerDefense/MagicBookData")]
public class MagicBookData : ScriptableObject
{
    [Header("기본 정보")]
    public string Name;
    public string Code;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("등급 및 스택")]
    public EBookRank Rank;
    public int MaxStack;

    [Header("획득 조건")]
    public AcquisitionCondition condition;

    [Header("효과 목록")]
    // 마법책이 여러 효과를 가질 수 있도록 List<BookEffect>로 변경 
    public List<BookEffect> Effects;

    // 스택 레벨에 따른 효과 값 (예: 1레벨 +10, 2레벨 +20...)
    // Effects의 Value와 곱해져서 최종 효과를 결정할 수 있습니다.
    [Header("스택별 효과 값")]
    public List<int> EffectValuesByStack;

    public string GetFormattedDescription(int forStackLevel)
    {
        // 효과(Effects) 목록이 비어있으면, 기본 Description을 그대로 반환
        if (Effects == null || Effects.Count == 0)
        {
            return Description;
        }

        // 스택이 0이하로 들어오는 예외 처리 (선택창에서는 1 이상이어야 함)
        if (forStackLevel <= 0) forStackLevel = 1;

        // 요청된 스택 레벨에 맞는 효과 값을 가져옴
        // (리스트 범위를 초과하지 않도록 Clamp로 안전장치)
        int stackIndex = Mathf.Clamp(forStackLevel - 1, 0, EffectValuesByStack.Count - 1);
        int stackValue = EffectValuesByStack[stackIndex];

        // {0}을 채울 최종 값 계산 (첫 번째 효과를 기준으로 함)
        var firstEffect = Effects[0];
        float finalEffectValue = firstEffect.Value * stackValue;

        // 값 타입에 따라 %를 붙이거나 안 붙임
        string valueText = (firstEffect.ValueType == EValueType.Percentage)
            ? $"+{finalEffectValue}%"
            : $"+{finalEffectValue}";

        // string.Format을 사용하여 Description의 "{0}" 부분을 실제 값으로 교체하여 반환
        return string.Format(Description, valueText);
    }
}

// 마법책 등급
public enum EBookRank
{
    Normal,
    Rare,
    Epic,
    Special
}

// 값의 종류 (고정값, 비율)
public enum EValueType
{
    Flat,
    Percentage
}

