using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType { Direct, Explosive, GroundAoE }

public class Projectile : MonoBehaviour
{
    public ElementType elementType;

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
    private Vector3 _lastKnownPosition; // 타겟의 마지막 위치를 저장할 변수
    private bool _targetIsLost = false; // 타겟을 잃어버렸는지 확인하는 플래그

    // 타워로부터 전달받을 상태 이상 정보 변수
    private StatusEffect _effectToApply;
    // 충돌음 정보 저장 변수
    private SfxType _impactSound;

    /// <summary>
    /// 타워에서 발사체를 생성할 때 호출할 단일 초기화 함수입니다.
    /// </summary>
    /// <param name="target">목표 대상</param>
    /// <param name="damage">적용할 데미지</param>
    /// <param name="effect">적용할 상태 이상 정보</param>


    public void Setup(Transform target, float damage, StatusEffect effect, SfxType impactSound)
    {
        _target = target;
        _damage = damage;
        _effectToApply = effect;
        _impactSound = impactSound; // 전달받은 충돌음 정보를 변수에 저장

        _startPos = transform.position;
        _t = 0f;
    }

    private void Update()
    {
        // 타겟이 사라졌는지 확인
        if (!_targetIsLost && _target == null)
        {
            _targetIsLost = true; // 타겟을 잃어버렸다고 표시
        }

        // 목표 지점을 향해 이동
        MoveTowardsTarget();

        // 목표 지점 도착 여부 확인
        Vector3 targetPosition = _targetIsLost ? _lastKnownPosition : _target.position;
        if (Vector3.Distance(transform.position, targetPosition) <= arriveDistance)
        {
            ApplyEffectAndDestroy(targetPosition);
        }
    }

    private void ApplyEffectAndDestroy(Vector3 hitPosition)
    {
        // 저장해 둔 충돌음을 재생하는 코드를 추가합니다.
        // 재생할 충돌음이 있는지 확인 ('None'이 아니면 재생)
        if (_impactSound != SfxType.None)
        {
            AudioManager.Instance.PlaySFXAtPoint(_impactSound, hitPosition);
        }

        ApplyDamageAndEffect(hitPosition);
        Destroy(gameObject);
    }

    private void MoveTowardsTarget()
    {
        // 이동 전에 항상 마지막 위치를 갱신
        if (!_targetIsLost)
        {
            _lastKnownPosition = _target.position;
        }

        // 목표 위치 설정
        Vector3 targetPosition = _lastKnownPosition;

        switch (motionType)
        {
            case ProjectileMotionType.Straight:
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                break;
            case ProjectileMotionType.Parabola:
                _t += Time.deltaTime * speed / Mathf.Max(0.001f, Vector3.Distance(_startPos, targetPosition));
                var pos = Vector3.Lerp(_startPos, targetPosition, Mathf.Clamp01(_t));
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
        if (!_targetIsLost && _target != null && _effectToApply != null && _effectToApply.Type != StatusEffectType.None)
        {
            EnemyStatusManager statusManager = _target.GetComponent<EnemyStatusManager>();
            if (statusManager != null)
            {
                statusManager.ApplyStatusEffect(_effectToApply);
            }
        }

        if (DamageEffectManager.Instance != null)
        {
            var forward = transform.right;

            // 타겟이 사라졌다면 null을, 아니라면 _target을 넘겨준다.
            Transform targetToSend = _targetIsLost ? null : _target;

            DamageEffectManager.Instance.ApplyEffect(
                effectType,
                hitPosition,
                _damage,
                targetToSend, // 수정된 targetToSend 변수를 사용
                radius,
                coneAngle,
                forward,
                elementType
            );
        }
        else
        {
            Debug.LogWarning("DamageEffectManager 인스턴스가 씬에 없습니다!");
        }
    }
}
