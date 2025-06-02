using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("적 스폰 설정")]
    // 스폰할 적 프리탭
    public GameObject enemyPrefab;
    public MapManage mapManage; // MapManage 스크립트 레퍼런스

    [Header("스폰 타이밍 설정")]
    // 스폰 시작까지 대기 시간
    public float startDelay = 1f;
    // 적 스폰 간격
    public float spawnInterval = 0.5f;
    // 스폰할 적의 총 마릿수
    public int numberOfEnemiesToSpawn = 20;
    // 현재까지 스폰된 적 수
    private int enemiesSpawned = 0;
    // 스폰이 현재 진행 중인지 확인하는 플래그
    private bool isSpawning = false;

    private void Start()
    {
        // Inspector에서 프리팹과 MapManage 레퍼런스가 연결되었는지 확인
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnManager: 스폰할 적 프리팹이 지정되지 않았습니다!");
        }
        if (mapManage == null)
        {
            Debug.LogError("SpawnManager: MapManage 레퍼런스가 지정되지 않았습니다!");
        }
        Debug.Log("SpawnManager 초기화 완료. Start 버튼 클릭 대기 중.");
    }

    // ClickBtn 스크립트의 Start 버튼 클릭 이벤트에 연결될 public 메서드
    public void StartSpawningFromButton()
    {
        if (enemyPrefab == null || mapManage == null)
        {
            Debug.LogError("스폰 시작 불가: 적 프리팹 또는 MapManage 레퍼런스가 누락되었습니다.");
            return;
        }

        if (isSpawning)
        {
            Debug.LogWarning("SpawnManager: 이미 스폰이 진행 중입니다.");
            return;
        }

        // 스폰 코루틴 시작
        Debug.Log("Start 버튼 클릭! 적 스폰 시작 코루틴 시작.");
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        isSpawning = true; // 스폰 시작 플래그 설정
        enemiesSpawned = 0; // 스폰된 적 수 초기화

        // MapManage로부터 생성된 경로 데이터를 가져옴
        List<Vector3> path = mapManage.GetPathWorldPositions();
        // 시작 지연 시간 대기
        yield return new WaitForSeconds(startDelay);

        while (enemiesSpawned < numberOfEnemiesToSpawn)
        {
            // 경로의 첫 번째 지점을 스폰 위치로 사용
            Vector3 spawnPosition = path[0];

            // 적 하나 스폰 및 경로 설정
            SpawnSingleEnemy(spawnPosition, path); // 아래 메서드 호출
            enemiesSpawned++;   // 스폰된 적 수 증가

            if (enemiesSpawned >= numberOfEnemiesToSpawn)
            {
                Debug.Log("모든 적 스폰 완료");
                break; // 루프 종료
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false; // 스폰 종료 플래그 설정
    }

    void SpawnSingleEnemy(Vector3 initialSpawnPosition, List<Vector3> path)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("스폰할 적 프리팹을 지정해야함");
            return;
        }

        // 적 프리팹 인스턴스 생성 (지정된 스폰 위치와 회전 없이)
        GameObject newEnemy = Instantiate(enemyPrefab, initialSpawnPosition, Quaternion.identity);

        // 생성된 적 오브젝트에서 EnemyMovement 스크립트 가져오기
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            // EnemyMovement 스크립트의 SetInitialPosition 메서드 호출
            enemyMovement.SetInitialPosition(initialSpawnPosition);
            // 가져온 경로(Vector3 리스트) 전달
            enemyMovement.SetPath(path); 
        }
    }

}
