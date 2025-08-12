using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightMover : IProjectileMover
{
    ProjectileConfig cfg;
    ProjectileController owner;
    Vector2 direction;
    float speed;

    public StraightMover(ProjectileConfig config)
    {
        cfg = config;
        speed = cfg.speed;
    }

    public void Init(ProjectileController _owner, Vector2 aimPoint, Transform target = null)
    {
        owner = _owner;
        direction = (aimPoint - (Vector2)owner.transform.position).normalized;
        if (direction.magnitude < 0.001f) direction = Vector2.right;
    }

    public void Tick(float deltaTime)
    {
        owner.transform.position += (Vector3)(direction * speed * deltaTime);

        Collider2D hit = Physics2D.OverlapCircle(owner.transform.position, 0.2f, cfg.enemyLayer);
        if (hit != null)
        {
            owner.ApplyImpactAt(owner.transform.position);
            owner.ReturnToPool();
        }
    }
}
