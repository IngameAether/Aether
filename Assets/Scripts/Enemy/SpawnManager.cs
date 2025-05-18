using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("적 스폰 위치 설정")]
    // 스폰할 적 프리탭
    public GameObject enemyPrefab;
    // 적이 처음 스폰될 위치 (SetInitialPosition 메소드에서 사용)
    public Transform spawnPoint;
    // 적이 따라 이동할 경로 (watpoints 목록)
    public List<Vector3> enemyPath;

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
        // 코루틴을 사용하여 일정 간격으로 적 스폰 시작
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        // 시작 지연 시간 대기
        yield return new WaitForSeconds(startDelay);

        while (enemiesSpawned < numberOfEnemiesToSpawn)
        {
            SpawnEnemy();
            enemiesSpawned++;
            // 다음 스폰까지 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab = null)
        {
            Debug.Log("스폰할 적 프리팹을 지정해야함");
            return;
        }

        // 적 프리팹 인스턴스 생성
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        // EnemyMovement 스크립트 가져오기
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            // 초기 위치 설정 (SpawnManager의 위치 or 별도 지정 가능)
            // 메소드를 통해 지정
            if (spawnPoint != null){
                enemyMovement.SetInitialPosition(spawnPoint.position);
            }
            else 
            {
                // SpawnManager의 위치를 초기 위치로 설정
                enemyMovement.SetInitialPosition(transform.position);
            }

            // 이동 경로 설정
            if (enemyPath != null && enemyPath.Count > 0)
            {
                enemyMovement.SetPath(enemyPath);
            }
            else
            {
                // 경로가 설정되지 않았을 경우 EnemyMovement 스크립트의 기본경로로 설정
                Debug.LogWarning(" 적 이동 경로가 설정되지 않았음. EnemyMovement의 기본경로로 설정");
                // 또는 이동하지 않도록 설정할 수 있음
                // newEnemy.GetComponent<EnemyMovement>().enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("적 프리팹에 EnemyMovement 스크립트가 없음");
        }
    }
}
