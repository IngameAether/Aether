using System.Collections.Generic;
using UnityEngine;

public class FireObjectBase : MonoBehaviour
{
    protected Vector2 towerPos;
    protected Vector2 targetPos;
    protected Transform target;

    //[Header("Components")]
    //[SerializeField]
    protected Animator animator;
    protected AnimatorOverrideController animatorOverride;

    private void Awake()
    {
        animator = this.GetComponent<Animator>();
    }

    public virtual void Init(Vector2 tower, Transform target)
    {
        animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);

        this.towerPos = tower;
        this.target = target;
        this.targetPos = target.position;
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
}
