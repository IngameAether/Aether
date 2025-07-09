using UnityEngine;

namespace Towers.Component
{
    /// <summary>
    /// 테스트용 타워
    /// </summary>
    public class ArrowTower : Core.Tower
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

            Animator mouthAnimator = transform.Find("Mouth")?.GetComponent<Animator>();
            Animator scaleAnimator = GetComponent<Animator>();

            if (mouthAnimator != null)
            {
                Debug.Log("{mouth}: Animator 실행");
                mouthAnimator.SetTrigger("CanAttack");
            }

            if (scaleAnimator != null)
            {
                Debug.Log("{scale}: Animator 실행");
                scaleAnimator.SetTrigger("CanAttack");
            }
        }
    }
}