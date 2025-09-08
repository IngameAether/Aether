using UnityEngine;
using System.Collections.Generic;

// 마법책의 개별 효과를 정의하는 구조체
[System.Serializable]
public struct BookEffect
{
    public EBookEffectType EffectType;
    public EValueType ValueType;
    public int Value; // EffectValue 대신 단일 값으로 변경. 스택별 값은 MagicBookData의 리스트로 관리
    public ElementType TargetElement; // ElementType enum이 별도로 정의되어 있다고 가정
    public string TargetTowerCode;
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

    [Header("효과 목록")]
    // 마법책이 여러 효과를 가질 수 있도록 List<BookEffect>로 변경 
    public List<BookEffect> Effects;

    // 스택 레벨에 따른 효과 값 (예: 1레벨 +10, 2레벨 +20...)
    // Effects의 Value와 곱해져서 최종 효과를 결정할 수 있습니다.
    [Header("스택별 효과 값")]
    public List<int> EffectValuesByStack;
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
