using UnityEngine;
using System.Collections;

/// <summary>
/// 테스트용 타워
/// </summary>
public class ArrowTower : Tower
{
    public GameObject projectilePrefab;
    public Transform spawnPoint; // 인스펙터에 발사 포인트 연결
    private float projectileSpeed = 300f;

    protected override void Attack()
    {
        if (!currentTarget) return;

        Animator mouthAnimator = transform.Find("Mouth")?.GetComponent<Animator>();
        Animator scaleAnimator = GetComponent<Animator>();
        if (mouthAnimator != null && scaleAnimator != null)
        {
            mouthAnimator.SetTrigger("CanAttack");
            scaleAnimator.SetTrigger("CanAttack");
 //           Debug.Log("Animation 실행");
        }
        else
        {
            // 애니가 없으면 바로 발사
            SpawnProjectile();
        }
    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnProjectile();
    }

    public void ThrowProjectile()
    {
        if (!currentTarget) return;
        SpawnProjectile();
    }

    // 투사체 생성 및 발사
    private void SpawnProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("projectilePrefab이 할당되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var projectileScript = projectile.GetComponent<TowerAttack>();
        if (projectileScript != null)
        {
            // 방향은 타겟을 향하도록 계산
            Vector3 dir = (currentTarget.position - spawnPos).normalized;
            projectile.transform.right = dir; // 프리팹이 오른쪽을 앞(Forward)으로 본다고 가정
            // Initialize(데미지, 타겟, 방향, 속도) 시그니처가 다르면 맞춰서 호출하세요
            projectileScript.Initialize(towerSetting.Damage, currentTarget, dir, projectileSpeed);
        }
    }
}
