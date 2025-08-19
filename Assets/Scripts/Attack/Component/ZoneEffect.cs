using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneEffect : MonoBehaviour
{
    public float duration = 5f;
    public float tickInterval = 1f;
    public float radius = 3f;
    public float damagePerTick = 2f;

    private float _timer;

    void Start() => Destroy(gameObject, duration);

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= tickInterval)
        {
            _timer = 0f;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                    Debug.Log("장판 데미지 적용");
            }
        }
    }
}
