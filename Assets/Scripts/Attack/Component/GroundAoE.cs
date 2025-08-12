using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAoE : MonoBehaviour
{
    public float radius = 1.5f;
    public float duration = 3f;
    public float tickInterval = 0.5f;
    public float tickDamage = 5f;
    public LayerMask enemyLayer;

    public static void Spawn(Vector2 pos, ProjectileConfig cfg)
    {
        // 간단 구현: Instantiate a GameObject with this component
        // 실제로는 풀을 사용하는 것을 권장
        GameObject go = new GameObject("GroundAoE");
        go.transform.position = pos;
        var aoe = go.AddComponent<GroundAoE>();
        aoe.radius = cfg.radius;
        aoe.duration = cfg.zoneDuration;
        aoe.tickInterval = cfg.zoneTickInterval;
        aoe.tickDamage = cfg.zoneTickDamage;
        aoe.enemyLayer = cfg.enemyLayer;
        aoe.StartCoroutine(aoe.Run());
    }

    IEnumerator Run()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
            foreach (var h in hits)
            {
                var dmg = h.GetComponent<IDamageable>();
                dmg?.TakeDamage(tickDamage);
            }
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
