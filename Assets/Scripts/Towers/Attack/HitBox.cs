using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public LayerMask targetLayer;
    private ContactFilter2D filter;
    private Collider2D myCollider;
    private List<Collider2D> resultColliders = new List<Collider2D>();

    void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        filter.SetLayerMask(targetLayer); // 적 레이어만 필터링
    }

    void OnEnable()
    {
        ApplyAttack();

        gameObject.SetActive(false);
    }

    private void ApplyAttack()
    {
        int count = myCollider.OverlapCollider(filter, resultColliders);

        (StatusEffectType type, float value) statusEffect = transform.parent.GetComponent<FireObjectBase>().GetStatusEffectInfo();
        float damage = transform.parent.GetComponent<FireObjectBase>().Damage;

        if (statusEffect.type != StatusEffectType.None)
        {
            for (int i = 0; i < count; i++)
            {
                if (resultColliders[i] != null)
                    resultColliders[i].GetComponent<NormalEnemy>().TakeHit(statusEffect.type, statusEffect.value, damage);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (resultColliders[i] != null)
                    resultColliders[i].GetComponent<NormalEnemy>().TakeDamage(damage);
            }
        }
    }
}
