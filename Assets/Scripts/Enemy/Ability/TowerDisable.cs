using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerDisable", menuName = "TowerDefense/Ability/TowerDisable")]
public class TowerDisable : SpecialAbility
{
    private float radius = 1.5f;
    private float interval = 4f;
    private float duration = 1f;

    public override void ApplySpecialAbility(NormalEnemy enemy)
    {
        enemy.StartCoroutine(AbilityDisable(enemy));
    }

    private IEnumerator AbilityDisable(NormalEnemy enemy)
    {
        while (enemy != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.transform.position, radius);
            foreach (Collider2D hit in hits)
            {
                Tower tower = hit.GetComponent<Tower>();
                if (tower != null)
                {
                    tower.DisableForSeconds(duration);
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
