using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 경로 설정")]
    public List<Vector3> waypoints = new List<Vector3>(); // 이동할 웨이포인트 목록
    private int currentWaypointIndex = 0; // 현재 이동 중인 웨이포인트 인덱스
    private NormalEnemy normalEnemyStats; // NormalEnemy 스크립트 인스턴스 참조 (이동 속도 등)
    private float currentMoveSpeed; // 현재 적용되는 이동 속도

    // 적이 경로의 끝에 도달했을 때 발생할 액션
    public static event Action OnReachEndPoint;
    public static event Action OnEnemyDestroyed;


    private void Awake()
    {
        // NormalEnemy 컴포넌트 가져오기
        normalEnemyStats = GetComponent<NormalEnemy>();
        if (normalEnemyStats == null)
        {
            Debug.LogError("EnemyMovement: NormalEnemy 컴포넌트를 찾을 수 없습니다. 적 프리팹에 NormalEnemy 스크립트가 연결되어 있는지 확인하세요.");
        }
    }

    // 초기 위치를 설정하는 메서드 (외부에서 호출)
    public void SetInitialPosition(Vector3 initialPosition)
    {
        transform.position = initialPosition;
    }

    // 경로(waypoints)를 설정하는 메서드 (외부, 주로 SpawnManager에서 호출)
    public void SetPath(List<Vector3> pathPoints)
    {
        if (pathPoints != null && pathPoints.Count > 0)
        {
            waypoints = pathPoints;
            // 경로 시작점부터 이동
            currentWaypointIndex = 0;
            // 적이 경로 시작점으로 순간이동하지 않도록 현재 위치를 첫 웨이포인트로 설정
            transform.position = waypoints[0];
        }
        else
        {
            Debug.LogWarning("EnemyMovement: 유효한 경로 데이터가 필요합니다. 경로가 비어있거나 null입니다.");
        }
    }

    private void Update()
    {
        // 필요한 참조가 없거나 경로가 비어있으면 이동하지 않음
        if (normalEnemyStats == null || waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        // 현재 이동 속도 가져오기
        currentMoveSpeed = normalEnemyStats.moveSpeed;

        // 다음 웨이포인트가 남아있는지 확인
        if (currentWaypointIndex < waypoints.Count)
        {
            // 현재 목표 웨이포인트 위치
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            // 목표 위치로 이동
            float step = currentMoveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            bool isLastWayPoint = currentWaypointIndex == waypoints.Count - 1;
            // 다음 목표와 얼만큼 도달했는지 체크하는 거리 조정
            float distWayPoint = isLastWayPoint ? 0.5f : 0.05f;

            // 목표 위치에 거의 도달했는지 확인 (충분히 가까워지면 다음 웨이포인트로 이동)
            if (Vector3.Distance(transform.position, targetPosition) < distWayPoint)
            {
                currentWaypointIndex++; // 다음 웨이포인트로 이동 목표 변경
            }
        }
        else
        {
            // 적이 마지막 타일 바깥을 빠져나가 없어지게 함
            if (Vector3.Distance(transform.position, waypoints[^1]) < 0.5f) ReachedEndOfPath();
        }
    }

    // 적이 경로의 최종 목적지에 도달했을 때 호출되는 메서드
    void ReachedEndOfPath()
    {
        Debug.Log("EnemyMovement: 적이 최종 목적지에 도달했습니다.");

        // GameManager에 목숨 감소 요청
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(); // GameManager의 LoseLife() 호출
        }
        else
        {
            Debug.LogError("EnemyMovement: GameManager 인스턴스를 찾을 수 없습니다. 목숨을 감소시킬 수 없습니다.");
        }

        // OnReachEndPoint 이벤트를 구독하고 있는 다른 스크립트들에게 알림 (선택 사항)
        OnReachEndPoint?.Invoke();
        // 적 GameObject 파괴 (또는 오브젝트 풀에 반환)
        Destroy(gameObject);
    }

    public void Die()
    {
        OnEnemyDestroyed?.Invoke();
        Destroy(gameObject);
    }

    private void ReachEnd()
    {
        OnReachEndPoint?.Invoke();
        Destroy(gameObject);
    }
}
