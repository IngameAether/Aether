using TMPro;
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

    // 유도 기능 활성화 여부를 저장할 변수
    private bool _isGuided = false;

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
        _target = target;
        _damage = damage;
        _effectToApply = effect;
        _effectBuildup = effectBuildup;
        _impactSound = impactSound;

        // --- TowerData로부터 모든 능력치를 복사 ---
        elementType = data.ElementType;
        speed = data.projectileSpeed;
        arcHeight = data.arcHeight;
        hitType = data.hitType;
        damageEffectType = data.damageEffectType;
        radius = data.radiant;

        // TowerData로부터 Guided 설정을 받아옵니다.
        _isGuided = data.Guided;

        motionType = data.AttackMode switch
        {
            AttackMode.Parabolic => ProjectileMotionType.Parabola,
            _ => ProjectileMotionType.Straight,
        };

        // 타겟의 최초 위치를 _lastKnownPosition으로 설정
        if (target != null)
        {
            _lastKnownPosition = target.position;
        }

        _startPos = transform.position;
        _t = 0f;
    }

    private void Update()
    {
        // [디버그] 이 발사체가 누구를 쫓고 있는지 항상 확인
        if (_target != null)
        {
            Debug.DrawLine(transform.position, _target.position, Color.green); // 씬 뷰에 초록색 선으로 추적 경로 표시
        }

        // [디버그] 타겟이 사라지는 바로 그 순간을 포착!
        if (_target == null && !_targetIsLost)
        {
            Debug.LogError($"★★★ 타겟 상실! {gameObject.name}이(가) 타겟을 잃었습니다. 마지막 위치: {_lastKnownPosition}", this.gameObject);
            _targetIsLost = true;
        }

        if (speed <= 0) return;
        Vector3 targetPosition = _lastKnownPosition;

        // 기존 이동 로직, 위의 디버그는 필요없어지면 지우기
        if (_isGuided && _target != null)
        {
            targetPosition = _target.position;
        }

        // 발사체가 항상 목표 지점을 바라보도록 각도를 실시간으로 조절합니다.
        if (speed > 0)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.right = direction; // 2D 스프라이트의 오른쪽(x축)이 타겟을 향하도록 설정
        }

        MoveTowardsTarget(targetPosition);

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

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
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
