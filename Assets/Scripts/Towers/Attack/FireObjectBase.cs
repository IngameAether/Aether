using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    None,
    Slow, // 둔화
    Stun, // 기절
    Burn, // 화상
    Bleed // 출혈
}

public class FireObjectBase : MonoBehaviour
{
    // Variables
    [Header("Variables")]
    [SerializeField] protected bool isMultiHit;
    [SerializeField] protected StatusEffectType statusEffect;
    [SerializeField] protected float effctValue;
    protected Vector2 towerPos;
    protected Vector2 targetPos;
    protected Transform target;
    protected float damage;

    
    //[SerializeField]
    protected Animator animator;
    protected AnimatorOverrideController animatorOverride;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    public virtual void Init(Vector2 tower, Transform target, float damage)
    {
        animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);

        this.towerPos = tower;
        transform.position = towerPos;

        this.target = target;
        this.targetPos = target.position;
        this.damage = damage;
    }

    public virtual void PlayPrepareAnim(float speed = 1.0f)
    {
        animator.SetTrigger("prepare");
        animator.speed = speed;
    }
    public virtual void PlayFlyingAnim(float speed = 1.0f)
    {
        animator.SetTrigger("flying");
        animator.speed = speed;
    }
    public virtual void PlayAttackAnim(float speed = 1.0f)
    {
        animator.SetTrigger("attack");
        animator.speed = speed;
    }

    protected void OnTargetHit()
    {
        if (target != null)
        {
            var enemy = target.GetComponent<NormalEnemy>();
            if (enemy == null)
            {
                enemy = target.parent.GetComponent<NormalEnemy>();
                if (enemy != null)
                {
                    if (statusEffect != StatusEffectType.None) enemy.TakeHit(statusEffect, effctValue, damage);
                    else enemy.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject);
    }
}
