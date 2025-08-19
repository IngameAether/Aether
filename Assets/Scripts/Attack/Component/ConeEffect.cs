using UnityEngine;

public class ConeEffect : MonoBehaviour
{
    [Header("Cone Settings")]
    public float range = 4f;          // 공격 거리
    public float angle = 15f;         // 부채꼴 반쪽 각도 (45면 총 90도 범위)
    public float damage = 5f;         // 데미지
    public float attackInterval = 0.5f; // 공격 속도

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= attackInterval)
        {
            _timer = 0f;
            FireConeAttack();
        }
    }

    void FireConeAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // 타워 → 적 방향 벡터
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                // forward 기준 각도
                float currentAngle = Vector3.Angle(transform.forward, dir);

                if (currentAngle < angle)
                {
                    Debug.Log($"Cone 범위 공격: {hit.name}");
                    // 실제로는 적 체력 감소 로직
                    // hit.GetComponent<Enemy>().TakeDamage(damage);
                }
            }
        }
    }
}
