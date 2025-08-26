using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CCImmunity", menuName = "TowerDefense/Ability/CCImmunity")]
public class CCImmunity : SpecialAbility
{
    public override void ApplySpecialAbility(NormalEnemy normalEnemy)
    {
        // 제어 효과 영향 받지 않음
    }
}
