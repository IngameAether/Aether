using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageReduction", menuName = "TowerDefense/Ability/DamageReduction")]
public class DamageReduction : SpecialAbility
{
    public override void ApplySpecialAbility(NormalEnemy enemy)
    {
        enemy.finalDamageReduction = true;
    }
}
