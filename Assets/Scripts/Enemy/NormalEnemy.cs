using UnityEngine;
using UnityEngine.UI;

public class NormalEnemy : MonoBehaviour
{
    // ���� �ִ� ü��
    public float maxHealth = 100f;
    // ���� ü��
    private float currentHealth;

    // �̵� �ӵ�
    public float moveSpeed = 5f;
    // ����
    public float defense = 10f;

    // ü�� �� �̹����� ������ ����
    public Image healthBarImage; // ü�� ���� �̹���

    void Start()
    {
        // ���� ���� �� ���� ü���� �ִ� ü������ ����
        currentHealth = maxHealth;
        // ü�� �� �ʱ�ȭ
        UpdateHealthBar();
    }

    void Update()
    {
        
        // ����: ���� ������ ��� �̵��Ѵٰ� ����
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // �������� �߻������� �ֿܼ� ��� (�ִϸ��̼� Ȯ�ο� �����)
        Debug.Log("�� �̵� ��...");

        // TODO: ���⿡ ���� �̵�, ����, ���� ��ȭ �� ���� ���� ������ �߰�
        // ����: �÷��̾ �����ϴ� ���� ��
    }

    // ���� ���ظ� �Ծ��� �� ȣ��� �Լ�
    public void TakeDamage(float damageAmount)
    {
        // ������ ������ ���� ���ط� ���
        float actualDamage = Mathf.Max(0, damageAmount - defense);
        // ���� ü�¿��� ���ط���ŭ ����
        currentHealth -= actualDamage;
        // ü���� 0���� �۾����� �ʵ��� clamped
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // ü�� �� ������Ʈ
        UpdateHealthBar();

        // ü���� 0 ���ϰ� �Ǹ� �� ���� �� ó��
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ü�� �� UI�� ������Ʈ�ϴ� �Լ�
    void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            // ���� ü�� ������ ���� ü�� �� �̹����� Fill Amount�� ����
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
    }

    // ���� �׾��� �� ȣ��� �Լ�
    void Die()
    {
        Debug.Log(gameObject.name + "�� �׾����ϴ�.");
        // TODO: ���⿡ ���� �׾��� �� �߻��� �̺�Ʈ (�ִϸ��̼� ���, ������ ���, ������Ʈ ���� ��)�� �߰�
        Destroy(gameObject); // ����: �� ������Ʈ ����
    }
}