using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Motion")]
    public ProjectileMotionType motionType = ProjectileMotionType.Straight;
    public float speed = 10f;
    public float arcHeight = 2f;
    public float arriveDistance = 0.3f;

    public enum HitType { Direct, Explosive, GroundAoE }

    [SerializeField] private HitType hitType = HitType.Direct;
    [SerializeField] private DamageEffectType effectType = DamageEffectType.SingleTarget;

    [Header("Damage / AoE")]
    [SerializeField] private float damage = 10f;      // 타워가 세팅해줌
    [SerializeField] private float radius = 2f;       // 폭발/콘 반경
    [SerializeField] private float coneAngle = 60f;   // 콘 각도(전체 각)

    private Transform _target;
    private Vector3 _startPos;
    private float _t;

    public void SetDamage(float dmg) => damage = dmg;

    // 기본 초기화(인스펙터에서 설정한 hit/effect 유지)
    public void Init(Transform target)
    {
        _target = target;
        _startPos = transform.position;
        _t = 0f;
    }

    // 타입도 함께 지정하고 싶을 때
    public void Init(Transform target, HitType hit, DamageEffectType effect)
    {
        _target = target;
        hitType = hit;
        effectType = effect;
        _startPos = transform.position;
        _t = 0f;
    }

    // 혹시 따로 바꾸고 싶을 때용
    public void SetTypes(HitType hit, DamageEffectType effect)
    {
        hitType = hit;
        effectType = effect;
    }

    private void Update()
    {
        if (_target == null)
        {
            ApplyEffect(transform.position);
            Destroy(gameObject);
            return;
        }

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
            case ProjectileMotionType.None:
                ApplyEffect(_target.position);
                Destroy(gameObject);
                return;
        }

        if (Vector3.Distance(transform.position, _target.position) <= arriveDistance)
        {
            ApplyEffect(_target.position);
            Destroy(gameObject);
        }
    }

    private void ApplyEffect(Vector3 pos)
    {
        if (DamageEffectManager.Instance == null)
        {
            Debug.LogWarning("DamageEffectManager 인스턴스가 씬에 없습니다!");
            return;
        }

        var forward = transform.right; // 2D 기준; 프로젝트 규칙에 맞게 수정 가능
        DamageEffectManager.Instance.ApplyEffect(
            effectType,
            pos,
            damage,
            _target,
            radius,
            coneAngle,
            forward
        );
    }
}
