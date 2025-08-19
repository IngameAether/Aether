using UnityEngine;
using System.Collections;

/// <summary>
/// 테스트용 타워
/// </summary>
public class ArrowTower : Tower
{
    [Header("Projectile Prefabs")]
    [Tooltip("기본 직선탄 (모든 Rank1 공통, Air/Water Rank2도 사용)")]
    [SerializeField] private GameObject projectileStraightPrefab;

    [Tooltip("불 타워 Rank2 전용 (화염방사) - Projectile.motionType=None, effectType=FireCone 세팅")]
    [SerializeField] private GameObject projectileFireConePrefab;

    [Tooltip("땅 타워 Rank2 전용 (포물선 + 폭발) - Projectile.motionType=Parabola, hitType=GroundAoE 세팅")]
    [SerializeField] private GameObject projectileEarthArcPrefab;

    [Header("Animation Option")]
    [SerializeField] private bool useAnimation = false;

    protected override void InitializeTower()
    {
        base.InitializeTower();

        // firePoint 자동 탐색 (없으면 타워 기준 발사)
        if (firePoint == null)
        {
            var fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
        }
    }

    protected override void Attack()
    {
        if (!currentTarget) return;

        if (useAnimation)
        {
            // 애니메이션 트리거 → AnimationEvent에서 ThrowProjectile 호출
            Animator mouthAnimator = transform.Find("Mouth")?.GetComponent<Animator>();
            Animator scaleAnimator = GetComponent<Animator>();
            if (mouthAnimator != null) mouthAnimator.SetTrigger("CanAttack");
            if (scaleAnimator != null) scaleAnimator.SetTrigger("CanAttack");
        }
        else
        {
            // 애니메이션 안 쓰면 바로 발사
            FireProjectile();
        }
    }

    public void ThrowProjectile()
    {
        if (!currentTarget) return;
        FireProjectile();
    }

    private void FireProjectile()
    {
        // Rank/속성에 맞는 ProjectilePrefab 선택
        GameObject prefabToFire = SelectProjectilePrefab();

        if (prefabToFire == null)
        {
            Debug.LogWarning($"{name}: 발사체 프리팹이 지정되지 않았습니다. (Rank:{towerSetting.Rank}, Type:{towerSetting.Type})");
            return;
        }

        // 위치/방향 계산
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 dir = (currentTarget.position - spawnPos).normalized;

        // 생성
        GameObject proj = Instantiate(prefabToFire, spawnPos, Quaternion.identity);

        // 방향 회전
        if (dir.sqrMagnitude > 0.0001f)
            proj.transform.right = dir;

        // Projectile 초기화
        var projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(currentTarget);
        }
        else
        {
            Debug.LogWarning($"{name}: 생성된 프리팹에 Projectile 컴포넌트가 없습니다.");
        }
    }

    private GameObject SelectProjectilePrefab()
    {
        // Rank 1 → 무조건 직선 탄
        if (towerSetting.Rank <= 1)
            return projectileStraightPrefab;

        // Rank 2 이상: 원소별 분기
        switch (towerSetting.Type)
        {
            case ElementType.Fire:
                return projectileFireConePrefab;
            case ElementType.Earth:
                return projectileEarthArcPrefab;
            case ElementType.Air:
            case ElementType.Water:
            default:
                return projectileStraightPrefab;
        }
    }
}
