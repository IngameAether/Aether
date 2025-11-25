using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitFlicking : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Color originColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originColor = spriteRenderer.color;
    }

    public void HitFlicking()
    {
        StopAllCoroutines();
        StartCoroutine(FlickingRoutine());
    }

    IEnumerator FlickingRoutine()
    {
        float interval = 0.07f;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(interval);
        spriteRenderer.color = originColor;
    }
}
