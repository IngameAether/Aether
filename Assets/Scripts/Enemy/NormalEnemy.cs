using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 관련 기능을 위해 추가

public class NormalEnemy : MonoBehaviour, IDamageable
{
    // 적의 최대 체력
    public float maxHealth = 10f;
    // 현재 체력
    private float currentHealth;
    public float CurrentHealth => currentHealth;

    // 이동 속도
    public float moveSpeed = 2f;
    
    // 마법 저항력 (Magic Resistance)
    [Range(0, 50)] // Inspector 창에서 5~50 사이의 값만 입력 가능하도록 설정
    public int magicResistance = 5; // 기본 마법 저항력을 5%로 설정

    // 정신력 (Mental Strength, 강인함 효과)
    [Range(0, 100)] // Inspector 창에서 10~100 사이의 값만 입력 가능하도록 설정
    public int mentalStrength = 10; // 기본 정신력을 10%로 설정

    // 체력 바 이미지를 연결할 변수
    public Image healthBarFillImage; // 체력 바의 Fill 타입 이미지

    // EnemyMovement 컴포넌트 참조
    private EnemyMovement enemyMovement; 

    void Start()
    {
        // 게임 시작 시 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;
        // 체력 바 초기화
        UpdateHealthBar();

        // EnemyMovement 컴포넌트 참조
        enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            Debug.LogError("EnemyMovement 컴포넌트를 찾을 수 없습니다.");
        }
    }


    // 적이 피해를 입었을 때 호출될 함수
    public void TakeDamage(float damageAmount)
    {
        // 기본 데미지 계산
        float modifiedDamage = damageAmount;

        // 특수 능력 적용 (예: 마법 저항력)
        //modifiedDamage = CalculateDamageAfterResistance(modifiedDamage);

        // 최종 데미지 적용
        currentHealth -= modifiedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 체력 바 업데이트
        UpdateHealthBar();

        // 체력이 0 이하가 되면 적 제거 등 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 마법 저항력에 따른 데미지 계산 함수
    private float CalculateDamageAfterResistance(float damageAmount)
    {
        // 마법 저항력 %를 기준으로 데미지 감소율 계산
        float resistanceRatio = magicResistance / 100f;
        // 최종 데미지 계산 (데미지 * (1 - 저항력 비율))
        float finalDamage = damageAmount * (1 - resistanceRatio);
        return finalDamage;
    }

    // 정신력에 따른 CC 지속 시간 감소 계산 함수
    public float CalculateReducedCCDuration(float baseDuration)
    {
        // 정신력 %를 기준으로 지속 시간 감소율 계산
        float tenacityRatio = mentalStrength / 100f;
        // 최종 CC 지속 시간 계산 (기본 지속 시간 * (1 - 감소율))
        float finalDuration = baseDuration * (1 - tenacityRatio);
        return finalDuration;
    }

    // 체력 바 UI를 업데이트하는 함수
    void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            // 현재 체력 비율에 맞춰 체력 바 이미지의 Fill Amount를 조절
            healthBarFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    // 적이 죽었을 때 호출될 함수
    void Die()
    {
        Debug.Log(gameObject.name + "가 죽었습니다.");
        // TODO: 여기에 적이 죽었을 때 발생할 이벤트 (애니메이션 재생, 아이템 드랍, 오브젝트 제거 등)를 추가
        Destroy(gameObject); // 예시: 적 오브젝트 제거
    }

    // 이동 속도 변경 함수 (특수 능력 적용)
    public void ApplyMoveSpeedModifier(float modifier)
    {
        moveSpeed += modifier;
        //moveSpeed = Mathf.Clamp(moveSpeed, 0, maxMoveSpeed); // 최대 이동 속도 제한
    }
}