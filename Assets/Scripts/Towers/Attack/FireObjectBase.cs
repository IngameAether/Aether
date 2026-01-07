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
    public float Damage { get { return damage; } }

    
    //[SerializeField]
    protected Animator animator;
    protected AnimatorOverrideController animatorOverride;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    public virtual void Init(Vector2 tower, Transform target, float damage)
    {
        this.towerPos = tower;
        transform.position = towerPos;

        this.target = target;
        this.targetPos = target.position;
        this.damage = damage;

        // 지정된 애니메이션으로 바꾸기
        SetAnimationClip();
    }

    public (StatusEffectType type, float value) GetStatusEffectInfo()
    {
        return (statusEffect, effctValue);
    }

    protected void PlayPrepareAnim(float speed = 1.0f, float offsetX = 0f, float offsetY = 0f)
    {
        Vector3 offset = new Vector3 (offsetX, offsetY);
        transform.position = transform.position + offset;
        animator.SetTrigger("prepare");
        animator.speed = speed;
    }
    protected void PlayFlyingAnim(float speed = 1.0f)
    {
        animator.SetTrigger("flying");
        animator.speed = speed;
    }
    protected void PlayAttackAnim(float speed = 1.0f, float offsetX = 0f, float offsetY = 0f)
    {
        Vector3 offset = new Vector3(offsetX, offsetY);
        transform.position = transform.position + offset;
        animator.SetTrigger("attack");
        animator.speed = speed;
    }
    protected void PlayAttackAnim(Vector3 newTargetPos, float speed = 1.0f)
    {
        transform.position = newTargetPos;
        animator.SetTrigger("attack");
        animator.speed = speed;
    }

    protected virtual void SetAnimationClip()
    {
        if (!(animator.runtimeAnimatorController is AnimatorOverrideController))
        {
            animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverride;
        }
        else
        {
            animatorOverride = (AnimatorOverrideController)animator.runtimeAnimatorController;
        }
    }

    protected void OnTargetHit()
    {
        if (isMultiHit)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
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
        }

        AfterTargetHit(0f);
    }

    // 공격이 완전히 끝난 뒤에 제거
    protected virtual void AfterTargetHit(float t)
    {
        Invoke("DestroySelf", t);
    }
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
