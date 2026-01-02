using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBase : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private FireObjectBase fireObject;

    [Space]
    [Header("Variables")]
    private Vector2 towerPos;
    private Vector2 targetPos;
    // 나머지 변수들 불러오면 됨

    /// <summary>
    /// 공격을 단계별로 나누어 로직을 구분
    /// </summary>
    // 공격 전에 이루어져야 하는 로직
    public virtual void BeforeAttack() { return; }

    // 공격을 준비하면서 이루어져야 하는 로직
    public virtual void Prepare() { return; }

    // 공격이 투사체라면 날아가면서 이루어져야 하는 로직
    public virtual void Flying() { return; }

    // 공격이 실제로 이루어지는 동안의 로직
    public virtual void Attack() { return; }

    // 추가적인 공격이 이루어지는 로직
    public virtual void AdditionalAttack() { return; }

    // 공격이 끝나고 이루어져야 하는 로직
    public virtual void AfterAttack() { return; }
}
