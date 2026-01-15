using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusEffect : MonoBehaviour
{
    [SerializeField] private List<GameObject> statusEffectIcon;
    private NormalEnemy enemy;

    private float[] statusEffectThreshold = new float[4];
    private float[] statusEffectGauge = { 0f, 0f, 0f, 0f };
    private float[] statusEffectTimer = { 0f, 0f, 0f, 0f };

    private float originalSpeed;
    private float slowSpeed;
    private float burnDamage;
    private float bleedDamage;
    private float bleedAdditionalGauge;

    private int burnTimerIndex = 0;
    private float[] burnTimer = { 2.01f, 1.51f, 1.01f, 0.51f };

    public void Initialize(EnemyInfoData enemyInfo)
    {
        enemy = GetComponent<NormalEnemy>();

        // 수치 받아오기
        statusEffectThreshold[0] = enemyInfo.SlowdownGauge;
        statusEffectThreshold[1] = enemyInfo.StunGauge;
        statusEffectThreshold[2] = enemyInfo.BurnGauge;
        statusEffectThreshold[3] = enemyInfo.BleedingGauge;

        originalSpeed = enemy.MoveSpeed;

        slowSpeed = StatusEffectController.Instance.GetSlowSpeed(originalSpeed);
        float stunThreshold = statusEffectThreshold[1];
        statusEffectThreshold[1] = StatusEffectController.Instance.GetStunthresholdCoef(stunThreshold);
        burnDamage = StatusEffectController.Instance.GetBurnDamage();
        bleedDamage = enemy.CurrentHealth * 0.1f;
        bleedAdditionalGauge = StatusEffectController.Instance.GetAdditionalBleedGauge();
    }

    private void Update()
    {
        // Slow (slow, stun이 둘 다 걸려 있으면 stun 우선)
        if (statusEffectTimer[0] > 0f)
        {
            enemy.MoveSpeed = slowSpeed;
            statusEffectTimer[0] -= Time.deltaTime;

            if (statusEffectTimer[0] <= 0f) statusEffectIcon[0].SetActive(false);
        }

        // Stun
        if (statusEffectTimer[1] > 0f)
        {
            enemy.MoveSpeed = 0f;
            statusEffectTimer[1] -= Time.deltaTime;

            if (statusEffectTimer[1] <= 0f) statusEffectIcon[1].SetActive(false);
        }

        // Burn
        if (statusEffectTimer[2] > 0f)
        {
            if (statusEffectTimer[2] < burnTimer[burnTimerIndex])
            {
                enemy.TakeDamage(burnDamage);
                if (++burnTimerIndex == 3) burnTimerIndex = 0;
            }
            statusEffectTimer[2] -= Time.deltaTime;

            if (statusEffectTimer[2] <= 0f) statusEffectIcon[2].SetActive(false);
        }

        // Bleed
        if (statusEffectTimer[3] > 0f)
        {
            statusEffectTimer[3] -= Time.deltaTime;

            if (statusEffectTimer[3] <= 0f)
            {
                enemy.TakeDamage(bleedDamage);
                statusEffectIcon[3].SetActive(false);
            }
        }
    }

    public void TakeStatusEffect(StatusEffectType statusEffect, float value)
    {
        int index = (int)statusEffect;
        if (statusEffectTimer[index] > 0f)
        {
            Debug.Log($"이미 {statusEffect} 상태이상 적용 중");
            return;
        }
        else
        {
            statusEffectGauge[index] += value;
            if (index == 3) statusEffectGauge[index] += bleedAdditionalGauge;

            if (statusEffectGauge[index] >= statusEffectThreshold[index])
            {
                statusEffectTimer[index] = (index == 1) ? 1f : 2f;
                statusEffectGauge[index] = 0f;
                statusEffectIcon[index].SetActive(true);
            }
        }
    }
}
