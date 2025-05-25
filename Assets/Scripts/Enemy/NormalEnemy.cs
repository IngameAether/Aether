using UnityEngine;
using UnityEngine.UI;

public class NormalEnemy : MonoBehaviour
{
    // 적의 최대 체력
    public float maxHealth = 100f;
    // 현재 체력
    private float currentHealth;

    // 이동 속도
    public float moveSpeed = 5f;
    // 방어력
    public float defense = 10f;

    // 체력 바 이미지를 연결할 변수
    public Image healthBarFillImage; // 체력 바의 Fill 타입 이미지

    void Start()
    {
        // 게임 시작 시 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;
        // 체력 바 초기화
        UpdateHealthBar();
    }

    void Update()
    {
        // 예시: 적이 앞으로 계속 이동한다고 가정
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // 움직임이 발생했음을 콘솔에 출력 (애니메이션 확인용 디버그)
        Debug.Log("적 이동 중...");

        // TODO: 여기에 적의 이동, 공격, 상태 변화 등 실제 게임 로직을 추가
        // 예시: 플레이어를 추적하는 로직 등
    }

    // 적이 피해를 입었을 때 호출될 함수
    public void TakeDamage(float damageAmount)
    {
        // 방어력을 적용한 실제 피해량 계산
        float actualDamage = Mathf.Max(0, damageAmount - defense);
        // 현재 체력에서 피해량만큼 감소
        currentHealth -= actualDamage;
        // 체력이 0보다 작아지지 않도록 clamped
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 체력 바 업데이트
        UpdateHealthBar();

        // 체력이 0 이하가 되면 적 제거 등 처리
        if (currentHealth <= 0)
        {
            Die();
        }
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
}