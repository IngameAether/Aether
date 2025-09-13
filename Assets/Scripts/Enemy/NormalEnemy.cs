using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적의 체력, 능력치, 데미지 처리, 사망을 관리합니다.
/// </summary>
public class NormalEnemy : MonoBehaviour, IDamageable
{
    [Header("기본 정보")]
    public string idCode;
    public string GetEnemyId => idCode;

    [Header("능력치")]
    public float maxHealth = 10f;
    public float moveSpeed = 2f;
    [Range(0, 50)] private int magicResistance = 5;
    [Range(0, 100)] private int mentalStrength = 10;

    [Header("UI")]
    public Image healthBarFillImage;

    // 현재 체력 (외부에서는 읽기만 가능)
    public float CurrentHealth { get; private set; }
    private float currentShield;    // 보호막 값

    // 컴포넌트 참조
    private EnemyMovement enemyMovement;
    private EnemyStatusManager statusManager;

    public EnemyData enemyData;
    public EnemyInfoData enemyInfo;
    public int curEnemyIndex;
    public bool finalDamageReduction = false;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        // 필수 컴포넌트들을 미리 찾아와서 저장합니다.
        enemyMovement = GetComponent<EnemyMovement>();
        statusManager = GetComponent<EnemyStatusManager>();

        if (enemyMovement == null || statusManager == null)
        {
            Debug.LogError($"{gameObject.name}에서 필수 컴포넌트(EnemyMovement 또는 EnemyStatusManager)를 찾을 수 없습니다.");
        }

        enemyInfo = EnemyDatabase.GetEnemyInfoData(idCode);
        if (enemyInfo != null)
        {
            moveSpeed = enemyInfo.Speed;
        }
    }

    private void Start()
    {
        currentShield = 0;
        UpdateHealthBar();
    }

    public void SetEnemyData(EnemyData data, int enemyIndex)
    {
        enemyData = data;
        curEnemyIndex = enemyIndex;

        // 특수 능력 적용
        if (enemyData.abilities.Count > 0)
        {
            foreach (var ability in enemyData.abilities)
            {
                ability.ApplySpecialAbility(this);
            }
        }
    }

    /// <summary>
    /// 특수 능력: 전공 속성과 일치하는 속성의 공격 대미지 감소 적용
    /// </summary>
    /// <param name="towerElementType"></param>
    public void SetResistance(ElementType towerElementType)
    {
        if (enemyData.Major == towerElementType)
        {
            CalculateDamageAfterResistance(10f);
        }
    }

    /// <summary>
    /// 특수 능력: 3초마다 보호막 생성
    /// </summary>
    /// <param name="amount"></param>
    public void SetShield(float amount)
    {
        currentShield = amount;   // 갱신형
        Debug.Log($"{gameObject.name} 보호막 갱신: {currentShield}");
    }

    /// <summary>
    /// 데미지를 받는 함수. 부패, 마법 저항력 순서로 최종 데미지를 계산합니다.
    /// </summary>
    public void TakeDamage(float damageAmount) 
    {
        float finalDamage = damageAmount;

        // 1. 상태 이상 효과 적용 (부패 등)
        finalDamage = ApplyStatusEffects(finalDamage);

        // 2. 마법 저항력 적용
        finalDamage = CalculateDamageAfterResistance(finalDamage);

        // 3. 특수 능력 적용
        finalDamage = ApplySpecialAbilities(finalDamage);

        if (finalDamage > 0)
        {
            CurrentHealth -= finalDamage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            UpdateHealthBar();
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // 상태 이상으로 인한 데미지 증폭/감소 효과를 적용합니다.
    private float ApplyStatusEffects(float damage)
    {
        if (statusManager != null)
        {
            // 부패(Rot) 효과 등으로 변경된 데미지 배율을 가져와 적용합니다.
            damage *= statusManager.DamageTakenMultiplier;
        }
        return damage;
    }

    /// <summary>
    /// 마법 저항력을 기반으로 데미지를 감소시킵니다.
    /// </summary>
    public float CalculateDamageAfterResistance(float damageAmount)
    {
        // 저항력이 10이면 10% 데미지 감소 -> 원래 데미지의 90%만 받음
        float damageMultiplier = 1.0f - (magicResistance / 100f);
        return damageAmount * damageMultiplier;
    }

    /// <summary>
    /// 정신력(강인함)을 기반으로 CC기(상태 이상)의 지속 시간을 감소시킵니다.
    /// </summary>
    public float CalculateReducedCCDuration(float baseDuration)
    {
        // 정신력이 10이면 10% 시간 감소 -> 원래 시간의 90%만 적용
        float durationMultiplier = 1.0f - (mentalStrength / 100f);
        return baseDuration * durationMultiplier;
    }

    // =======================================================================
    // 공격을 받는 새로운 진입점. Projectile이 이 함수를 호출해야 합니다.
    // =======================================================================
    public void TakeHit(float damageAmount, StatusEffect effect, float effectChance)
    {
        // 적용할 상태이상이 있는지 확인
        if (effect != null && effect.Type != StatusEffectType.None)
        {
            // 제어저항(정신력)에 따른 최종 확률 계산
            float resistanceFactor = 1.0f - (mentalStrength / 100f); // 정신력 10 = 10% 저항
            float finalChance = effectChance * resistanceFactor;

            // 확률 체크
            if (Random.Range(0f, 1f) <= finalChance)
            {
                // 확률 성공! 상태이상 적용 로직 실행
                statusManager.TryApplyStatusEffect(effect);
            }
        }
        // 데미지 처리 로직 실행
        TakeDamage(damageAmount);
    }

    /// <summary>
    /// 특수 능력 처리
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    private float ApplySpecialAbilities(float damage)
    {
        // B1: 특수 능력
        if (finalDamageReduction && SpawnManager._aliveS3Enemies > 0)
            damage *= 0.5f;

        // B2: 보호막 계산
        if (currentShield > 0)
        {
            float shield = Mathf.Min(currentShield, damage);
            currentShield -= shield;
            damage -= shield;
        }
        return damage;
    }

    private void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            healthBarFillImage.fillAmount = CurrentHealth / maxHealth;
        }
    }

    /// <summary>
    /// 사망 처리 함수. 상태 이상 효과를 먼저 정리하고 오브젝트를 파괴합니다.
    /// </summary>
    private void Die()
    {
        if (GetEnemyId == "S3") SpawnManager._aliveS3Enemies--;

        Debug.Log(gameObject.name + "가 죽었습니다.");

        // 사망 시 모든 상태 이상 효과를 즉시 정리하여 오류를 방지합니다.
        statusManager?.ClearAllEffectsOnDeath();
        
        int baseReward = enemyInfo.Aether;
        // 코인 보상
        int bonusReward = ResourceManager.Instance.EnemyKillBonusCoin;
        int totalReward = baseReward + bonusReward;
        ResourceManager.Instance.AddCoin(totalReward);

        // 코인 보상 등 게임 로직 처리
        // ResourceManager.Instance.AddCoin(ResourceManager.Instance.EnemyKillBonusCoin);

        // EnemyMovement를 통해 오브젝트 파괴 및 이벤트 전파
        if (enemyMovement != null)
        {
            enemyMovement.Die();
        }
        else
        {
            // EnemyMovement가 없는 비상 상황을 대비해 직접 파괴
            Destroy(gameObject);
        }
    }
}
