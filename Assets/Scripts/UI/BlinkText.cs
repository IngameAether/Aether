using System.Collections;
using UnityEngine;
using TMPro;

public class FadeBlinkText : MonoBehaviour
{
    [Tooltip("한 번 페이드인 또는 페이드아웃 되는 데 걸리는 시간")]
    public float fadeDuration = 0.5f; // 페이드인 또는 페이드아웃에 걸리는 시간

    [Tooltip("글씨가 완전히 사라진(알파 0) 상태로 머무는 시간")]
    public float stayHiddenDuration = 0f; // 완전히 사라진 상태로 유지되는 시간

    [Tooltip("글씨가 완전히 나타난(알파 1) 상태로 머무는 시간")]
    public float stayVisibleDuration = 0f; // 완전히 나타난 상태로 유지되는 시간

    private TextMeshProUGUI textMeshPro;
    private Color originalColor; // 원래 텍스트 색상을 저장합니다.

    private Coroutine currentBlinkRoutine; // 현재 실행 중인 코루틴을 저장하여 중지할 때 사용합니다.

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogWarning("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. 이 스크립트는 비활성화됩니다.", this);
            enabled = false; // 컴포넌트가 없으면 스크립트를 비활성화
            return;
        }
        originalColor = textMeshPro.color; // 텍스트의 원래 색상 (알파값 포함)을 저장합니다.

        // 게임 시작 시 바로 깜빡임을 시작합니다.
        StartFadingBlink();
    }

    public void StartFadingBlink() // 투명해지기 시작
    {
        StopAllCoroutines(); 
        // 새로운 페이드 깜빡임 코루틴 시작
        currentBlinkRoutine = StartCoroutine(FadeBlinkCoroutine());
    }

    public void StopFadingBlink() // 다시 선명해지기 시작
    {
        if (currentBlinkRoutine != null)
        {
            StopCoroutine(currentBlinkRoutine);
            currentBlinkRoutine = null;
        }
        if (textMeshPro != null)
        {
            textMeshPro.color = originalColor; // 원래 색상으로 되돌려 완전히 보이게 합니다.
        }
    }

    private IEnumerator FadeBlinkCoroutine()
    {
        while (true)
        {
            // 1. 페이드 아웃 (완전히 보임 -> 완전히 사라짐)
            yield return StartCoroutine(FadeTextToAlpha(originalColor.a, 0f, fadeDuration));

            // 완전히 사라진 상태로 유지
            if (stayHiddenDuration > 0)
            {
                yield return new WaitForSeconds(stayHiddenDuration);
            }

            // 2. 페이드 인 (완전히 사라짐 -> 완전히 보임)
            yield return StartCoroutine(FadeTextToAlpha(0f, originalColor.a, fadeDuration));

            // 완전히 보이는 상태로 유지
            if (stayVisibleDuration > 0)
            {
                yield return new WaitForSeconds(stayVisibleDuration);
            }
        }
    }

    private IEnumerator FadeTextToAlpha(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color currentColor = textMeshPro.color; // 현재 텍스트 색상을 가져옵니다.

        while (timer < duration)
        {
            timer += Time.deltaTime; // 경과 시간 계산
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration); // Lerp로 알파 값 보간
            currentColor.a = alpha; // 현재 색상의 알파 값만 변경
            textMeshPro.color = currentColor; // 변경된 색상 적용
            yield return null; // 다음 프레임까지 대기
        }
        // 정확한 종료 알파 값으로 설정하여 혹시 모를 오차 방지
        currentColor.a = endAlpha;
        textMeshPro.color = currentColor;
    }
}
