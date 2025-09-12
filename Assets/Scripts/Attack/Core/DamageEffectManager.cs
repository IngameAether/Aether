using UnityEngine;
using System.Collections.Generic;

public class DamageEffectManager : MonoBehaviour
{
    public static DamageEffectManager Instance { get; private set; }

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer; // "Enemy" 레이어로 세팅 권장

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (enemyLayer.value == 0) enemyLayer = LayerMask.GetMask("Enemy");
    }

    // Projectile이 모든 걸 넘겨줍니다.
    public void ApplyEffect(
        DamageEffectType type,
        Vector3 pos,
        float damage,
        Transform target = null,
        float radius = 2f,
        float coneAngle = 60f,
        Vector3 coneForward = default,
        ElementType elementType = ElementType.None
    )

    {
        // target이 유효한지
        if (target != null)
        {
            // 유효한 타겟이 있을 때만 저항력 관련 로직을 실행합니다.
            if (target.TryGetComponent<NormalEnemy>(out var enemy))
            {
                enemy.SetResistance(elementType);
            }
        }

        switch (type)
        {
            case DamageEffectType.SingleTarget:
                ApplySingleTarget(target, pos, damage);
                break;

            case DamageEffectType.Explosion:
                ApplyExplosion(pos, radius, damage);
                break;

            case DamageEffectType.Cone:
                if (coneForward == default) coneForward = Vector3.right;
                ApplyCone(pos, radius, coneAngle, coneForward, damage);
                break;

            case DamageEffectType.Zone:
                Debug.Log("장판 설치 (원한다면 여기서 프리팹 생성)");
                break;
        }
    }

    void ApplySingleTarget(Transform target, Vector3 pos, float damage)
    {
        if (target != null && target.TryGetComponent<IDamageable>(out var d))
        {
            d.TakeDamage(damage);
            return;
        }

        // 타겟이 없으면 근처에서 한 명 찾아서 때림(백업)
        var c2D = Physics2D.OverlapCircleAll(pos, 0.5f, enemyLayer);
        if (c2D.Length > 0 && c2D[0].TryGetComponent<IDamageable>(out var d2))
        {
            d2.TakeDamage(damage);
            return;
        }

        var c3D = Physics.OverlapSphere(pos, 0.5f, enemyLayer);
        if (c3D.Length > 0 && c3D[0].TryGetComponent<IDamageable>(out var d3))
        {
            d3.TakeDamage(damage);
        }
    }

    void ApplyExplosion(Vector3 pos, float radius, float damage)
    {
        // 2D
        foreach (var col in Physics2D.OverlapCircleAll(pos, radius, enemyLayer))
            if (col.TryGetComponent<IDamageable>(out var d)) d.TakeDamage(damage);

        // 3D
        foreach (var col in Physics.OverlapSphere(pos, radius, enemyLayer))
            if (col.TryGetComponent<IDamageable>(out var d)) d.TakeDamage(damage);
    }

    void ApplyCone(Vector3 pos, float radius, float angleDeg, Vector3 forward, float damage)
    {
        // 2D
        foreach (var col in Physics2D.OverlapCircleAll(pos, radius, enemyLayer))
        {
            Vector3 dir = (col.transform.position - pos).normalized;
            if (Vector3.Angle(forward, dir) <= angleDeg * 0.5f &&
                col.TryGetComponent<IDamageable>(out var d)) d.TakeDamage(damage);
        }
        // 3D
        foreach (var col in Physics.OverlapSphere(pos, radius, enemyLayer))
        {
            Vector3 dir = (col.transform.position - pos).normalized;
            if (Vector3.Angle(forward, dir) <= angleDeg * 0.5f &&
                col.TryGetComponent<IDamageable>(out var d)) d.TakeDamage(damage);
        }
    }
}
