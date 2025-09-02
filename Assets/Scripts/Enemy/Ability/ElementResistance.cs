using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementResistance", menuName = "TowerDefense/Ability/ElementResistance")]
public class ElementResistance : SpecialAbility
{
    public ElementType major;
    public override void ApplySpecialAbility(NormalEnemy enemy)
    {
        //enemy.SetEnemyResistance(major, 10f);
    }
}
