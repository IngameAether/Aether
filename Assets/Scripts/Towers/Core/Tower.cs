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

    protected virtual void Awake()
    {
        // 확장 스크립트를 찾아서 연결합니다.
        _extension = GetComponent<TowerExtension>();

        // 데이터 초기화는 Awake에서 먼저 실행되도록 유지
        Setup(this.towerData);
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
        return Vector2.Distance(transform.position, target.position)
            <= (_extension != null ? _extension.BuffedRange : this.Range);
    }

    /// <summary>
    ///     가장 가까운 적을 찾는 함수
    /// </summary>
    protected virtual Transform FindNearestTarget()
    {
        LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");

        var enemiesInRange = Physics2D.OverlapCircleAll(
            transform.position,
            (_extension != null ? _extension.BuffedRange : this.Range),
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

        return null;
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
        // 공격의 실제 실행을 TowerExtension에 위임
        if (_extension != null)
        { 
            // 확장 스크립트가 있다면, 애니메이션 재생을 요청
            _extension.TriggerAttackAnimation();
        }
        else
        {
            // 확장 스크립트가 없다면, 직접 발사 (예비용)
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
        if (currentTarget == null) return;
        if (towerData.projectilePrefab == null) return;
        if (towerData.attackSound != SfxType.None) AudioManager.Instance.PlaySFX(towerData.attackSound);

        Vector3 spawnPos = FirePoint.position;
        GameObject proj = Instantiate(towerData.projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.right = (currentTarget.position - spawnPos).normalized;

        var projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // [수정] 모든 능력치를 _extension에서 가져오도록 변경
            var effect = new StatusEffect(
                towerData.effectType,
                _extension != null ? _extension.BuffedEffectDuration : this.EffectDuration,
                towerData.effectValue, // TODO: EffectValue 버프 추가 필요
                transform.position
            );

            projectile.Setup(
                currentTarget,
                _extension != null ? _extension.BuffedDamage : this.Damage,
                effect,
                _extension != null ? _extension.BuffedEffectBuildup : towerData.effectBuildup,
                towerData.impactSound,
                this.towerData
            );
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
