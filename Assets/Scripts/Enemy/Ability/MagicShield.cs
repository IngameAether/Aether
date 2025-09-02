using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagicShield", menuName = "TowerDefense/Ability/MagicShield")]
public class MagicShield : SpecialAbility
{
    public float interval = 3f;
    public float percent = 0.1f;
    public override void ApplySpecialAbility(NormalEnemy enemy)
    {
        enemy.StartCoroutine(CreateShield(enemy));
    }

    private IEnumerator CreateShield(NormalEnemy enemy)
    {
        while (enemy != null)
        {
            float shieldAmlunt = enemy.maxHealth * percent;
            enemy.SetShield(shieldAmlunt);
            yield return new WaitForSeconds(interval);
        }
    }
}
