using System;
using Unity.VisualScripting;
using UnityEngine;

public enum ReinforceType { None, Light, Dark };

public class Tower : MonoBehaviour
{
    // TowerExtension 참조 변수
    private TowerExtension _extension;
    private TowerInformation towerInformation;

    [Header("Tower Configuration")]
    [SerializeField] public TowerData towerData; // protected를 public으로 변경했음. 도저히 오류를 잡을 수가 없었음.
    public static event Action<Tower> OnTowerClicked;
    public static event Action OnTowerDestroyed;

    [Header("Tower Reinforce")]
    public ReinforceType reinforceType;

    public string type { get; set; }

    public TowerInfoData towerInfo;

    protected SpriteRenderer spriteRenderer;
    protected SpriteRenderer magicCircleRenderer;

    protected float lastAttackTime;
    protected bool isFacingRight = true;
    protected Transform currentTarget; // 현재 타겟으로 삼고 있는 적의 위치
    protected Vector3 direction; // 적 방향
    private bool isDisable = false;
    private float disableTimer = 0f;

    [Header("Firing")]
    [SerializeField] protected Transform firePoint;

    public Transform FirePoint => firePoint != null ? firePoint : transform;

    // 초기화 완료 상태를 저장할 변수
    private bool _isInitialized = false;

    [Header("Tower Data")]
    private int reinforceLevel = 0;  // 현재 타워의 강화 횟수를 저장하는 변수
    private int lightReinforceCount = 0;
    private int darkReinforceCount = 0;
    private float bonusRange = 0f;
    private float bonusBuildup = 0f;

    private TowerBuffData _appliedBuffs;

    // 외부 효과로 추가된 상태이상 지속시간을 저장할 변수
    private float bonusEffectDuration = 0f;
    // 최종 지속시간은 '기본 지속시간'과 '추가 지속시간'을 더한 값입니다.
    public float EffectDuration => towerData.effectDuration + bonusEffectDuration;
    public float EffectBuildup => towerData.effectBuildup + bonusBuildup;
    public string TowerName => towerData.Name;
    public float Damage => FormulaEvaluator.EvaluateTowerData(towerInfo.Attack, lightReinforceCount, darkReinforceCount);
    public float AttackSpeed => towerInfo.Speed;
    public float Range => towerInfo.Range;
    public int Rank => towerData.Level;
    public float CriticalHit => FormulaEvaluator.EvaluateTowerData(towerInfo.CriticalRate, lightReinforceCount, darkReinforceCount);
    public int CurrentReinforceLevel => reinforceLevel;
    public int MaxReinforce => towerInfo.MaxReinforcement;
    public int UnleashingPotential => towerInfo.UnleashingPotential;
    private TowerFinalStats _cachedStats;
    private bool _statsValid = false;

    protected virtual void Awake()
    {
        // 확장 스크립트를 찾아서 연결합니다.
        _extension = GetComponent<TowerExtension>();

        // 데이터 초기화는 Awake에서 먼저 실행되도록 유지
        Setup(this.towerData);
        MagicBookBuffSystem.OnBuffsUpdated += OnMagicBookBuffsUpdated;
    }

    protected virtual void Start()
    {
        Debug.Log($"[5] {gameObject.name}: Start() 함수 호출됨");
    }

    protected virtual void Update()
    {
        // 타워가 초기화되지 않았으면 아무 행동도 하지 않습니다.
        if (!_isInitialized)
        {
            return;
        }

        // 타워가 기능 정지 상태인 경우 타겟 찾는 로직 실행 X
        if (isDisable)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0) isDisable = false;
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
        Debug.Log($"[3] {gameObject.name}: Setup() 함수가 호출됨. 전달된 data is {(data == null ? "null" : data.name)}");

        if (data == null)
        {
            Debug.LogError($"{this.name}: 유효하지 않은 TowerData로 Setup을 시도했습니다.");
            _isInitialized = false;
            gameObject.SetActive(false);
            return;
        }

        towerInfo = TowerDatabase.GetTowerInfoData(data.ID);
        if (towerInfo == null)
        {
            Debug.Log($"{data.ID}에 해당하는 TowerInfoData가 없음");
        }
        else
        {
            // 이 로그가 뜬다면, Setup 함수는 정상적으로 데이터를 받았습니다.
            Debug.Log($"===> {this.name}: Setup 함수가 '{data.name}' 데이터로 성공적으로 초기화되었습니다.", this.gameObject);
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
    /// 타워 초기화 - 컴포넌트 설정 및 스프라이트 적용
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

        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
        if (magicCircleRenderer != null && magicCircleRenderer != spriteRenderer)
        {
            magicCircleRenderer.flipX = isFacingRight;
        }
    }

    /// <summary>
    /// B4 특수 능력: 사거리 내의 타워 1초간 기능 정지
    /// </summary>
    /// <param name="duration"></param>
    public void DisableForSeconds(float duration)
    {
        isDisable = true;
        disableTimer = duration;
        Debug.Log($"{gameObject.name}타워: {disableTimer} 동안 기능 일시 정지");
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
            if (direction.x > 0 && !isFacingRight)
            {
                FlipTower(); // 적이 오른쪽에 있는데 왼쪽을 보고 있으면 뒤집기
            }
            else if (direction.x < 0 && isFacingRight)
            {
                FlipTower(); // 적이 왼쪽에 있는데 오른쪽을 보고 있으면 뒤집기
            }
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
        return Vector2.Distance(transform.position, target.position)
            <= (_extension != null ? _extension.BuffedRange : this.Range);
    }

    /// <summary>
    ///     가장 가까운 적을 찾는 함수
    /// </summary>
    protected virtual Transform FindNearestTarget()
    {
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");
        float searchRange = (_extension != null ? _extension.BuffedRange : this.Range);

        var enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position,
            searchRange,
            enemyLayerMask
        );

        if (enemiesInRange.Length == 0)
        {
            return null; // 주변에 적이 없음
        }

        Transform nearestTarget = null;
        float closestDistance = float.MaxValue;

        // 감지된 모든 적을 순회
        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider.gameObject == this.gameObject) continue;

            float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
            if (distance < closestDistance)
            {
                NormalEnemy enemy = enemyCollider.GetComponent<NormalEnemy>();

                // 살아있는 적인지, 그리고 조준점이 있는지 확인
                if (enemy != null && enemy.CurrentHealth > 0)
                {
                    closestDistance = distance;
                    nearestTarget = enemy.AimPoint;
                }
            }
        }
        return nearestTarget;
    }

    // 타겟이 살아 있는지 확인
    protected virtual bool isTargetAlive(Transform target)
    {
        if (target == null) return false;

        // 먼저 target(AimPoint) 자체에서 IDamageable을 찾습니다.
        var damageable = target.GetComponent<IDamageable>();

        // 만약 찾지 못했다면(null이라면), 부모 오브젝트에서 다시 찾아봅니다.
        if (damageable == null)
        {
            damageable = target.GetComponentInParent<IDamageable>();
        }

        // 최종적으로 찾은 damageable이 유효하고, 체력이 0보다 큰지 확인합니다.
        return damageable != null && damageable.CurrentHealth > 0f;
    }

    /// <summary>
    ///     공격 가능한지 확인 (딜레이 체크)
    /// </summary>
    protected virtual bool CanAttack()
    {
        float currentAttackSpeed = (_extension != null) ? _extension.BuffedAttackSpeed : this.AttackSpeed;
        if (currentAttackSpeed <= 0f) return false;
        float attackInterval = 1f / currentAttackSpeed;
        return Time.time >= lastAttackTime + attackInterval;
    }

    /// <summary>
    ///     타워 공격
    /// </summary>
    protected virtual void Attack()
    {
        // TowerExtension 스크립트가 있다면 애니메이션을 통해 발사를 시도합니다.
        if (_extension != null)
        {
            _extension.TriggerAttackAnimation();
        }
        else // 없다면 직접 발사합니다.
        {
            FireProjectile();
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

    /// <summary>
    /// 실제 발사 로직. TowerExtension 또는 Attack()에서 호출됩니다.
    /// </summary>
    public void FireProjectile()
    {
        // 발사 직전 타겟 유효성 검사
        if (currentTarget == null || !isTargetAlive(currentTarget)) return;

        // 프리팹 확인
        if (towerData.projectilePrefab == null)
        {
            Debug.LogWarning($"{towerData.Name}: 발사체 프리팹이 지정되지 않았습니다.");
            return;
        }

        // 공격 사운드 재생
        if (towerData.attackSound != SfxType.None)
        {
            AudioManager.Instance.PlaySFX(towerData.attackSound);
        }

        // 발사체 생성 및 방향 설정 (Fire Point 사용)
        Vector3 spawnPos = firePoint.position;
        GameObject proj = Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);
        // 스프라이트가 위를 볼 경우
        Vector3 direction = (currentTarget.position - spawnPos).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
        // 발사체에 정보 주입 (Setup 호출)
        var projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 버프가 적용된 최종 능력치를 계산합니다.
            var buffedEffect = GetBuffedStatusEffect();
            float buffedDamage = GetBuffedDamage();
            float excessCritMultiplier = GetExcessCritDamageMultiplier();
            buffedDamage *= excessCritMultiplier;

            // 최종 계산된 능력치와 정보를 projectile에 한번만 전달합니다.
            projectile.Setup(currentTarget, buffedDamage, buffedEffect, this.EffectBuildup, towerData.impactSound, this.towerData);
        }
    }
    #endregion

    #region Tower Buff

    /// <summary>
    /// 마법도서 버프 적용
    /// </summary>
    public void ApplyMagicBookBuffs()
    {
        _statsValid = false; // 캐시 무효화
        Debug.Log($"[{towerData?.Name}] 버프 캐시 무효화");
    }

    /// <summary>
    /// 최종 스탯
    /// </summary>
    private void EnsureStatsValid()
    {
        if (!_statsValid && MagicBookBuffSystem.Instance != null)
        {
            _cachedStats = MagicBookBuffSystem.Instance.CalculateFinalStats(this);
            _statsValid = true;
        }
    }

    public float GetBuffedDamage()
    {
        EnsureStatsValid();
        return _cachedStats.Damage;
    }

    public float GetBuffedAttackSpeed()
    {
        EnsureStatsValid();
        return _cachedStats.AttackSpeed;
    }

    public float GetBuffedRange()
    {
        EnsureStatsValid();
        return _cachedStats.Range;
    }

    public float GetBuffedCritChance()
    {
        EnsureStatsValid();
        return _cachedStats.CritChance;
    }

    public float GetExcessCritDamageMultiplier()
    {
        EnsureStatsValid();
        return _cachedStats.ExcessCritDamageMultiplier;
    }

    public StatusEffect GetBuffedStatusEffect()
    {
        EnsureStatsValid();
        return _cachedStats.StatusEffect;
    }

    // 기존 보너스 값들 접근용 public 메서드 추가
    public float GetBonusRange() => bonusRange;
    public float GetBonusEffectDuration() => bonusEffectDuration;

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

        ApplyMagicBookBuffs(); // 캐시만 무효화
        Debug.Log($"[{towerData?.Name}] 새로운 마법도서 버프 준비 완료!");
    }

    #endregion
}
