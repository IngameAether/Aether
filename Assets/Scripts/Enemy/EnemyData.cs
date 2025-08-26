using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string ID { get; private set; }
    [field: SerializeField] public float HP { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float DamageReduction { get; private set; }
    [field: SerializeField] public float ControlResistance { get; private set; }
    [field: SerializeField] public int Aether {  get; private set; }
    [field: SerializeField] public List<SpecialAbility> abilities { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
}
