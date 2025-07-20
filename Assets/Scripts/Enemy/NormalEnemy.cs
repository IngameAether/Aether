using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI ���� ����� ���� �߰�

public class NormalEnemy : MonoBehaviour, IDamageable
{
    // ���� �ִ� ü��
    public float maxHealth = 10f;
    // ���� ü��
    private float currentHealth;
    public float CurrentHealth => currentHealth;

    // �̵� �ӵ�
    public float moveSpeed = 2f;
    
    // ���� ���׷� (Magic Resistance)
    [Range(0, 50)] // Inspector â���� 5~50 ������ ���� �Է� �����ϵ��� ����
    public int magicResistance = 5; // �⺻ ���� ���׷��� 5%�� ����

    // ���ŷ� (Mental Strength, ������ ȿ��)
    [Range(0, 100)] // Inspector â���� 10~100 ������ ���� �Է� �����ϵ��� ����
    public int mentalStrength = 10; // �⺻ ���ŷ��� 10%�� ����

    // ü�� �� �̹����� ������ ����
    public Image healthBarFillImage; // ü�� ���� Fill Ÿ�� �̹���

    // EnemyMovement ������Ʈ ����
    private EnemyMovement enemyMovement; 

    void Start()
    {
        // ���� ���� �� ���� ü���� �ִ� ü������ ����
        currentHealth = maxHealth;
        // ü�� �� �ʱ�ȭ
        UpdateHealthBar();

        // EnemyMovement ������Ʈ ����
        enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            Debug.LogError("EnemyMovement ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }


    // ���� ���ظ� �Ծ��� �� ȣ��� �Լ�
    public void TakeDamage(float damageAmount)
    {
        // �⺻ ������ ���
        float modifiedDamage = damageAmount;

        // Ư�� �ɷ� ���� (��: ���� ���׷�)
        //modifiedDamage = CalculateDamageAfterResistance(modifiedDamage);

        // ���� ������ ����
        currentHealth -= modifiedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // ü�� �� ������Ʈ
        UpdateHealthBar();

        // ü���� 0 ���ϰ� �Ǹ� �� ���� �� ó��
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ���� ���׷¿� ���� ������ ��� �Լ�
    private float CalculateDamageAfterResistance(float damageAmount)
    {
        // ���� ���׷� %�� �������� ������ ������ ���
        float resistanceRatio = magicResistance / 100f;
        // ���� ������ ��� (������ * (1 - ���׷� ����))
        float finalDamage = damageAmount * (1 - resistanceRatio);
        return finalDamage;
    }

    // ���ŷ¿� ���� CC ���� �ð� ���� ��� �Լ�
    public float CalculateReducedCCDuration(float baseDuration)
    {
        // ���ŷ� %�� �������� ���� �ð� ������ ���
        float tenacityRatio = mentalStrength / 100f;
        // ���� CC ���� �ð� ��� (�⺻ ���� �ð� * (1 - ������))
        float finalDuration = baseDuration * (1 - tenacityRatio);
        return finalDuration;
    }

    // ü�� �� UI�� ������Ʈ�ϴ� �Լ�
    void UpdateHealthBar()
    {
        if (healthBarFillImage != null)
        {
            // ���� ü�� ������ ���� ü�� �� �̹����� Fill Amount�� ����
            healthBarFillImage.fillAmount = currentHealth / maxHealth;
        }
    }

    // ���� �׾��� �� ȣ��� �Լ�
    void Die()
    {
        Debug.Log(gameObject.name + "�� �׾����ϴ�.");
        // TODO: ���⿡ ���� �׾��� �� �߻��� �̺�Ʈ (�ִϸ��̼� ���, ������ ���, ������Ʈ ���� ��)�� �߰�
        Destroy(gameObject); // ����: �� ������Ʈ ����
    }

    // �̵� �ӵ� ���� �Լ� (Ư�� �ɷ� ����)
    public void ApplyMoveSpeedModifier(float modifier)
    {
        moveSpeed += modifier;
        //moveSpeed = Mathf.Clamp(moveSpeed, 0, maxMoveSpeed); // �ִ� �̵� �ӵ� ����
    }
}