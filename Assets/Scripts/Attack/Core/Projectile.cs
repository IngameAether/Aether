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
    // 값을 받아서 저장했다가 적에게 명중 시 전달
    private float _effectBuildup;
    // Animator 참조 변수
    private Animator _animator;

    private void Awake()
    {
        // Animator 컴포넌트를 미리 찾아둡니다.
        _animator = GetComponent<Animator>();
    }

    /// 타워에서 발사체를 생성할 때 호출할 단일 초기화 함수입니다.
    public void Setup(Transform target, float damage, StatusEffect effect, float effectBuildup, SfxType impactSound)
    {
        _target = target;
        _damage = damage;
        _effectToApply = effect;
        _effectBuildup = effectBuildup; // 전달받은 누적치 정보를 변수에 저장
        _impactSound = impactSound;

        _startPos = transform.position;
        _t = 0f;
    }

    private void Update()
    {
        // 이미 목표 지점에 도달해서 멈췄다면 더 이상 업데이트하지 않음
        if (speed <= 0) return;

        // 타겟이 살아있고 아직 잃어버리지 않았다면, 마지막 위치를 계속 갱신
        if (_target != null && !_targetIsLost)
        {
            _lastKnownPosition = _target.position;
        }
        // 타겟이 사라졌는지 확인 (최초 1회만 실행)
        else if (_target == null && !_targetIsLost)
        {
            _targetIsLost = true;
        }

        // 목표 지점을 향해 계속 이동
        MoveTowardsTarget();

        // 목표 지점(마지막으로 알던 위치) 도착 여부 확인
        if (Vector3.Distance(transform.position, _lastKnownPosition) <= arriveDistance)
        {
            OnTargetHit(_lastKnownPosition);
        }
    }

    public void OnImpactAnimationEnd()
    {
        Destroy(gameObject);
    }

    // 적 명중 시 호출되는 함수
    private void OnTargetHit(Vector3 hitPosition)
    {
        // 더 이상 움직이지 않도록 속도를 0으로 설정
        speed = 0;

        // 충돌음 재생
        if (_impactSound != SfxType.None)
        {
            AudioManager.Instance.PlaySFXAtPoint(_impactSound, hitPosition);
        }

        // 데미지 및 상태이상 적용
        switch (hitType)
        {
            case HitType.Direct:
                if (!_targetIsLost && _target != null)
                {
                    NormalEnemy enemy = _target.GetComponent<NormalEnemy>();
                    if (enemy != null)
                    {
                        enemy.TakeHit(_damage, _effectToApply, _effectBuildup);
                    }
                }
                break;
            case HitType.Explosive:
            case HitType.GroundAoE:
                ApplyAoeDamage(hitPosition);
                break;
        }

        // 애니메이터가 있다면 'onHit' 트리거를 발동시켜 부딪히는 모션을 재생
        if (_animator != null)
        {
            _animator.SetTrigger("onHit");
            // 파괴는 애니메이션 이벤트(OnImpactAnimationEnd)가 담당하므로 여기서 Destroy하지 않음
        }
        else
        {
            // 애니메이터가 없다면 즉시 파괴
            Destroy(gameObject);
        }
    }

    // 범위 공격 로직을 별도 함수로 분리
    private void ApplyAoeDamage(Vector3 hitPosition)
    {
        if (DamageEffectManager.Instance != null)
        {
            var forward = transform.right;
            Transform targetToSend = _targetIsLost ? null : _target;

            DamageEffectManager.Instance.ApplyEffect(
                effectType, hitPosition, _damage, targetToSend,
                radius, coneAngle, forward, elementType
            );
        }
        else
        {
            Debug.LogWarning("DamageEffectManager 인스턴스가 씬에 없습니다!");
        }
    }

    private void MoveTowardsTarget()
    {
        // 목표 위치는 항상 _lastKnownPosition을 사용
        Vector3 targetPosition = _lastKnownPosition;

        // motionType에 따라 이동
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
}
