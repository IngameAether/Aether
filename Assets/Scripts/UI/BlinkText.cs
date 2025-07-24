using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public float blinkInterval = 0.5f;
    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if(textMeshPro == null)
        {
            enabled = false;
            return;
        }
        StartBlinking();
    }

    public void StartBlinking()
    {
        StopAllCoroutines();
        StartCoroutine(BlinkCoroutine());
    }

    public void StopBlinking()
    {
        StopAllCoroutines();
        if(textMeshPro != null)
        {
            textMeshPro.enabled = true;
        }
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            textMeshPro.enabled = !textMeshPro.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}