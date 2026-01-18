using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    Slow, // 둔화
    Stun, // 기절
    Burn, // 화상
    Bleed, // 출혈
    None,
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

    // 공격 속도에 반영
    protected float aroundTime;
    public virtual float GetAroundTime()
    {
        aroundTime = 0;
        return aroundTime;
    }

    //[SerializeField]
    protected Animator animator;
    protected AnimatorOverrideController animatorOverride;

    // 상태이상 보너스 (타워에서 주입됨)
    protected float _bonusEffectValue = 0f;
    protected float _bonusEffectDuration = 0f;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    /// <summary>
    /// 외부(Tower)에서 계산된 상태이상 보너스 수치를 주입받는 메서드
    /// </summary>
    public void SetStatusEffectBonus(float valueBonus, float durationBonus)
    {
        _bonusEffectValue = valueBonus;
        _bonusEffectDuration = durationBonus;
    }

    public virtual void Init(Vector2 tower, Transform target, float damage)
    {
        this.towerPos = tower;
        transform.position = towerPos;

        this.target = target;
        this.targetPos = target.position;
        this.damage = damage;

        // 마법도서 상태이상 데미지 버프 (화상 등)
        if (MagicBookBuffSystem.Instance != null && statusEffect == StatusEffectType.Burn)
        {
            var modifier = MagicBookBuffSystem.Instance.GetStatusEffectModifier(StatusEffectType.Burn);
            if (modifier.DamageMultiplier > 1f)
            {
                this.damage *= modifier.DamageMultiplier;
            }
        }

        // 지정된 애니메이션으로 바꾸기
        SetAnimationClip();
    }

    public (StatusEffectType type, float value) GetStatusEffectInfo()
    {
        // 기본 수치 + 보너스 수치 반환
        return (statusEffect, effctValue + _bonusEffectValue);
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
        //if (!(animator.runtimeAnimatorController is AnimatorOverrideController))
        //{
        //    animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        //    animator.runtimeAnimatorController = animatorOverride;
        //}
        //else
        //{
        //    animatorOverride = (AnimatorOverrideController)animator.runtimeAnimatorController;
        //}

        // Lv.3 타워 공격 이펙트가 겹쳐 애니메이션 독립되게 생성되도록 수정
        AnimatorOverrideController baseController = animator.runtimeAnimatorController as AnimatorOverrideController;

        if (baseController != null)
        {
            animatorOverride = new AnimatorOverrideController(baseController);
        }
        else
        {
            animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        }

        animator.runtimeAnimatorController = animatorOverride;
    }

    protected void Rotate()
    {
        // 타겟 방향 투사체 회전
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
                        if (statusEffect != StatusEffectType.None) enemy.TakeHit(statusEffect, effctValue + _bonusEffectValue, damage);
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
        Invoke(nameof(DestroySelf), t);
    }
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
