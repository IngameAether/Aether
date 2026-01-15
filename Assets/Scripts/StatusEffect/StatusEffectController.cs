using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    public static StatusEffectController Instance { get; private set; }

    private void Awake()
    {
        // 인스턴스가 이미 있는지 확인해서 중복 피함
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private float slowCoef;
    private float stunCoef;
    private float burnCoef;
    private float bleedCoef;

    private void Start()
    {
        // 마법도서를 획득한 내용이 있다면 저장된 걸로 수정되어야 함
        slowCoef = 0.5f;
        stunCoef = 1f;
        burnCoef = 1f;
        bleedCoef = 0f;
    }

    public void SetSlowCoef(float coef) => slowCoef = coef;
    public void SetStunCoef(float coef) => stunCoef = coef;
    public void SetBurnCoef(float coef) => burnCoef = coef;
    public void SetBleedCoef(float coef) => bleedCoef = coef;

    public float GetSlowSpeed(float speed)
    {
        return speed * slowCoef;
    }

    public float GetStunthresholdCoef(float threshold)
    {
        return threshold * stunCoef;
    }

    public float GetBurnDamage()
    {
        return (float)(GameManager.Instance.CurrentWave * 10) * burnCoef;
    }

    public float GetAdditionalBleedGauge()
    {
        return bleedCoef;
    }
}
