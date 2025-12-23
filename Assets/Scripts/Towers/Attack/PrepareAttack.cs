using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareAttack : FireObjectBase
{
    [Space]
    [Header("Prepare Variables")]
    [SerializeField] private AnimationClip prepareAnim;
    [SerializeField] private float prepareAnimSpeed;

    [Space]
    [Header("Attack Variables")]
    [SerializeField] private AnimationClip attackAnim;
    [SerializeField] private float attackAnimSpeed;
    public override void Init(Vector2 tower, Transform target, float damage)
    {
        base.Init(tower, target, damage);

        // prepare 애니메이션 재생
        PlayPrepareAnim(prepareAnimSpeed);

        // prepare 에서 attack 애니메이션으로 전환
        StartCoroutine(TransPrepareToAttack(prepareAnim.length + 0.05f));
    }

    protected override void SetAnimationClip()
    {
        base.SetAnimationClip();
        
        animatorOverride["L2E_prepare"] = prepareAnim;
        animatorOverride["L2F_attack"] = attackAnim;
    }

    private IEnumerator TransPrepareToAttack(float t)
    {
        yield return new WaitForSeconds(t);

        this.transform.position = targetPos;
        OnTargetHit();

        PlayAttackAnim(attackAnimSpeed);
    }

    protected override void AfterTargetHit(float t)
    {
        base.AfterTargetHit(attackAnim.length);
    }
}
