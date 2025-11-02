using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicBookButton : MonoBehaviour
{
    // --- 기존 변수들은 그대로 ---
    [Header("UI Components")]
    [SerializeField] private Button _button;
    private MagicBookSelectionUI _selectionUI;
    private int _buttonIndex;

    // 텍스트 UI 참조 변수 변경
    [Header("Text Components")]
    [SerializeField] private TMP_Text _title;      // 책 제목
    [SerializeField] private TMP_Text _rank;       // 책 등급 (Normal 등)
    [SerializeField] private TMP_Text _description; // 책 세부 설명

    [Header("Layout Refs")]
    [SerializeField] private RectTransform _row;
    [SerializeField] private RectTransform _leftColumn;
    [SerializeField] private RectTransform _rightColumn;

    [Header("Sizing")]
    [SerializeField] private float _titleAreaWidth = 50f;
    [SerializeField] private float _rightMinWidth = 40f;

    // 버튼을 초기화하는 함수
    public void Initialize(MagicBookSelectionUI selectionUI, int index)
    {
        _selectionUI = selectionUI;
        _buttonIndex = index;
    }

    private void Awake()
    {
        if (_button == null) _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
        ConfigureHorizontalOnce();
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        _selectionUI.OnBookSelected(_buttonIndex);
    }

    private void ConfigureHorizontalOnce()
    {
        if (_row != null)
        {
            var h = _row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 12f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = false;
        }

        if (_leftColumn != null)
        {
            var leLeft = _leftColumn.GetComponent<LayoutElement>();
            leLeft.preferredWidth = _titleAreaWidth;
            leLeft.flexibleWidth = 0;
        }

        if (_rightColumn != null)
        {
            var leRight = _rightColumn.GetComponent<LayoutElement>();
            leRight.minWidth = _rightMinWidth;
            leRight.flexibleWidth = 1;
        }
    }

    // SetBookData 함수를 새로운 구조에 맞게 변경
    public void SetBookData(Sprite bookSprite, string title, string rank, string description)
    {
        // 전달받은 bookSprite가 null이 아닐 경우에만 이미지를 변경합니다.
        if (_button != null && bookSprite != null)
        {
            _button.GetComponent<Image>().sprite = bookSprite;
        }

        if (_title != null)
        {
            _title.enableWordWrapping = true;
            _title.enableAutoSizing = false;
            _title.text = title;

            var le = _title.GetComponent<LayoutElement>();
            le.preferredWidth = _titleAreaWidth;

            if (_leftColumn != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_leftColumn);

            Vector2 pref = _title.GetPreferredValues(title, _titleAreaWidth, 0f);
            var rt = _title.rectTransform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _titleAreaWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pref.y);
            _title.ForceMeshUpdate();
        }

        if (_rank != null) _rank.text = rank;
        if (_description != null)
        {
            _description.enableWordWrapping = true;
            _description.enableAutoSizing = false;
            _description.text = description;
        }

        if (_row != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_row);
    }
}
