using UnityEngine;

public enum HitType { Direct, Explosive, GroundAoE }

public class Projectile : MonoBehaviour
{
    // 모든 설정 변수를 private으로 변경 
    // 스크립트의 인스펙터 창에는 아무것도 안뜸
    private ElementType elementType;
    private ProjectileMotionType motionType;
    private float speed;
    private float arcHeight;
    private float arriveDistance = 0.3f;
    private HitType hitType;
    private DamageEffectType damageEffectType;
    private float radius;
    private float coneAngle;

    // 내부 동작 변수 
    private Transform _target;
    private Vector3 _startPos;
    private float _t;
    private float _damage;
    private Vector3 _lastKnownPosition;
    private bool _targetIsLost = false;
    private StatusEffect _effectToApply;
    private SfxType _impactSound;
    private float _effectBuildup;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// TowerData로부터 모든 행동 지침을 받아 초기화합니다.
    /// </summary>
    public void Setup(Transform target, float damage, StatusEffect effect, float effectBuildup, SfxType impactSound, TowerData data)
    {
        // 기본 정보 설정
        _target = target;
        _damage = damage;
        _effectToApply = effect;
        _effectBuildup = effectBuildup;
        _impactSound = impactSound;

        // TowerData로부터 모든 능력치를 복사
        elementType = data.ElementType;
        speed = data.projectileSpeed;
        arcHeight = data.arcHeight;
        hitType = data.hitType;
        damageEffectType = data.damageEffectType;
        radius = data.radiant;

        // AttackMode를 ProjectileMotionType으로 변환
        motionType = data.AttackMode switch
        {
            AttackMode.Parabolic => ProjectileMotionType.Parabola,
            _ => ProjectileMotionType.Straight,
        };

        // 시작 위치 및 시간 초기화
        _startPos = transform.position;
        _t = 0f;
    }

    private void Update()
    {
        // 타겟 정보가 아예 없이 생성되었다면 스스로를 파괴하고 즉시 종료
        if (_target == null && !_targetIsLost)
        {
            Destroy(gameObject);
            return;
        }

        if (speed <= 0) return;

        if (_target != null && !_targetIsLost)
        {
            _lastKnownPosition = _target.position;
        }
        else if (_target == null && !_targetIsLost)
        {
            _targetIsLost = true;
        }

        MoveTowardsTarget();

        if (Vector3.Distance(transform.position, _lastKnownPosition) <= arriveDistance)
        {
            OnTargetHit(_lastKnownPosition);
        }
    }

    private void OnTargetHit(Vector3 hitPosition)
    {
        speed = 0;

        if (_impactSound != SfxType.None)
        {
            AudioManager.Instance.PlaySFXAtPoint(_impactSound, hitPosition);
        }

        switch (hitType)
        {
            case HitType.Direct:
                if (!_targetIsLost && _target != null)
                {
                    var enemy = _target.GetComponent<NormalEnemy>();
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

        if (_animator != null)
        {
            _animator.SetTrigger("onHit");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnImpactAnimationEnd()
    {
        Destroy(gameObject);
    }

    private void ApplyAoeDamage(Vector3 hitPosition)
    {
        if (DamageEffectManager.Instance != null)
        {
            Transform targetToSend = _targetIsLost ? null : _target;
            DamageEffectManager.Instance.ApplyEffect(
                damageEffectType, hitPosition, _damage, targetToSend,
                radius, coneAngle, transform.right, elementType
            );
        }
    }

    private void MoveTowardsTarget()
    {
        switch (motionType)
        {
            case ProjectileMotionType.Straight:
                transform.position = Vector3.MoveTowards(transform.position, _lastKnownPosition, speed * Time.deltaTime);
                break;
            case ProjectileMotionType.Parabola:
                _t += Time.deltaTime * speed / Mathf.Max(0.001f, Vector3.Distance(_startPos, _lastKnownPosition));
                var pos = Vector3.Lerp(_startPos, _lastKnownPosition, Mathf.Clamp01(_t));
                pos.y += arcHeight * Mathf.Sin(Mathf.Clamp01(_t) * Mathf.PI);
                transform.position = pos;
                break;
        }
    }
}
