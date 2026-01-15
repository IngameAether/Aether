using System.Collections.Generic;
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

    [Space]
    [Header("능력치")]
    public float maxHealth = 10f;
    public float moveSpeed = 2f;
    public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }
    [Range(0, 50)] private int magicResistance = 5;

    [Space]
    [Header("UI")]
    public Image healthBarFillImage;

    [Header("Targeting Settings")]
    [Tooltip("타워가 조준할 몸 중앙 지점입니다. (AimPoint 자식 오브젝트)")]
    [SerializeField] private Transform aimPoint;
    public Transform AimPoint
    {
        get
        {
            // 안전장치
            if (aimPoint == null)
            {
                return this.transform;
            }
            return aimPoint;
        }
    }

    // 현재 체력 (외부에서는 읽기만 가능)
    public float CurrentHealth { get; private set; }
    private float currentShield;    // 보호막 값

    // 컴포넌트 참조
    private EnemyMovement enemyMovement;
    private EnemyHitFlicking enemyHit;
    private EnemyStatusEffect enemyStatusEffect;

    public EnemyData enemyData;
    public EnemyInfoData enemyInfo;
    public int curEnemyIndex;
    public bool finalDamageReduction = false;

    private void Awake()
    {
        // 필수 컴포넌트들을 미리 찾아와서 저장합니다.
        enemyMovement = GetComponent<EnemyMovement>();
        enemyHit = GetComponent<EnemyHitFlicking>();
        enemyStatusEffect = GetComponent<EnemyStatusEffect>();

        if (enemyMovement == null)
        {
            Debug.LogError($"{gameObject.name}에서 필수 컴포넌트(EnemyMovement 또는 EnemyStatusManager)를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        currentShield = 0;
        UpdateHealthBar();
    }

    /// <summary>
    /// Enemy Data 세팅
    /// </summary>
    /// <param name="info"></param>
    /// <param name="currentWave"></param>
    public void Initialize(EnemyInfoData info, int currentWave)
    {
        enemyInfo = info;
        if (enemyInfo != null)
        {
            moveSpeed = enemyInfo.Speed;

            // hp 계산
            maxHealth = FormulaEvaluator.EvaluateToFloat(enemyInfo.Hp, currentWave);
            CurrentHealth = maxHealth;

            // 저항력, 정신력 계산
            magicResistance = FormulaEvaluator.EvaluateToInt(enemyInfo.DamageReduction, currentWave);

            // 상태이상 관련 초기화
            enemyStatusEffect.Initialize(enemyInfo);
        }
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
    /// 마법 저항력 -> 특수 능력 -> 최종 적용 순서로 최종 데미지를 계산합니다.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        Debug.Log(CurrentHealth);

        float finalDamage = damageAmount;

        // 1. 마법 저항력 적용
        finalDamage = CalculateDamageAfterResistance(finalDamage);

        // 2. 특수 능력 적용
        finalDamage = ApplySpecialAbilities(finalDamage);

        if (finalDamage > 0)
        {
            CurrentHealth -= finalDamage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            UpdateHealthBar();

            enemyHit.HitFlicking();
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
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

    // 공격을 받는 새로운 진입점. Projectile이 이 함수를 호출해야 합니다.
    public void TakeHit(StatusEffectType statusEffect, float effectValue, float damageAmount)
    {
        // 상태이상 효과 적용
        enemyStatusEffect.TakeStatusEffect(statusEffect, effectValue);

        // 데미지 처리 로직은 그대로 실행
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
        // 비트마스킹을 통해 정리할거임 ㄱㄷ

        int bonus = ResourceManager.Instance.IsBossRewardDouble ? 2 : 1;

        int baseReward = enemyInfo.Aether;
        // 코인 보상
        int bonusReward = ResourceManager.Instance.EnemyKillBonusCoin;
        int totalReward = baseReward + bonusReward;
        ResourceManager.Instance.AddCoin(totalReward * bonus);

        // 빛/어둠 재화 보상
        int element = enemyInfo.Element;
        ResourceManager.Instance.GetElement(element * bonus);

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
