using System;
using Unity.VisualScripting;
using UnityEngine;

public enum ReinforceType { None, Light, Dark };

public abstract class Tower : MonoBehaviour
{
    private TowerInformation towerInformation;

    [Header("Tower Configuration")]
    [SerializeField] public TowerData towerData; // protected를 public으로 변경했음. 도저히 오류를 잡을 수가 없었음.

    public static event Action<Tower> OnTowerClicked;
    public static event Action OnTowerDestroyed;

    [Header("Tower Reinforce")]
    public ReinforceType reinforceType;

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

    // 초기화 완료 상태를 저장할 변수
    private bool _isInitialized = false;

    [Header("Tower Data")]
    private int reinforceLevel = 0;
    public string TowerName => towerData.Name;
    public float Damage => towerData.GetDamage(reinforceLevel);
    public float AttackSpeed => towerData.GetAttackSpeed(reinforceLevel);
    public float Range => towerData.BaseRange;
    public int Rank => towerData.Level; // <-- 이 코드가 없으면 this.Rank 에서 CS1061 오류 발생
    public float CriticalHit => towerData.GetCriticalRate(reinforceLevel);
    public int CurrentReinforceLevel => reinforceLevel;
    public int MaxReinforce => towerData.MaxReinforce;

    protected virtual void Start()
    {
        InitializeTower();
    }

    protected virtual void Update()
    {
        if (!_isInitialized)
        {
            return;
        }

        // 타겟 찾기 및 공격 
        FindAndAttackTarget();
    }

    protected virtual void OnDestroy()
    {
        OnTowerDestroyed?.Invoke();
    }

    public void Setup(TowerData data)
    {
        if (data == null)
        {
            Debug.LogError($"{this.name}: 유효하지 않은 TowerData(null)로 Setup을 시도했습니다. 타워를 비활성화합니다.");
            _isInitialized = false;
            gameObject.SetActive(false); // 문제가 생긴 타워는 비활성화 처리
            return;
        }

        this.towerData = data;
        this.reinforceLevel = 0;
        _isInitialized = true;
    }

    public TowerData GetTowerData()
    {
        return towerData;
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
        return Vector2.Distance(transform.position, target.position) <= towerInformation.Range;
    }

    /// <summary>
    ///     가장 가까운 적을 찾는 함수
    /// </summary>
    protected virtual Transform FindNearestTarget()
    {
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");

        var enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position,
            towerInformation.Range,
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
        if (towerInformation.AttackSpeed <= 0f) return false;
        float attackInterval = 1f / towerInformation.AttackSpeed;
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

        switch (towerInformation.Rank)
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
                towerInformation.effectType,
                towerInformation.effectDuration,
                towerInformation.effectValue,
                transform.position
            );

            // 2. 발사체에 타겟, 데미지, 상태 이상 정보를 한 번에 전달
            projectile.Setup(currentTarget, this.Damage, effect, towerData.impactSound);
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
