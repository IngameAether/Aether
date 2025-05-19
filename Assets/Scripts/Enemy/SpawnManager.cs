using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("적 스폰 위치 설정")]
    // 스폰할 적 프리탭
    public GameObject enemyPrefab;
    [Header("스폰 위치 및 경로 설정")]
    // 적이 처음 스폰될 위치 (SetInitialPosition 메소드에서 사용)
    public Transform spawnPoint;
    // 적이 따라 이동할 경로 (watpoints 목록)
    public List<Vector3> enemyPath = new List<Vector3>();

    [Header("스폰 타이밍 설정")]
    // 스폰 시작까지 대기 시간
    public float startDelay = 1f;
    // 적 스폰 간격
    public float spawnInterval = 1f;
    // 스폰할 적의 총 마릿수 = 3;
    public int numberOfEnemiesToSpawn = 3;
    // 현재까지 스폰된 적 수
    private int enemiesSpawned = 0;

    private void Start()
    {
        // Inspector에서 경로가 비어있을 경우 경고 메시지를 출력
        if (enemyPath == null || enemyPath.Count == 0)
        {
            Debug.LogWarning("SpawnManager: 적 이동 경로(enemyPath)가 설정되지 않았습니다!");
        }
        // 코루틴을 사용하여 일정 간격으로 적 스폰 시작
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        // 시작 지연 시간 대기
        yield return new WaitForSeconds(startDelay);

        while (enemiesSpawned < numberOfEnemiesToSpawn)
        {
            SpawnSingleEnemy(); // 적 하나 스폰
            enemiesSpawned++;   // 스폰된 적 수 증가
            // 모든 적 다 스폰했을 경우
            if (enemiesSpawned >= numberOfEnemiesToSpawn)
            {
                Debug.Log("모든 적 스폰 완료");
                yield break; // 루프 종료
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnSingleEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("스폰할 적 프리팹을 지정해야함");
            return;
        }

        Vector3 initialSpawnPosition;
        if (spawnPoint != null)
        {
            // spawnPoint Transform이 지정되어 있으면 해당 위치를 사용
            initialSpawnPosition = spawnPoint.position;
            Debug.Log($"SpawnManager: 적 #{enemiesSpawned + 1}을 지정된 스폰 지점({initialSpawnPosition})에 스폰.");
        }
        else
        {
            // spawnPoint가 지정되지 않았으면 SpawnManager 오브젝트 자체의 위치를 사용
            initialSpawnPosition = transform.position;
            Debug.Log($"SpawnManager: 적 #{enemiesSpawned + 1}을 SpawnManager 위치({initialSpawnPosition})에 스폰.");
        }

        // 적 프리팹 인스턴스 생성
        GameObject newEnemy = Instantiate(enemyPrefab, initialSpawnPosition, Quaternion.identity);
        // EnemyMovement 스크립트 가져오기
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.SetPath(enemyPath);
            Debug.Log($"SpawnManager: 적 #{enemiesSpawned + 1}에 이동 경로 설정 완료.");
        }
        else
        {
            Debug.LogWarning("적 프리팹에 EnemyMovement 스크립트가 없음");
        }
    }
}
