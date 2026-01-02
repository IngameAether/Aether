using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float radius = 3f;
    public float damage = 10f;

    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // hit.GetComponent<Enemy>().TakeDamage(damage);
                Debug.Log("폭발 데미지 적용");
            }
        }
        Destroy(gameObject, 1f); // 이펙트 끝나면 삭제
    }
}
