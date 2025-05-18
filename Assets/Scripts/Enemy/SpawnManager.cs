using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ��ġ ����")]
    // ������ �� ������
    public GameObject enemyPrefab;
    // ���� ó�� ������ ��ġ (SetInitialPosition �޼ҵ忡�� ���)
    public Transform spawnPoint;
    // ���� ���� �̵��� ��� (watpoints ���)
    public List<Vector3> enemyPath;

    [Header("���� Ÿ�̹� ����")]
    // ���� ���۱��� ��� �ð�
    public float startDelay = 1f;
    // �� ���� ����
    public float spawnInterval = 1f;
    // ������ ���� �� ������ = 3;
    public int numberOfEnemiesToSpawn = 3;
    // ������� ������ �� ��
    private int enemiesSpawned = 0;

    private void Start()
    {
        // �ڷ�ƾ�� ����Ͽ� ���� �������� �� ���� ����
        StartCoroutine(SpawnEnmiesRoutine());
    }

    IEnumerator SpawnEnmiesRoutine()
    {
        // ���� ���� �ð� ���
        yield return new WaitForSeconds(startDelay);

        while (enemiesSpawned < numberOfEnemiesToSpawn)
        {
            SpawnEnemy();
            enemiesSpawned++;
            // ���� �������� ���
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab = null)
        {
            Debug.Log("������ �� �������� �����ؾ���");
            return;
        }

        // �� ������ �ν��Ͻ� ����
        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        // EnemyMovement ��ũ��Ʈ ��������
        EnemyMovement enemyMovement = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            // �ʱ� ��ġ ���� (SpawnManager�� ��ġ or ���� ���� ����)
            // �޼ҵ带 ���� ����
            if (spawnPoint != null){
                enemyMovement.SetInitialPosition(spawnPoint.position);
            }
            else 
            {
                // SpawnManager�� ��ġ�� �ʱ� ��ġ�� ����
                enemyMovement.SetInitialPosition(transform.position);
            }

            // �̵� ��� ����
            if (enemyPath != null && enemyPath.Count > 0)
            {
                enemyMovement.SetPath(enemyPath);
            }
            else
            {
                // ��ΰ� �������� �ʾ��� ��� EnemyMovement ��ũ��Ʈ�� �⺻��η� ����
                Debug.LogWarning(" �� �̵� ��ΰ� �������� �ʾ���. EnemyMovement�� �⺻��η� ����");
                // �Ǵ� �̵����� �ʵ��� ������ �� ����
                // newEnemy.GetComponent<EnemyMovement>().enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("�� �����տ� EnemyMovement ��ũ��Ʈ�� ����");
        }
    }
}
