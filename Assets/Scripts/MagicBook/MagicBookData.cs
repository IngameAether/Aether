using UnityEngine;

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
}

public enum EBookRank
{
    Normal,
    Rare,
    Epic,
    Special
}
