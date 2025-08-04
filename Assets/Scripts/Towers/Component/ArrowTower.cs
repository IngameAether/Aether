using UnityEngine;

/// <summary>
/// 테스트용 타워
/// </summary>
public class ArrowTower : Tower
{
    public GameObject projectilePrefab;
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
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<TowerAttack>();
        if (projectileScript != null )
        {
            projectileScript.Initialize(towerSetting.damage, currentTarget, direction, projectileSpeed);
        }
    }
}
