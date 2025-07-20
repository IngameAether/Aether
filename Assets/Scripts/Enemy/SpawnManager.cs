using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("적 스폰 설정")]
    // 스폰할 적 프리탭
    public GameObject[] enemyPrefabs;
    public GameObject enemyPrefab;
    public MapManage mapManage; // MapManage 스크립트 레퍼런스

    [Header("웨이브 설정")]
    public List<Wave> waves;
    public float initialDelay = 1f; // 첫 웨이브 시작 전 대기 시간
    public float waveBreakTime = 10f; // 웨이브 간 대기 시간

    // 현재까지 스폰된 적 수
    private int enemiesSpawnedInCurrentWave = 0;
    // 스폰이 현재 진행 중인지 확인하는 플래그
    private bool isSpawning = false;
    private static int currentWaveLevel = 0;

    private void Start()
    {
        // Inspector에서 프리팹과 MapManage 레퍼런스가 연결되었는지 확인
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnManager: 스폰할 적 프리팹이 지정되지 않았습니다!");
            return;
        }
        if (mapManage == null)
        {
            Debug.LogError("SpawnManager: MapManage 레퍼런스가 지정되지 않았습니다!");
            return;
        }
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("SpawnManager: 웨이브 설정이 비어있습니다!");
            return;
        }
        Debug.Log("SpawnManager 초기화 완료. Start 버튼 클릭 대기 중.");
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        isSpawning = true; // 스폰 시작 플래그 설정

        // MapManage로부터 생성된 경로 데이터를 가져옴
        List<Vector3> path = mapManage.GetPathWorldPositions();
        if (path == null || path.Count == 0)
        {
            Debug.LogError("SpawnManager: 유효한 경로 데이터를 가져올 수 없습니다. 적 스폰 중단.");
            isSpawning = false;
            yield break; // 경로가 없으면 코루틴 종료
        }

        // 적이 마지막 타일을 벗어나게 하기 위하여 마지막 방향으로 타일 한 칸 더 추가
        if (path.Count >= 2)
        {
            Vector3 dir = (path[^1] - path[^2]).normalized;
            path.Add(path[^1] + dir);
        }

        // 시작 지연 시간 대기
        yield return new WaitForSeconds(initialDelay);

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            currentWaveLevel = waveIndex;
            Wave currentWave = waves[waveIndex];
            Debug.Log($"--- 웨이브 {waveIndex + 1} 시작! (적 {currentWave.numberOfEnemies}마리) ---");

            enemiesSpawnedInCurrentWave = 0; // 현재 웨이브 스폰된 적 수 초기화

            while (enemiesSpawnedInCurrentWave < currentWave.numberOfEnemies)
            {
                Vector3 spawnPosition = path[0]; // 경로의 첫 번째 지점을 스폰 위치로 사용
                SpawnSingleEnemy(spawnPosition, path);
                enemiesSpawnedInCurrentWave++;  

                // 현재 웨이브의 모든 적을 스폰하기 전까지는 스폰 간격 대기
                if (enemiesSpawnedInCurrentWave < currentWave.numberOfEnemies)
                {
                    yield return new WaitForSeconds(currentWave.spawnInterval);
                }
            }

            Debug.Log($"--- 웨이브 {waveIndex + 1} 적 스폰 완료. ---");

            // 마지막 웨이브가 아니라면 웨이브 간 대기 시간 적용
            if (waveIndex < waves.Count - 1)
            {
                Debug.Log($"다음 웨이브까지 {waveBreakTime}초 대기...");
                yield return new WaitForSeconds(waveBreakTime);
            }
            else
            {
                Debug.Log("모든 웨이브 완료!");
            }
        }

        isSpawning = false; // 모든 웨이브 스폰 종료 플래그 설정
    }

    void SpawnSingleEnemy(Vector3 initialSpawnPosition, List<Vector3> path)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("스폰할 적 프리팹을 지정해야함");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefabs[currentWaveLevel], initialSpawnPosition, Quaternion.identity);
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.SetInitialPosition(initialSpawnPosition);
            enemyMovement.SetPath(path);
        }
        else
        {
            Debug.LogError("SpawnManager: 스폰된 적 프리팹에 EnemyMovement 컴포넌트가 없습니다.");
        }
    }
}

[System.Serializable] 
public class Wave
{
    public int numberOfEnemies; // 이 웨이브에서 스폰할 적의 수
    public float spawnInterval; // 이 웨이브에서 적 스폰 간격
}