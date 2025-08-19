using TMPro;
using UnityEngine;

public class ElementUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text lightElementText;
    [SerializeField] private TMP_Text darkElementText;

    private void OnEnable()
    {
        // 이벤트 구독
        ResourceManager.OnElementChanged += UpdateUI;

        // 초기 값 갱신
        UpdateUI(ResourceManager.Instance.LightElement, ResourceManager.Instance.DarkElement);
    }

    private void OnDisable()
    {
        // 이벤트 해제 (메모리 누수 방지)
        ResourceManager.OnElementChanged -= UpdateUI;
    }

    private void UpdateUI(int light, int dark)
    {
        if (lightElementText != null)
            lightElementText.text = light.ToString();

        if (darkElementText != null)
            darkElementText.text = dark.ToString();
    }
}
