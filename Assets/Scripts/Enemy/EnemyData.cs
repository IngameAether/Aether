using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상태이상 종류와 최대 게이지 값을 한 쌍으로 묶는 클래스
[System.Serializable]
public class StatusGauge
{
    public StatusEffectType type;   // 상태이상 종류
    public float maxValue = 100f;   // 해당 상태이상의 최대 게이지 값
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public ElementType Major { get; private set; }
    [field: SerializeField] public float HP { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float DamageReduction { get; private set; }
    [field: SerializeField] public float ControlResistance { get; private set; }
    [field: SerializeField] public int Aether {  get; private set; }
    [field: SerializeField] public List<SpecialAbility> abilities { get; private set; }
    [field: SerializeField] public string Description { get; private set; }

    [Header("상태이상 게이지 정보")]
    public List<StatusGauge> statusGauges;

    public bool HasAbility<T>() where T : SpecialAbility
    {
        foreach (var ability in abilities)
        {
            if (ability is T)
                return true;
        }
        return false;
    }
}
