using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicBookButton : MonoBehaviour
{
    public event Action OnButtonClick;

    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _descText;  // 타이틀 세부 설명

    private MagicBookSelectionUI _selectionUI;
    private int _buttonIndex;

    // 버튼을 초기화하는 함수
    public void Initialize(MagicBookSelectionUI selectionUI, int index)
    {
        _selectionUI = selectionUI;
        _buttonIndex = index;
    }

    private void Awake()
    {
        if (_button == null) TryGetComponent(out _button);
        _button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(HandleClick);
    }

    private void OnEnable()
    {
        if (_button != null) _button.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        if (_button != null) _button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        _selectionUI.OnBookSelected(_buttonIndex);
    }

    // 기존 코드와 호환(설명만 세팅)
    public void SetBookData(string description)
    {
        if (_descText != null)
            _descText.text = description;
    }
}
