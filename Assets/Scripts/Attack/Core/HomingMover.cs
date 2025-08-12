using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMover : IProjectileMover
{
    ProjectileConfig cfg;
    ProjectileController owner;
    Transform target;
    Vector2 aimPoint;
    float speed;
    float turnSpeed; // deg/sec

    public HomingMover(ProjectileConfig config)
    {
        cfg = config;
        speed = cfg.speed;
        turnSpeed = cfg.turnSpeed;
    }

    public void Init(ProjectileController _owner, Vector2 _aimPoint, Transform _target = null)
    {
        owner = _owner;
        target = _target;
        aimPoint = _aimPoint;
    }

    public void Tick(float deltaTime)
    {
        Vector2 pos = owner.transform.position;
        Vector2 desiredDir;
        if (target != null) desiredDir = ((Vector2)target.position - pos).normalized;
        else desiredDir = (aimPoint - pos).normalized;

        // current forward using rotation (assuming right is forward; 조정 가능)
        float currentAngle = owner.transform.eulerAngles.z;
        Vector2 forward = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        float targetAngle = Mathf.Atan2(desiredDir.y, desiredDir.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * deltaTime);
        owner.transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

        Vector2 move = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * speed * deltaTime;
        owner.transform.position += (Vector3)move;

        // 간단 충돌 감지: 근접한 적이 있으면 즉시 임팩트
        Collider2D hit = Physics2D.OverlapCircle(owner.transform.position, 0.25f, cfg.enemyLayer);
        if (hit != null)
        {
            // 직접 데미지 또는 폭발 처리
            owner.ApplyImpactAt(owner.transform.position);
            owner.ReturnToPool();
        }
    }
}
