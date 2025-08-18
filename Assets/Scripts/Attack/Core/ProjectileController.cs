using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ProjectileController : MonoBehaviour
{
    [Header("Runtime (auto)")]
    public ProjectileConfig config;

    [Header("Visual")]
    [Tooltip("포물선 시 시각적 높이 오프셋을 줄 spriteRenderer를 child로 연결하세요. 없다면 parent transform의 위치만 이동합니다.")]
    public SpriteRenderer spriteChild; // 인스펙터에 연결: 프리팹에서 'sprite'를 child로 두고 연결

    // 내부
    IProjectileMover mover;
    float lifeTimer;
    public Vector2 aimPoint; // 초기 발사 시 결정되는 목표 지점 (포물선용)
    public Transform target; // 유도용 타겟(있을 수도, 없을 수도)

    // 초기화 API
    public void Init(ProjectileConfig cfg, Transform _target, Vector2 _aimPoint)
    {
        config = cfg;
        target = _target;
        aimPoint = _aimPoint;
        lifeTimer = cfg.lifeTime;

        // mover 선택
        switch (cfg.movementType)
        {
            case MovementType.Parabolic:
                mover = new ParabolicMover(cfg);
                break;
            case MovementType.Homing:
                mover = new HomingMover(cfg);
                break;
            case MovementType.Straight:
            default:
                mover = new StraightMover(cfg);
                break;
        }

        mover.Init(this, aimPoint, target);
    }

    void Update()
    {
        if (config == null) return;
        float dt = Time.deltaTime;
        lifeTimer -= dt;
        if (lifeTimer <= 0f)
        {
            ReturnToPool();
            return;
        }
        if (mover != null) mover.Tick(dt);
    }

    // 투사체가 '임팩트'를 발생시키려 할 때 호출 (mover에서 호출)
    public void ApplyImpactAt(Vector2 pos)
    {
        if (config == null) return;

        // Direct: 단일 충돌은 mover가 직접 처리하는 경우가 많음 (mover에서 OverlapCircle로 검사)
        if (config.hitType == HitType.Direct)
        {
            Collider2D hit = Physics2D.OverlapCircle(pos, 0.25f, config.enemyLayer);
            if (hit != null)
            {
                var dmg = hit.GetComponent<IDamageable>();
                dmg?.TakeDamage(config.damage);
            }
        }
        else if (config.hitType == HitType.Explosive)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, config.radius, config.enemyLayer);
            foreach (var c in hits)
            {
                var dmg = c.GetComponent<IDamageable>();
                dmg?.TakeDamage(config.damage);
            }

            // 아래 zone 생성 활성화(폭발형 투사체의 추가적 능력)
            // -------------------------------------------
            // if (config.createsZoneOnExplode) {
            //     GroundAoE.Spawn(pos, config);
            // }
            // -------------------------------------------
        }
        else if (config.hitType == HitType.GroundAoE)
        {
            // 즉시 데미지 적용 후 장판을 만들 수 있음
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, config.radius, config.enemyLayer);
            foreach (var c in hits)
            {
                var dmg = c.GetComponent<IDamageable>();
                dmg?.TakeDamage(config.damage);
            }

            // 장판 생성
            // -------------------------------------------
            // GroundAoE.Spawn(pos, config);
            // -------------------------------------------
        }
    }

    public void ReturnToPool()
    {
        Destroy(gameObject);
    }

    // 디버그용 gizmo로 반경 표시
    void OnDrawGizmosSelected()
    {
        if (config == null) return;
        Gizmos.color = Color.red;
        if (config.hitType == HitType.Explosive || config.hitType == HitType.GroundAoE)
        {
            Gizmos.DrawWireSphere(transform.position, config.radius);
        }
    }
}
