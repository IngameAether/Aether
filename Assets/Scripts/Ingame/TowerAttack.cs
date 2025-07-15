using System.Collections;
using System.Collections.Generic;
using Towers.Core;
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
        GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);  // 투사체에 힘을 줘 적에게 날아가도록 함
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;  // 적이 아닌 대상과 충돌하는 경우
        if (collision.transform != currentTarget) return;   // 현재 target이 아닌 경우

        Debug.Log("투사체 맞음");
        damageable = currentTarget.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"{gameObject.name}이 {currentTarget.name}에게 {damage}의 피해를 입혔습니다");
        }
        Destroy(gameObject);
    }
}
