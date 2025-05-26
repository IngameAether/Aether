using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ����")]
    // ������ �� ������
    public GameObject enemyPrefab;
    public MapManage mapManage; // MapManage ��ũ��Ʈ ���۷���

    [Header("���� Ÿ�̹� ����")]
    // ���� ���۱��� ��� �ð�
    public float startDelay = 1f;
    // �� ���� ����
    public float spawnInterval = 0.5f;
    // ������ ���� �� ������
    public int numberOfEnemiesToSpawn = 20;
    // ������� ������ �� ��
    private int enemiesSpawned = 0;
    // ������ ���� ���� ������ Ȯ���ϴ� �÷���
    private bool isSpawning = false;

    private void Start()
    {
        // Inspector���� �����հ� MapManage ���۷����� ����Ǿ����� Ȯ��
        if (enemyPrefab == null)
        {
            Debug.LogError("SpawnManager: ������ �� �������� �������� �ʾҽ��ϴ�!");
        }
        if (mapManage == null)
        {
            Debug.LogError("SpawnManager: MapManage ���۷����� �������� �ʾҽ��ϴ�!");
        }
        Debug.Log("SpawnManager �ʱ�ȭ �Ϸ�. Start ��ư Ŭ�� ��� ��.");
    }

    // ClickBtn ��ũ��Ʈ�� Start ��ư Ŭ�� �̺�Ʈ�� ����� public �޼���
    public void StartSpawningFromButton()
    {
        if (enemyPrefab == null || mapManage == null)
        {
            Debug.LogError("���� ���� �Ұ�: �� ������ �Ǵ� MapManage ���۷����� �����Ǿ����ϴ�.");
            return;
        }

        if (isSpawning)
        {
            Debug.LogWarning("SpawnManager: �̹� ������ ���� ���Դϴ�.");
            return;
        }

        // ���� �ڷ�ƾ ����
        Debug.Log("Start ��ư Ŭ��! �� ���� ���� �ڷ�ƾ ����.");
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        isSpawning = true; // ���� ���� �÷��� ����
        enemiesSpawned = 0; // ������ �� �� �ʱ�ȭ

        // MapManage�κ��� ������ ��� �����͸� ������
        List<Vector3> path = mapManage.GetPathWorldPositions();
        // ���� ���� �ð� ���
        yield return new WaitForSeconds(startDelay);

        while (enemiesSpawned < numberOfEnemiesToSpawn)
        {
            // ����� ù ��° ������ ���� ��ġ�� ���
            Vector3 spawnPosition = path[0];

            // �� �ϳ� ���� �� ��� ����
            SpawnSingleEnemy(spawnPosition, path); // �Ʒ� �޼��� ȣ��
            enemiesSpawned++;   // ������ �� �� ����

            if (enemiesSpawned >= numberOfEnemiesToSpawn)
            {
                Debug.Log("��� �� ���� �Ϸ�");
                break; // ���� ����
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        isSpawning = false; // ���� ���� �÷��� ����
    }

    void SpawnSingleEnemy(Vector3 initialSpawnPosition, List<Vector3> path)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("������ �� �������� �����ؾ���");
            return;
        }

        // �� ������ �ν��Ͻ� ���� (������ ���� ��ġ�� ȸ�� ����)
        GameObject newEnemy = Instantiate(enemyPrefab, initialSpawnPosition, Quaternion.identity);

        // ������ �� ������Ʈ���� EnemyMovement ��ũ��Ʈ ��������
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            // EnemyMovement ��ũ��Ʈ�� SetInitialPosition �޼��� ȣ��
            enemyMovement.SetInitialPosition(initialSpawnPosition);
            // ������ ���(Vector3 ����Ʈ) ����
            enemyMovement.SetPath(path); 
        }
    }

}
