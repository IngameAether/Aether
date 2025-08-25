using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType { Direct, Explosive, GroundAoE }

public class Projectile : MonoBehaviour
{
    [Header("Motion")]
    public ProjectileMotionType motionType = ProjectileMotionType.Straight;
    public float speed = 10f;
    public float arcHeight = 2f;
    public float arriveDistance = 0.3f;

    [Header("Damage / AoE")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float coneAngle = 60f;

    [SerializeField] private HitType hitType = HitType.Direct;
    [SerializeField] private DamageEffectType effectType = DamageEffectType.SingleTarget;

    // --- 내부 변수 ---
    private Transform _target;
    private Vector3 _startPos;
    private float _t;
    private float _damage; // 타워가 설정해줄 데미지

    // 타워로부터 전달받을 상태 이상 정보 변수
    private StatusEffect _effectToApply;

    /// <summary>
    /// 타워에서 발사체를 생성할 때 호출할 단일 초기화 함수입니다.
    /// </summary>
    /// <param name="target">목표 대상</param>
    /// <param name="damage">적용할 데미지</param>
    /// <param name="effect">적용할 상태 이상 정보</param>
    public void Setup(Transform target, float damage, StatusEffect effect)
    {
        _target = target;
        _damage = damage;
        _effectToApply = effect;

        _startPos = transform.position;
        _t = 0f;
    }

    private void Update()
    {
        // 목표가 사라졌을 경우 (이미 죽었을 경우)
        if (_target == null)
        {
            // 현재 위치에 효과를 적용하고 소멸 (예: 범위 공격)
            ApplyDamageAndEffect(transform.position);
            Destroy(gameObject);
            return;
        }

        // 목표를 향해 이동
        MoveTowardsTarget();

        // 목표에 도달했을 경우
        if (Vector3.Distance(transform.position, _target.position) <= arriveDistance)
        {
            ApplyDamageAndEffect(_target.position);
            Destroy(gameObject);
        }
    }

    private void MoveTowardsTarget()
    {
        switch (motionType)
        {
            case ProjectileMotionType.Straight:
                transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);
                break;
            case ProjectileMotionType.Parabola:
                _t += Time.deltaTime * speed / Mathf.Max(0.001f, Vector3.Distance(_startPos, _target.position));
                var pos = Vector3.Lerp(_startPos, _target.position, Mathf.Clamp01(_t));
                pos.y += arcHeight * Mathf.Sin(Mathf.Clamp01(_t) * Mathf.PI);
                transform.position = pos;
                break;
        }
    }

    /// <summary>
    /// 목표 지점에 도달했을 때 데미지와 상태 이상 효과를 모두 적용합니다.
    /// </summary>
    private void ApplyDamageAndEffect(Vector3 hitPosition)
    {
        // 상태 이상 효과 적용 로직 
        // 목표가 아직 살아있고, 적용할 효과가 있을 경우에만 실행
        if (_target != null && _effectToApply != null && _effectToApply.Type != StatusEffectType.None)
        {
            EnemyStatusManager statusManager = _target.GetComponent<EnemyStatusManager>();
            if (statusManager != null)
            {
                // 적의 상태 이상 매니저에게 효과 적용을 요청
                statusManager.ApplyStatusEffect(_effectToApply);
            }
        }

        // 기존의 데미지 효과 적용 로직 (DamageEffectManager 호출)
        if (DamageEffectManager.Instance != null)
        {
            var forward = transform.right;
            DamageEffectManager.Instance.ApplyEffect(
                effectType,
                hitPosition,
                _damage, // 타워에서 설정해준 데미지 사용
                _target,
                radius,
                coneAngle,
                forward
            );
        }
        else
        {
            Debug.LogWarning("DamageEffectManager 인스턴스가 씬에 없습니다!");
        }
    }
}
