using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareAttack : FireObjectBase
{
    [Space]
    [Header("Prepare Variables")]
    [SerializeField] private AnimationClip prepareAnim;
    [SerializeField] private float prepareAnimSpeed;
    [SerializeField] private float prepareOffsetX;
    [SerializeField] private float prepareOffsetY;

    [Space]
    [Header("Attack Variables")]
    [SerializeField] private AnimationClip attackAnim;
    [SerializeField] private bool changePosToTower = false;
    [SerializeField] private float attackAnimSpeed;
    [SerializeField] private float attackOffsetX;
    [SerializeField] private float attackOffsetY;
    [SerializeField] private float attackOffsetWait;

    public override void Init(Vector2 tower, Transform target, float damage)
    {
        base.Init(tower, target, damage);

        // prepare 애니메이션 재생
        PlayPrepareAnim(prepareAnimSpeed, prepareOffsetX, prepareOffsetY);

        // prepare 에서 attack 애니메이션으로 전환
        StartCoroutine(TransPrepareToAttack(prepareAnim.length + 0.05f));
    }

    protected override void SetAnimationClip()
    {
        base.SetAnimationClip();
        
        animatorOverride["L2E_prepare"] = prepareAnim;
        animatorOverride["L2F_attack"] = attackAnim;
    }

    private IEnumerator TransPrepareToAttack(float waitAttack)
    {
        yield return new WaitForSeconds(waitAttack);

        if (!changePosToTower) this.transform.position = targetPos;
        PlayAttackAnim(attackAnimSpeed, attackOffsetX, attackOffsetY);

        yield return new WaitForSeconds(attackOffsetWait);
        OnTargetHit();
    }

    protected override void AfterTargetHit(float t)
    {
        base.AfterTargetHit(attackAnim.length);
    }
}
