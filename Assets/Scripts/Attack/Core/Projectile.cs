using TMPro;
using UnityEngine;

public enum HitType { Direct, Explosive, GroundAoE }

public class Projectile : MonoBehaviour
{
    // --- 설정 변수 (외부에서 주입받음) ---
    private float speed;
    private float arcHeight;
    private float arriveDistance = 0.3f;
    private bool _isGuided = false;
    private ProjectileMotionType motionType;

    // --- 내부 동작 변수 ---
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

    public void Setup(Transform target, float damage, StatusEffect effect, float effectBuildup, SfxType impactSound, TowerData data)
    {
        _target = target;
        _damage = damage;
        _effectToApply = effect;
        _effectBuildup = effectBuildup;
        _impactSound = impactSound;

        // TowerData로부터 모든 능력치를 복사
        speed = data.projectileSpeed;
        arcHeight = data.arcHeight;
        _isGuided = data.Guided;
        motionType = data.AttackMode == AttackMode.Parabolic ? ProjectileMotionType.Parabola : ProjectileMotionType.Straight;

        if (target != null)
        {
            _lastKnownPosition = target.position;
        }

        _startPos = transform.position;
        _t = 0f;
    }

    private void Update()
    {
        if (speed <= 0) return; // 공격이 명중했으면 멈춤

        // 타겟이 사라졌는지 확인
        if (_target == null && !_targetIsLost)
        {
            _targetIsLost = true;
        }

        // 목표 지점 결정
        Vector3 targetPosition = _lastKnownPosition;
        if (_isGuided && _target != null)
        {
            targetPosition = _target.position;
        }

        // 목표 지점을 향해 회전
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero) // 방향이 0이 아닐 때만 회전 (오류 방지)
        {
            transform.right = direction;
        }

        // 목표 지점을 향해 이동
        MoveTowards(targetPosition);

        // 목표 지점 도착 확인
        if (speed > 0 && Vector3.Distance(transform.position, _lastKnownPosition) <= arriveDistance)
        {
            OnTargetHit(_lastKnownPosition);
        }
    }

    private void MoveTowards(Vector3 targetPosition)
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

    private void OnTargetHit(Vector3 hitPosition)
    {
        speed = 0;

        // 타겟이 살아있을 때만 데미지, 효과, 사운드 적용
        if (!_targetIsLost && _target != null)
        {
            if (_impactSound != SfxType.None)
            {
                AudioManager.Instance.PlaySFXAtPoint(_impactSound, hitPosition);
            }

            var enemy = _target.GetComponent<NormalEnemy>();
            if (enemy != null)
            {
                enemy.TakeHit(_damage, _effectToApply, _effectBuildup);
            }
        }

        // 애니메이터가 있다면 부딪히는 모션 재생, 없다면 즉시 파괴
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
}
