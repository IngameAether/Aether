using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TowerSetting
{
    [Header("Tower Stats")]
    public string Name;
    public string Description;
    public ElementType Type;
    public int Rank;
    public int reinforceLevel;
    public float Damage;
    public float AttackSpeed;
    public float Range;
    public float CriticalHit;
}

public enum ReinforceType { None, Light, Dark };

public abstract class Tower : MonoBehaviour
{
    [Header("Tower Configuration")]
    [SerializeField] protected TowerSetting towerSetting;

    public static event Action<Tower> OnTowerClicked;

    [Header("Tower Reinforce")]
    public ReinforceType reinforceType;

    public string type { get; set; }

    protected SpriteRenderer spriteRenderer;
    protected SpriteRenderer magicCircleRenderer;

    protected float lastAttackTime;
    protected bool isFacingRight = true;
    protected Transform currentTarget; // 현재 타겟으로 삼고 있는 적의 위치
    protected Vector3 direction; // 적 방향

    private float _originalDamage;
    private float _originalAttackSpeed;

    public TowerSetting GetTowerSetting()
    {
        return towerSetting;
    }

    protected virtual void Start()
    {
        InitializeTower();
    }

    protected virtual void OnEnable()
    {
        BuffManager.Instance.OnAllTowerAttackSpeedChanged += HandleUpdateAttackSpeed;
        BuffManager.Instance.OnElementDamageChanged += HandleUpdateElementDamage;
    }

    protected virtual void OnDisable()
    {
        BuffManager.Instance.OnAllTowerAttackSpeedChanged -= HandleUpdateAttackSpeed;
        BuffManager.Instance.OnElementDamageChanged -= HandleUpdateElementDamage;
    }

    protected virtual void Update()
    {
        // 타겟 찾기 및 공격
        FindAndAttackTarget();
    }

    /// <summary>
    ///     타워 초기화 - 컴포넌트 설정 및 스프라이트 적용
    /// </summary>
    protected virtual void InitializeTower()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        magicCircleRenderer = GetComponentInChildren<SpriteRenderer>();

        _originalDamage = towerSetting.Damage;
        _originalAttackSpeed = towerSetting.AttackSpeed;

        var data = BuffManager.Instance.GetActiveBuffData(towerSetting.Type);
        HandleUpdateAttackSpeed(data.AttackSpeed);
        HandleUpdateElementDamage(towerSetting.Type, data.ElementDamage);
    }

    /// <summary>
    ///     타워 좌우반전 처리
    /// </summary>
    public void FlipTower()
    {
        isFacingRight = !isFacingRight;

        spriteRenderer.flipX = isFacingRight;
        magicCircleRenderer.flipX = isFacingRight;
    }

    #region Tower Find Target & Attack

    /// <summary>
    ///     타겟을 찾고 공격하는 함수
    /// </summary>
    protected virtual void FindAndAttackTarget()
    {
        if (!IsTargetInRange(currentTarget))
        {
            currentTarget = FindNearestTarget();
            return;
        }

        if (CanAttack())
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
        if (!target) return false;

        var distance = Vector2.Distance(transform.position, target.position);
        return distance <= towerSetting.Range; // 타겟과의 거리가 사거리보다 작거나 같은지 확인
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
            if (enemyCollider.gameObject == null) continue;

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
        if (!target) return false;

        var damageable = target.GetComponent<IDamageable>();
        if (damageable == null) return false;

        return damageable.CurrentHealth > 0f;
    }

    /// <summary>
    ///     공격 가능한지 확인 (딜레이 체크)
    /// </summary>
    protected virtual bool CanAttack()
    {
        if (towerSetting.AttackSpeed <= 0f)
        {
            Debug.Log("AttackSpeed가 0 이하입니다.");
            return false;
        }
        float attackInterval = 1f / towerSetting.AttackSpeed;
        var isDelayOver = Time.time >= lastAttackTime + attackInterval;
        var isAlive = isTargetAlive(currentTarget);
        return isDelayOver && isAlive;
    }

    /// <summary>
    ///     타워 공격
    /// </summary>
    protected abstract void Attack();

    #endregion

    #region Action Handler

    /// <summary>
    ///     마우스 클릭 감지
    /// </summary>
    public void HandleTowerClicked()
    {
        OnTowerClicked?.Invoke(this);
    }

    private void HandleUpdateAttackSpeed(float percentage)
    {
        towerSetting.AttackSpeed = _originalAttackSpeed * (1f + (percentage / 100f));
    }

    private void HandleUpdateElementDamage(ElementType element, float percentage)
    {
        if (towerSetting.Type != element) return;
        towerSetting.Damage = _originalDamage * (1f + (percentage / 100f));
    }

    #endregion
}
