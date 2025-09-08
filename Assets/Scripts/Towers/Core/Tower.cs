using System;
using Unity.VisualScripting;
using UnityEngine;

public enum ReinforceType { None, Light, Dark };

public abstract class Tower : MonoBehaviour
{
    [Header("Tower Configuration")]
    [SerializeField] protected TowerData towerData;

    public static event Action<Tower> OnTowerClicked;
    public static event Action OnTowerDestroyed;

    [Header("Tower Reinforce")]
    public ReinforceType reinforceType;

    public TowerData towerData;
    public string type { get; set; }

    protected SpriteRenderer spriteRenderer;
    protected SpriteRenderer magicCircleRenderer;

    protected float lastAttackTime;
    protected bool isFacingRight = true;
    protected Transform currentTarget; // 현재 타겟으로 삼고 있는 적의 위치
    protected Vector3 direction; // 적 방향

    [Header("Firing")]
    [SerializeField] protected Transform firePoint; 

    public Transform FirePoint => firePoint != null ? firePoint : transform;

    [Header("Projectile Settings")]
    [SerializeField] protected float projectileSpeed = 10f;
    [SerializeField] protected ProjectileMotionType defaultMotionType;
    [SerializeField] protected DamageEffectType defaultEffectType;
    [SerializeField] protected HitType defaultHitType;

    [Header("Projectile Prefabs by Rank")]
    [SerializeField] private GameObject basicProjectilePrefab;
    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private GameObject advancedProjectilePrefab;

    public TowerSetting GetTowerSetting()
    {
        return towerSetting;
    }

    public void SetTowerSetting(TowerData data)
    {
        towerData = data;
        towerSetting.Name = data.Name;
        towerSetting.Type = data.ElementType;
        towerSetting.Rank = data.Level;
        towerSetting.Damage = data.GetDamage(towerSetting.reinforceLevel);
        towerSetting.Range = data.BaseRange;
        towerSetting.AttackSpeed = data.GetAttackSpeed(towerSetting.reinforceLevel);
        towerSetting.CriticalHit = data.GetCriticalRate(towerSetting.reinforceLevel);
    }

    protected virtual void Start()
    {
        InitializeTower();
    }

    protected virtual void Update()
    {
        // 타겟 찾기 및 공격
        FindAndAttackTarget();
    }

    protected virtual void OnDestroy()
    {
        OnTowerDestroyed?.Invoke();
    }

    /// <summary>
    ///     타워 초기화 - 컴포넌트 설정 및 스프라이트 적용
    /// </summary>
    protected virtual void InitializeTower()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        magicCircleRenderer = GetComponentInChildren<SpriteRenderer>();
        EnsureFirePoint();
    }

    protected void EnsureFirePoint()
    {
        if (firePoint == null)
        {
            var found = transform.Find("FirePoint");
            if (found != null) firePoint = found;
            else firePoint = transform; // 최후 대체
        }
    }

    /// <summary>
    ///     타워 좌우반전 처리
    /// </summary>
    public void FlipTower()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = isFacingRight;
        if (magicCircleRenderer != null && magicCircleRenderer != spriteRenderer)
        {
            magicCircleRenderer.flipX = isFacingRight;
        }
    }

    #region Tower Find Target & Attack

    /// <summary>
    ///     타겟을 찾고 공격하는 함수
    /// </summary>
    protected virtual void FindAndAttackTarget()
    {
        // 타겟이 없거나, 죽었거나, 범위를 벗어났으면 새로운 타겟을 찾습니다.
        if (currentTarget == null || !isTargetAlive(currentTarget) || !IsTargetInRange(currentTarget))
        {
            currentTarget = FindNearestTarget();
        }

        // 유효한 타겟이 있고 공격 가능하면 공격합니다.
        if (currentTarget != null && CanAttack())
        {
            direction = (currentTarget.transform.position - transform.position).normalized;
            Attack();
            lastAttackTime = Time.time;
        }
    }

    /// <summary>
    ///     타겟이 사거리 안에 있는지 확인
    /// </summary>
    protected virtual bool IsTargetInRange(Transform target)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= towerSetting.Range;
    }

    /// <summary>
    ///     가장 가까운 적을 찾는 함수
    /// </summary>
    protected virtual Transform FindNearestTarget()
    {
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");

        var enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position,
            towerSetting.Range,
            enemyLayerMask
        );

        if (enemiesInRange.Length == 0) return null;

        Transform nearestTarget = null;
        var nearestDistance = float.MaxValue;

        foreach (var enemyCollider in enemiesInRange)
        {
            var distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = enemyCollider.transform;
            }
        }

        return nearestTarget;
    }

    // 타겟이 살아 있는지 확인
    protected virtual bool isTargetAlive(Transform target)
    {
        if (target == null) return false;
        var damageable = target.GetComponent<IDamageable>();
        return damageable != null && damageable.CurrentHealth > 0f;
    }

    /// <summary>
    ///     공격 가능한지 확인 (딜레이 체크)
    /// </summary>
    protected virtual bool CanAttack()
    {
        if (towerSetting.AttackSpeed <= 0f) return false;
        float attackInterval = 1f / towerSetting.AttackSpeed;
        return Time.time >= lastAttackTime + attackInterval;
    }

    /// <summary>
    ///     타워 공격
    /// </summary>
    protected virtual void Attack()
    {
        if (currentTarget == null) return;

        // 발사체 프리팹 선택
        GameObject projectilePrefab = null;

        switch (towerSetting.Rank)
        {
            case 1:
                projectilePrefab = basicProjectilePrefab; // 1단계용
                break;
            case 2:
                projectilePrefab = fireProjectilePrefab; // 2단계용 (불타워 효과)
                break;
            case 3:
                projectilePrefab = advancedProjectilePrefab; // 3단계용 (추가 가능)
                break;
            default:
                projectilePrefab = basicProjectilePrefab;
                break;
        }

        if (projectilePrefab == null) return;

        // 발사체 생성
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();

        if (projectile != null)
        {
            // 1. 적용할 상태 이상 객체 생성
            var effect = new StatusEffect(
                towerSetting.effectType,
                towerSetting.effectDuration,
                towerSetting.effectValue,
                transform.position
            );

            // 2. 발사체에 타겟, 데미지, 상태 이상 정보를 한 번에 전달
            projectile.Setup(currentTarget, towerSetting.Damage, effect);
        }
    }

    #endregion
    #region Action Handler

    /// <summary>
    ///     마우스 클릭 감지
    /// </summary>
    public void HandleTowerClicked()
    {
        OnTowerClicked?.Invoke(this);
    }

    #endregion
}
