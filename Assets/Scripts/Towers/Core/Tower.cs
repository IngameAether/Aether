using System;
using UnityEngine;

namespace Towers.Core
{

    [Serializable]
    public class TowerSetting
    {
        [field: Header("Tower Stats")]
        [field: SerializeField]
        public string name { get; private set; }

        [field: SerializeField] public string description { get; private set; }
        [field: SerializeField] public int rank { get; private set; }
        [field: SerializeField] public float damage { get; private set; }
        [field: SerializeField] public float attackDelay { get; private set; }
        [field: SerializeField] public float range { get; private set; }
        [field: SerializeField] public float criticalHit { get; private set; }
    }

    public abstract class Tower : MonoBehaviour
    {
        [Header("Tower Configuration")] [SerializeField]
        protected TowerSetting towerSetting;

        protected SpriteRenderer spriteRenderer;
        protected SpriteRenderer magicCircleRenderer;

        protected float lastAttackTime = 0f;
        protected bool isFacingRight = true;
        protected Transform currentTarget;  // 현재 타겟으로 삼고 있는 적의 위치
        public Vector3 direction;   // 적 방향

        public static event Action<Tower> OnTowerClicked;

        public TowerSetting GetTowerSetting() => towerSetting;
        public Vector3 GetPosition() => transform.position;
        public bool GetIsFacingRight() => isFacingRight;

        protected virtual void Start()
        {
            InitializeTower();
        }

        protected virtual void Update()
        {
            // 타겟 찾기 및 공격
            FindAndAttackTarget();
        }

        /// <summary>
        /// 타워 초기화 - 컴포넌트 설정 및 스프라이트 적용
        /// </summary>
        protected virtual void InitializeTower()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            magicCircleRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// 타워 좌우반전 처리
        /// </summary>
        public void FlipTower()
        {
            isFacingRight = !isFacingRight;

            spriteRenderer.flipX = isFacingRight;
            magicCircleRenderer.flipX = isFacingRight;
        }

        #region Tower Find Target & Attack

        /// <summary>
        /// 타겟을 찾고 공격하는 함수
        /// </summary>
        protected virtual void FindAndAttackTarget()
        {
            if (!IsTargetInRange(currentTarget))
            {
                currentTarget = FindNearestTarget();
                return;
            }

            if (CanAttack())
            {
                direction = (currentTarget.transform.position - transform.position).normalized;
                Attack();
                lastAttackTime = Time.time;
            }
        }

        /// <summary>
        /// 타겟이 사거리 안에 있는지 확인
        /// </summary>
        protected virtual bool IsTargetInRange(Transform target)
        {
            if (!target) return false;

            float distance = Vector2.Distance(transform.position, target.position);
            return distance <= towerSetting.range; // 타겟과의 거리가 사거리보다 작거나 같은지 확인
        }

        /// <summary>
        /// 가장 가까운 적을 찾는 함수
        /// </summary>
        protected virtual Transform FindNearestTarget()
        {
            LayerMask enemyLayerMask = LayerMask.GetMask("Enemy");

            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
                transform.position,
                towerSetting.range,
                enemyLayerMask
            );

            if (enemiesInRange.Length == 0) return null;

            Transform nearestTarget = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider2D enemyCollider in enemiesInRange)
            {
                if (enemyCollider.gameObject == null) continue;

                float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = enemyCollider.transform;
                }
            }

            return nearestTarget;
        }

        // 타겟이 살아 있는지 확인
        protected virtual bool isTargetAlive(Transform target)
        {
            if (!target) return false;

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null) return false;

            return damageable.CurrentHealth > 0f;
        }

        /// <summary>
        /// 공격 가능한지 확인 (딜레이 체크)
        /// </summary>
        protected virtual bool CanAttack()
        {
            bool isDelayOver = Time.time >= lastAttackTime + towerSetting.attackDelay;
            bool isAlive = isTargetAlive(currentTarget);
            return isDelayOver && isAlive;
        }

        /// <summary>
        /// 타워 공격
        /// </summary>
        protected abstract void Attack();

        #endregion

        /// <summary>
        /// 마우스 클릭 감지
        /// </summary>
        public void HandleTowerClicked()
        {
            OnTowerClicked?.Invoke(this);
        }
    }
}