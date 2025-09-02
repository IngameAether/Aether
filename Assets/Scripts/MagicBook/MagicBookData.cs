using UnityEngine;

[System.Serializable] // 이 어트리뷰트가 있어야 인스펙터에 보입니다.
public struct BookEffect
{
    public EBookEffectType EffectType; // 어떤 종류의 효과인지
    public EValueType ValueType;      // 고정값인지, 퍼센트인지
    public int Value;                // 효과 수치
}

[CreateAssetMenu(fileName = "MagicBook", menuName = "TowerDefense/MagicBook")]
public class MagicBookData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Code { get; private set; }
    [field: SerializeField] public int MaxStack { get; private set; }
    [field: SerializeField] public EBookRank Rank { get; private set; }
    [field: SerializeField] public EBookEffectType EffectType { get; private set; }
    [field: SerializeField] public int[] EffectValue { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public EValueType ValueType { get; private set; }
}

public enum EBookRank
{
    Normal,
    Rare,
    Epic,
    Special
}

public enum EValueType
{
    Flat,       // 고정 수치 (예: +5 골드)
    Percentage  // 비율 (예: +5%)
}
