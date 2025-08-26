using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialAbility : ScriptableObject
{
    /// <summary>
    /// 특수 능력 추상화
    /// </summary>
    /// <param name="normalEnemy"></param>
    public abstract void ApplySpecialAbility(NormalEnemy normalEnemy);
}
