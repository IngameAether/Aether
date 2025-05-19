using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("이동 경로 설정")]
    // 이동할 경로 지점 목록
    public List<Vector3> waypoints = new List<Vector3>();
    // 이동 속도
    private int currentWaypointIndex = 0;
    private NormalEnemy normalEnemyStats; // NormalEnemy 인스턴스

    // 초기 위치를 설정하는 메소드
    public void SetInitialPosition(Vector3 initialPosition)
    {
        transform.position = initialPosition;
    }

    // 경로(wayPoints)를 설정하는 메소드(외부에서 호출, SpawnManager에서 사용)
    public void SetPath(List<Vector3> pathPoints)
    {
        if (pathPoints != null && pathPoints.Count > 0)
        {
            waypoints = pathPoints;
            // 경로 시작부터 이동
            currentWaypointIndex = 0; 
        }
        else
        {
            Debug.LogWarning("EnemyMovement: 유효한 경로 설정이 필요");
        }
    }

    private void Update()
    {
        // 이동할 웨이포인트가 있는지 확인
        if (currentWaypointIndex < waypoints.Count)
        {
            // 현재 목표 웨이포인트 위치
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            // 목표 위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, normalEnemyStats.moveSpeed * Time.deltaTime);
            // 목표 위치에 거의 도달했는지 확인
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                // 다음 웨이포인트로 이동 목표 변경
                currentWaypointIndex++;
            }
        }
        else
        {
            // 경로 이동이 완료됐다면
            ReachedEndOfPath();
        }
    }
    // 경로의 끝에 도달했을 때 호출되는 메소드
    void ReachedEndOfPath()
    {
        Debug.Log("적이 목표 지점에 도달");
        // 오브젝트 파괴
        Destroy(gameObject);
    }
}