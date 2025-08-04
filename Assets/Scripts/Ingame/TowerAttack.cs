using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    IDamageable damageable;
    private float damage;
    private Transform currentTarget;

    public void Initialize(float damage, Transform currentTarget, Vector3 direction, float projectileSpeed)
    {
        this.damage = damage;
        this.currentTarget = currentTarget;
        GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);  // ����ü�� ���� �� ������ ���ư����� ��
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;  // ���� �ƴ� ���� �浹�ϴ� ���
        if (collision.transform != currentTarget) return;   // ���� target�� �ƴ� ���

        Debug.Log("����ü ����");
        damageable = currentTarget.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"{gameObject.name}�� {currentTarget.name}���� {damage}�� ���ظ� �������ϴ�");
        }
        Destroy(gameObject);
    }
}
