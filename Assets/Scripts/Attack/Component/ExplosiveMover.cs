using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveMover : IProjectileMover
{
    ProjectileConfig cfg;
    ProjectileController owner;
    Vector2 startPos;
    Vector2 aimPoint;
    Vector2 direction;
    float speed;
    float arriveThreshold = 0.12f; // 도달 판정 반경 (필요시 조정)
    bool useFalloff;
    float minDamageFraction = 0.25f; // falloff시 최소 데미지 비율(중심에서 가장자리까지)

    public ExplosiveMover(ProjectileConfig config)
    {
        cfg = config;
        speed = cfg.speed;
        useFalloff = false; // 기본은 false. 필요시 Init에서 true로 바꿔주세요.
    }

    // Init에서 추가 파라미터를 전달하고 싶다면 오버로드하거나 cfg에 필드 추가하세요.
    public void Init(ProjectileController _owner, Vector2 _aimPoint, Transform target = null)
    {
        owner = _owner;
        startPos = owner.transform.position;
        aimPoint = _aimPoint;
        direction = (aimPoint - startPos).normalized;
        if (direction.magnitude < 0.001f) direction = Vector2.right;
        // 예: cfg에 falloff 플래그가 있으면 사용하도록 연결 가능
        // useFalloff = cfg.useDamageFalloff; // (cfg에 해당 필드가 있으면)
    }

    public void Tick(float deltaTime)
    {
        // 이동
        Vector2 pos = owner.transform.position;
        Vector2 next = pos + direction * speed * deltaTime;
        owner.transform.position = new Vector3(next.x, next.y, owner.transform.position.z);

        // 도달/충돌 판정: 목표 지점 근처 또는 장애물 충돌 등으로 확장 가능
        if (Vector2.Distance(next, aimPoint) <= arriveThreshold)
        {
            ExplodeAt(next);
            owner.ReturnToPool();
            return;
        }

        // 간단 근접 충돌감지(적에게 직접 닿으면 즉시 폭발)
        Collider2D hit = Physics2D.OverlapCircle(next, 0.12f, cfg.enemyLayer);
        if (hit != null)
        {
            ExplodeAt(next);
            owner.ReturnToPool();
        }
    }

    void ExplodeAt(Vector2 pos)
    {
        // 즉시 데미지 적용: OverlapCircleAll
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, cfg.radius, cfg.enemyLayer);
        foreach (var c in hits)
        {
            var damageable = c.GetComponent<IDamageable>();
            if (damageable == null) continue;

            float applied = cfg.damage;
            if (useFalloff)
            {
                float dist = Vector2.Distance(pos, c.transform.position);
                float t = Mathf.Clamp01(dist / Mathf.Max(cfg.radius, 0.0001f));
                // 선형 감쇠: center -> full, edge -> minDamageFraction
                applied = Mathf.Lerp(cfg.damage, cfg.damage * minDamageFraction, t);
            }
            damageable.TakeDamage(applied);
        }

        // 시각/사운드 이펙트 재생 위치: pos
        // 예: FXManager.Play("Explosion", pos);

        // 잔류영역(zone) 생성: 팀장 확인용으로 기본 주석 처리.
        // ----------------------------------------------
        // if (cfg.createsZoneOnExplode) {
        //     GroundAoE.Spawn(pos, cfg); // 주석 해제 시 활성화
        // }
        // ----------------------------------------------
    }
}
