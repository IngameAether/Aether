using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 확인 팝업 UI (예/아니오 선택)
/// </summary>
public class ConfirmPopUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private TMP_Text yesButtonText;
    [SerializeField] private TMP_Text noButtonText;

    private Action _onYesCallback;
    private Action _onNoCallback;

    /// <summary>
    /// 확인 팝업 초기화
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="onYes">"예" 버튼 클릭 콜백</param>
    /// <param name="onNo">"아니오" 버튼 클릭 콜백</param>
    public void Initialize(string message, Action onYes, Action onNo)
    {
        if (messageText != null)
            messageText.text = message;

        _onYesCallback = onYes;
        _onNoCallback = onNo;

        // 버튼 이벤트 등록
        yesButton?.onClick.AddListener(OnYesButtonClick);
        noButton?.onClick.AddListener(OnNoButtonClick);

        if (yesButtonText != null)
            yesButtonText.text = "예";
        if (noButtonText != null)
            noButtonText.text = "아니오";
    }

    private void OnYesButtonClick()
    {
        _onYesCallback?.Invoke();
    }

    private void OnNoButtonClick()
    {
        _onNoCallback?.Invoke();
    }

    private void OnDestroy()
    {
        yesButton?.onClick.RemoveListener(OnYesButtonClick);
        noButton?.onClick.RemoveListener(OnNoButtonClick);
    }
}
