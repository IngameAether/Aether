using UnityEngine;

/// <summary>
/// 테스트용 타워
/// </summary>
public class ArrowTower : Tower
{
    IDamageable damageable;
    protected override void Attack()
    {
        if (!currentTarget) return;

        damageable = currentTarget.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log($"{gameObject.name}이 {currentTarget.name}에게 {towerSetting.damage}의 피해를 입혔습니다");
            damageable.TakeDamage(towerSetting.damage);
        }
    }
}