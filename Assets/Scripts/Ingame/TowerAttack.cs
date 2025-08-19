using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    IDamageable damageable;
    private float damage;
    private Transform currentTarget;
    private IProjectileBehavior projectileBehavior;

    public void SetBehavior(IProjectileBehavior behavior)
    {
        projectileBehavior = behavior;
    }

    public void Initialize(float damage, Transform target, Vector3 direction, float speed)
    {
        projectileBehavior?.Initialize(this, damage, target, direction, speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        projectileBehavior?.OnHit(collision);
    }
}
