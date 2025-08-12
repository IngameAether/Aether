using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ����")]
    // ������ �� ������
    public GameObject[] enemyPrefabs;
    public MapManage mapManage; // MapManage ��ũ��Ʈ ���۷���
    public BuffChoiceUI buffChoiceUI;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;

    [Header("���̺� ����")]
    public List<Wave> waves;
    public float initialDelay = 1f; // ù ���̺� ���� �� ��� �ð�
    public float waveBreakTime = 10f; // ���̺� �� ��� �ð�

    // ������ ���� ���� ������ Ȯ���ϴ� �÷���
    private bool isSpawning = false;
    public static int currentWaveLevel = 0;
    private float _currentEndTime;
    private int _reachedEndEnemyCount = 0;
    private Coroutine _spawnCoroutine;

    private bool _isWaitingForBuffChoice = false;

    private void Start()
    {
        // Inspector���� �����հ� MapManage ���۷����� ����Ǿ����� Ȯ��
        if (enemyPrefabs == null)
        {
            Debug.LogError("SpawnManager: ������ �� �������� �������� �ʾҽ��ϴ�!");
            return;
        }
        if (mapManage == null)
        {
            Debug.LogError("SpawnManager: MapManage ���۷����� �������� �ʾҽ��ϴ�!");
            return;
        }
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("SpawnManager: ���̺� ������ ����ֽ��ϴ�!");
            return;
        }

        waveText.text = "0 wave";

        EnemyMovement.OnReachEndPoint += HandleReachedEndEnemy;
        buffChoiceUI.OnBuffChoiceCompleted += HandleBuffChoiceCompleted;

        Debug.Log("SpawnManager �ʱ�ȭ �Ϸ�. Start ��ư Ŭ�� ��� ��.");
        _spawnCoroutine = StartCoroutine(SpawnEnmiesRoutine());
    }

    private void OnDestroy()
    {
        EnemyMovement.OnReachEndPoint -= HandleReachedEndEnemy;
        buffChoiceUI.OnBuffChoiceCompleted -= HandleBuffChoiceCompleted;
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        isSpawning = true; // ���� ���� �÷��� ����

        // MapManage�κ��� ������ ��� �����͸� ������
        List<Vector3> path = mapManage.GetPathWorldPositions();
        if (path == null || path.Count == 0)
        {
            Debug.LogError("SpawnManager: ��ȿ�� ��� �����͸� ������ �� �����ϴ�. �� ���� �ߴ�.");
            isSpawning = false;
            yield break; // ��ΰ� ������ �ڷ�ƾ ����
        }

        // 적이 마지막 칸을 나가서 중앙에 오면 없어지도록 마지막 방향으로 타일 하나 추가
        if (path.Count >= 2)
        {
            Vector3 dir = (path[^1] - path[^2]).normalized;
            path.Add(path[^1] + dir);
        }

        yield return StartCoroutine(HandleBuffChoice());

        GameTimer.Instance.StartTimer();

        // ���� ���� �ð� ���
        yield return new WaitForSeconds(initialDelay);

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            currentWaveLevel = waveIndex;
            waveText.text = $"{waveIndex + 1} wave";

            yield return new WaitForSeconds(waves[waveIndex].startTime);

            Wave currentWave = waves[waveIndex];
            Debug.Log($"--- ���̺� {waveIndex + 1} ����!---");

            _currentEndTime = Time.time + waves[waveIndex].endTime;

            while (Time.time <= _currentEndTime)
            {
                Vector3 spawnPosition = path[0]; // ����� ù ��° ������ ���� ��ġ�� ���
                SpawnSingleEnemy(spawnPosition, path);

                // ���� ���̺��� ��� ���� �����ϱ� �������� ���� ���� ���
                if (Time.time <= _currentEndTime)
                {
                    yield return new WaitForSeconds(currentWave.spawnInterval);
                }
            }

            Debug.Log($"--- ���̺� {waveIndex + 1} �� ���� �Ϸ�. ---");

            if ((waveIndex + 1) % 10 == 0 && buffChoiceUI != null)
            {
                yield return StartCoroutine(HandleBuffChoice());
            }

            // ������ ���̺갡 �ƴ϶�� ���̺� �� ��� �ð� ����
            if (waveIndex < waves.Count - 1)
            {
                Debug.Log($"���� ���̺���� {waveBreakTime}�� ���...");
                yield return new WaitForSeconds(waveBreakTime);
            }
            else
            {
                Debug.Log("��� ���̺� �Ϸ�!");
            }
        }

        isSpawning = false; // ��� ���̺� ���� ���� �÷��� ����
    }

    void SpawnSingleEnemy(Vector3 initialSpawnPosition, List<Vector3> path)
    {
        if (enemyPrefabs == null)
        {
            Debug.LogError("������ �� �������� �����ؾ���");
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
            Debug.LogError("SpawnManager: ������ �� �����տ� EnemyMovement ������Ʈ�� �����ϴ�.");
        }
    }

    private void GameOver()
    {
        // 게임 끝나는 기준이 없음. 적 5명 지나가면 끝나는걸로 설정
        StopCoroutine(_spawnCoroutine);
    }

    private IEnumerator HandleBuffChoice()
    {
        Debug.Log($"웨이브 {currentWaveLevel + 1} 완료! 버프 선택을 시작합니다.");

        BuffData[] buffChoices = BuffManager.Instance.GetRandomBuffChoices();

        _isWaitingForBuffChoice = true;
        buffChoiceUI.ShowBuffChoices(buffChoices);

        while (_isWaitingForBuffChoice)
        {
            yield return null;
        }

        Debug.Log("버프 선택이 완료되었습니다. 게임을 계속 진행합니다.");
    }

    #region Acton Handler

    private void HandleReachedEndEnemy()
    {
        _reachedEndEnemyCount++;
        if (_reachedEndEnemyCount >= 5)
        {
            GameOver();
        }
    }

    private void HandleBuffChoiceCompleted()
    {
        _isWaitingForBuffChoice = false;
    }

    #endregion
}

[System.Serializable]
public class Wave
{
    public float spawnInterval;
    public float startTime;
    public float endTime;
}
