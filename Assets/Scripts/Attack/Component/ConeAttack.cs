using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeAttack : MonoBehaviour
{
    public float range = 2f;
    [Range(1, 360)] public float coneAngle = 60f; // 전체 각도
    public float damage = 15f;
    public LayerMask enemyLayer;

    // origin: 발사 위치, forward: 공격이 향하는 방향 (unit vector)
    public void Fire(Vector2 origin, Vector2 forward)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range, enemyLayer);
        foreach (var h in hits)
        {
            Vector2 dir = ((Vector2)h.transform.position - origin).normalized;
            float angle = Vector2.Angle(forward, dir);
            if (angle <= coneAngle * 0.5f)
            {
                var dmg = h.GetComponent<IDamageable>();
                dmg?.TakeDamage(damage);
            }
        }

        // TODO: 파티클/사운드 재생
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
