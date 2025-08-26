using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적의 체력, 능력치, 데미지 처리, 사망을 관리합니다.
/// </summary>
public class NormalEnemy : MonoBehaviour, IDamageable
{
    [Header("기본 정보")]
    public int idCode;
    public string GetEnemyId => idCode;

    [Header("능력치")]

    public float maxHealth = 10f;
    public float moveSpeed = 2f;
    [Range(0, 50)] public int magicResistance = 5;
    [Range(0, 100)] public int mentalStrength = 10;

    [Header("UI")]
    public Image healthBarFillImage;

    // 현재 체력 (외부에서는 읽기만 가능)
    public float CurrentHealth { get; private set; }

    // 컴포넌트 참조
    private EnemyMovement enemyMovement;
    private EnemyStatusManager statusManager;

    public EnemyData enemyData;
    
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
    }

    private void Start()
    {
        UpdateHealthBar();
        
        if (enemyData.abilities.Count > 0)
        {
            foreach (var ability in enemyData.abilities)
            {
                ability.ApplySpecialAbility(this);
            }
        }
    }

    public void SetEnemyData(EnemyData data)
    {
        enemyData = data;
    }

    /// <summary>
    /// 데미지를 받는 함수. 부패, 마법 저항력 순서로 최종 데미지를 계산합니다.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        float finalDamage = damageAmount;

        // 1. 부패(Rot) 효과 적용 (StatusManager의 데미지 배율 참조)
        if (statusManager != null)
        {
            finalDamage *= statusManager.DamageTakenMultiplier;
        }

        // 2. 마법 저항력 적용
        finalDamage = CalculateDamageAfterResistance(finalDamage);

        // 최종 데미지를 체력에서 차감
        CurrentHealth -= finalDamage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        UpdateHealthBar();

        // 체력이 0 이하면 사망 처리
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 마법 저항력을 기반으로 데미지를 감소시킵니다.
    /// </summary>
    private float CalculateDamageAfterResistance(float damageAmount)
    {
        float resistanceRatio = magicResistance / 100f;
        return damageAmount * (1 - resistanceRatio);
    }

    /// <summary>
    /// 정신력(강인함)을 기반으로 CC기(상태 이상)의 지속 시간을 감소시킵니다.
    /// </summary>
    public float CalculateReducedCCDuration(float baseDuration)
    {
        float tenacityRatio = mentalStrength / 100f;
        return baseDuration * (1 - tenacityRatio);
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
        Debug.Log(gameObject.name + "가 죽었습니다.");

        // 사망 시 모든 상태 이상 효과를 즉시 정리하여 오류를 방지합니다.
        statusManager?.ClearAllEffectsOnDeath();
        
        int baseReward = enemyData.Aether;
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
