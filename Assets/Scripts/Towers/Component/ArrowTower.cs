using UnityEngine;

namespace Towers.Component
{
    /// <summary>
    /// 테스트용 타워
    /// </summary>
    public class ArrowTower : Core.Tower
    {
        public GameObject waterProjectilePrefab;
        private float projectileSpeed = 300f;

        protected override void Attack()
        {
            if (!currentTarget) return;

            SpawnProjectile();

            Animator mouthAnimator = transform.Find("Mouth")?.GetComponent<Animator>();
            Animator scaleAnimator = GetComponent<Animator>();
            if (mouthAnimator != null && scaleAnimator != null)
            {
                mouthAnimator.SetTrigger("CanAttack");
                scaleAnimator.SetTrigger("CanAttack");
                Debug.Log("Animation 실행");
            }
        }
        
        // 투사체 생성 및 발사
        private void SpawnProjectile()
        {
            GameObject waterProjectile = Instantiate(waterProjectilePrefab, transform.position, Quaternion.identity);
            var waterProjectileScript = waterProjectile.GetComponent<TowerAttack>();
            if (waterProjectileScript != null )
            {
                waterProjectileScript.Initialize(towerSetting.damage, currentTarget, direction, projectileSpeed);
            }
        }
    }
}