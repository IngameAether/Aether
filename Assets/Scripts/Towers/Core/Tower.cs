using System;
using Unity.VisualScripting;
using UnityEngine;

public enum ReinforceType { None, Light, Dark };

public class Tower : MonoBehaviour
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

    // 초기화 완료 상태를 저장할 변수
    private bool _isInitialized = false;

    [Header("Tower Data")]
    private int reinforceLevel = 0;
    // 현재 타워의 강화 횟수를 저장하는 변수
    private int lightReinforceCount = 0;
    private int darkReinforceCount = 0;
    private float bonusRange = 0f;
    private float bonusBuildup = 0f;

    private TowerBuffData appliedBuffs;

    // 외부 효과로 추가된 상태이상 지속시간을 저장할 변수
    private float bonusEffectDuration = 0f;
    // 최종 지속시간은 '기본 지속시간'과 '추가 지속시간'을 더한 값입니다.
    public float EffectDuration => towerData.effectDuration + bonusEffectDuration;
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
        Setup(this.towerData);
        MagicBookBuffSystem.OnBuffsUpdated += OnMagicBookBuffsUpdated;
    }

    protected virtual void Update()
    {
        // 타워가 초기화되지 않았으면 아무 행동도 하지 않습니다.
        if (!_isInitialized)
        {
            return;
        }

        // 매 프레임마다 타겟을 찾고 공격을 시도합니다.
        FindAndAttackTarget();
    }

    public void Upgrade()
    {
        // 현재 강화 단계가 최대치 미만인지 확인 (예: 0, 1 < 2)
        if (reinforceLevel < towerData.MaxReinforce - 1)
        {
            // 외형이 바뀌지 않는 일반 강화 (1->2, 2->3)
            reinforceLevel++; // 강화 레벨만 1 올림
            Debug.Log($"{towerData.Name} {reinforceLevel + 1}단계로 강화 완료!");
        }
        else
        {
            // 최종 단계로 업그레이드 (3->4)
            TowerData nextData = towerData.nextUpgradeData;
            if (nextData != null && nextData.upgradedPrefab != null)
            {
                // 새 타워 생성 및 기존 타워 파괴 로직
                GameObject newTowerObject = Instantiate(nextData.upgradedPrefab, transform.position, transform.rotation);
                newTowerObject.GetComponent<Tower>()?.Setup(nextData);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("최대 레벨입니다.");
            }
        }
    }

    protected virtual void OnDestroy()
    {
        OnTowerDestroyed?.Invoke();
        MagicBookBuffSystem.OnBuffsUpdated -= OnMagicBookBuffsUpdated;
    }

    public void Setup(TowerData data)
    {
        if (data == null)
        {
            Debug.LogError($"{this.name}: 유효하지 않은 TowerData로 Setup을 시도했습니다.");
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }

        this.towerData = data;
        this.reinforceLevel = 0;

        InitializeTower();

        _isInitialized = true;

    }

    // Light 또는 Dark 재화로 타워를 강화하는 함수. UI 버튼 등에서 이 함수를 호출합니다.
    public void Reinforce(ReinforceType type)
    {
        // 강화 타입에 따라 횟수 증가
        if (type == ReinforceType.Light)
        {
            lightReinforceCount++;
            Debug.Log($"{towerData.Name} Light 강화! ({lightReinforceCount}/{towerData.reinforcementThreshold})");

            // 진화 조건 확인
            if (towerData.lightEvolutionData != null && lightReinforceCount >= towerData.reinforcementThreshold)
            {
                Evolve(towerData.lightEvolutionData);
            }
        }
        else if (type == ReinforceType.Dark)
        {
            darkReinforceCount++;
            Debug.Log($"{towerData.Name} Dark 강화! ({darkReinforceCount}/{towerData.reinforcementThreshold})");

            // 진화 조건 확인
            if (towerData.darkEvolutionData != null && darkReinforceCount >= towerData.reinforcementThreshold)
            {
                Evolve(towerData.darkEvolutionData);
            }
        }
    }

    /// 특정 데이터로 타워를 진화시키는 내부 함수
    private void Evolve(TowerData evolutionData)
    {
        Debug.Log($"{towerData.Name}이(가) {evolutionData.Name}(으)로 진화합니다!");

        // Upgrade() 함수의 프리팹 교체 로직과 동일
        GameObject prefabToSpawn = evolutionData.upgradedPrefab != null ? evolutionData.upgradedPrefab : this.gameObject;

        // 새 타워 생성
        GameObject newTowerObject = Instantiate(prefabToSpawn, transform.position, transform.rotation);
        newTowerObject.GetComponent<Tower>()?.Setup(evolutionData);

        // 기존 타워 파괴
        Destroy(gameObject);
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
        return Vector2.Distance(transform.position, target.position) <= GetBuffedRange();
    }

    /// <summary>
    ///     가장 가까운 적을 찾는 함수
    /// </summary>
    protected virtual Transform FindNearestTarget()
    {
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");

        var enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position,
            GetBuffedRange(),
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
        float buffedAttackSpeed = GetBuffedAttackSpeed();
        if (buffedAttackSpeed <= 0f) return false;

        float attackInterval = 1f / buffedAttackSpeed;
        return Time.time >= lastAttackTime + attackInterval;
    }

    /// <summary>
    ///     타워 공격
    /// </summary>
    protected virtual void Attack()
    {
        if (currentTarget == null) return;

        if (towerData.attackSound != SfxType.None)
        {
            // 이 부분은 현재 프로젝트의 '오디오 매니저' 이름에 맞게 수정해야 합니다.
            AudioManager.Instance.PlaySFX(towerData.attackSound);
        }

        // TowerData에 발사체 프리팹이 있는지 확인
        if (towerData.projectilePrefab == null)
        {
            Debug.LogWarning($"{towerData.Name}: 발사체 프리팹이 지정되지 않았습니다.");
            return;
        }

        // 발사체 생성
        Vector3 spawnPos = FirePoint.position;
        GameObject proj = Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);

        // 타워에서 타겟으로 향하는 방향 벡터를 계산합니다.
        Vector3 direction = (currentTarget.position - spawnPos).normalized;

        // 발사체의 오른쪽(x축)이 그 방향을 바라보도록 회전시킵니다.
        proj.transform.right = direction;

        // 발사체 초기화 (Projectile.cs의 Setup 호출)
        var projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            var buffedEffect = GetBuffedStatusEffect();
            float buffedDamage = GetBuffedDamage();

            // 초과 치명타 데미지 적용
            float excessCritMultiplier = GetExcessCritDamageMultiplier();
            buffedDamage *= excessCritMultiplier;

            projectile.Setup(currentTarget, buffedDamage, buffedEffect, towerData.effectBuildup, towerData.impactSound);
        }
    }

    public void AddBonusRange(float amount)
    {
        bonusRange += amount;
    }

    // 외부에서 타워의 상태이상 누적치를 강화하는 함수
    public void AddBonusBuildup(float amount)
    {
        bonusBuildup += amount;
        Debug.Log($"{TowerName}의 상태이상 누적치가 {amount}만큼 증가!");
    }

    // 외부에서 타워의 상태이상 지속시간을 늘려주는 함수
    public void AddBonusEffectDuration(float amount)
    {
        bonusEffectDuration += amount;
        Debug.Log($"{TowerName}의 상태이상 지속시간이 {amount}초 증가!");
    }
    #endregion

    #region Tower Buff

    public void ApplyMagicBookBuffs()
    {
        if (MagicBookBuffSystem.Instance == null) return;

        string towerCode = towerData?.Name ?? "DefaultTower";
        ElementType element = towerData!.ElementType;

        appliedBuffs = MagicBookBuffSystem.Instance.GetTowerBuffs(towerCode, element);
        Debug.Log($"[{towerCode}] 마법도서 버프 적용 완료");
    }

    /// <summary>
    ///     마법도서 버프가 적용된 최종 데미지 계산
    /// </summary>
    public float GetBuffedDamage()
    {
        var baseDamage = Damage;
        var finalDamage = baseDamage * appliedBuffs.FinalDamageMultiplier;

        // 타워 개수 기반 동적 버프 추가
        var dynamicDamageBonus = MagicBookBuffSystem.Instance?.GetTowerCountBasedBuff(
            towerData.Name, EBookEffectType.IncreaseDamagePerTowerCount) ?? 0f;

        if (dynamicDamageBonus > 0) finalDamage *= 1f + dynamicDamageBonus / 100f;

        return finalDamage;
    }

    /// <summary>
    ///     마법도서 버프가 적용된 최종 공격속도 계산
    /// </summary>
    public float GetBuffedAttackSpeed()
    {
        var baseAttackSpeed = AttackSpeed;
        var finalAttackSpeed = baseAttackSpeed * appliedBuffs.AttackSpeedMultiplier;

        // 타워 개수 기반 동적 버프 추가
        var dynamicSpeedBonus = MagicBookBuffSystem.Instance?.GetTowerCountBasedBuff(
            towerData.Name, EBookEffectType.IncreaseAttackSpeedPerTowerCount) ?? 0f;

        if (dynamicSpeedBonus > 0) finalAttackSpeed *= 1f + dynamicSpeedBonus / 100f;

        return finalAttackSpeed;
    }

    /// <summary>
    ///     마법도서 버프가 적용된 최종 사거리 계산
    /// </summary>
    public float GetBuffedRange()
    {
        if (appliedBuffs.OverrideRange > 0) return appliedBuffs.OverrideRange;

        var baseRange = Range + bonusRange; // 기존 보너스 사거리도 포함
        return baseRange * appliedBuffs.RangeMultiplier;
    }

    /// <summary>
    ///     마법도서 버프가 적용된 최종 치명타 확률 계산
    /// </summary>
    public float GetBuffedCritChance()
    {
        var baseCrit = CriticalHit;
        var finalCrit = baseCrit + appliedBuffs.CritChanceBonus;

        // 타워 개수 기반 동적 버프 추가
        var dynamicCritBonus = MagicBookBuffSystem.Instance?.GetTowerCountBasedBuff(
            towerData.Name, EBookEffectType.IncreaseCritChancePerTowerCount) ?? 0f;

        finalCrit += dynamicCritBonus;

        // 초과 치명타를 데미지로 변환하는 특수 효과 처리
        if (appliedBuffs.HasUniqueEffects.Contains(EBookEffectType.ConvertExcessCritToDamage))
            return Mathf.Min(finalCrit, 100f); // 100% 초과분은 데미지로 변환됨

        return finalCrit;
    }

    /// <summary>
    ///     초과 치명타가 데미지로 변환되는 추가 데미지 배수 계산
    /// </summary>
    public float GetExcessCritDamageMultiplier()
    {
        if (!appliedBuffs.HasUniqueEffects.Contains(EBookEffectType.ConvertExcessCritToDamage))
            return 1f;

        var totalCrit = CriticalHit + appliedBuffs.CritChanceBonus;

        // 타워 개수 기반 치명타도 포함
        var dynamicCritBonus = MagicBookBuffSystem.Instance?.GetTowerCountBasedBuff(
            towerData.Name, EBookEffectType.IncreaseCritChancePerTowerCount) ?? 0f;
        totalCrit += dynamicCritBonus;

        if (totalCrit > 100f)
        {
            var excessCrit = totalCrit - 100f;
            return 1f + excessCrit / 100f; // 초과분을 데미지 배수로 변환
        }

        return 1f;
    }

    /// <summary>
    ///     상태이상 효과에 마법도서 버프 적용
    /// </summary>
    public StatusEffect GetBuffedStatusEffect()
    {
        // 기본 지속시간 계산 (기존 보너스 포함)
        float baseDuration = towerData.effectDuration + bonusEffectDuration;
        float baseValue = towerData.effectValue;

        // 상태이상 관련 버프가 있는지 확인
        if (appliedBuffs.StatusEffectModifiers != null &&
            appliedBuffs.StatusEffectModifiers.TryGetValue(towerData.effectType, out var modifier))
        {
            // Value는 상태이상 타입에 따라 데미지 또는 수치를 의미
            // - Burn/Rot/Bleed: 데미지 값
            // - Slow/Stun/Paralyze: 효과 수치 (둔화율, 기절 수치 등)
            float modifiedValue = baseValue;
            float modifiedDuration = baseDuration + modifier.DurationBonus;

            switch (towerData.effectType)
            {
                case StatusEffectType.Burn:
                case StatusEffectType.Rot:
                case StatusEffectType.Bleed:
                    modifiedValue *= modifier.DamageMultiplier;
                    break;

                case StatusEffectType.Slow:
                case StatusEffectType.Stun:
                case StatusEffectType.Paralyze:
                case StatusEffectType.Fear:
                    modifiedValue *= modifier.PotencyMultiplier;
                    break;
            }

            return new StatusEffect(
                towerData.effectType,
                modifiedDuration,
                modifiedValue,
                transform.position
            );
        }

        // 버프가 없으면 기본 효과 반환
        return new StatusEffect(
            towerData.effectType,
            baseDuration,
            baseValue,
            transform.position
        );
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

    /// <summary>
    /// 새로운 마법도서가 획득되었을 때 호출
    /// </summary>
    private void OnMagicBookBuffsUpdated()
    {
        if (!_isInitialized) return;

        ApplyMagicBookBuffs();

        Debug.Log($"[{towerData?.Name}] 새로운 마법도서 버프 적용 완료!");
    }

    #endregion
}
