using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectileBehavior
{
    void Initialize(TowerAttack projectile, float damage, Transform target, Vector3 direction, float speed);
    void OnHit(Collider2D collision);
}
