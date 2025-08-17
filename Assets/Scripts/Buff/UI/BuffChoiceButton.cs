using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffChoiceButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _descText;

    private BuffData _buffData;
    private Action<BuffData> _onClick;

    private void Awake()
    {
        if (_button == null) _button = GetComponent<Button>();
    }

    // 초기화: 버튼에 데이터와 콜백을 연결
    public void Initialize(BuffData data, Action<BuffData> onClick)
    {
        _buffData = data;
        _onClick = onClick;
        _descText.text = data != null ? data.GetDescription() : string.Empty;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnButtonPressed);
        _button.interactable = data != null;
        gameObject.SetActive(data != null);
    }

    private void OnButtonPressed()
    {
        // 선택시 콜백 전달
        _onClick?.Invoke(_buffData);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonPressed);
    }

    // 외부에서 버튼 비활성화/활성화 가능
    public void SetInteractable(bool value)
    {
        if (_button != null) _button.interactable = value;
    }
}
