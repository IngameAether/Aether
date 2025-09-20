using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _stackText; // 스택 레벨을 표시할 텍스트

    public void Setup(MagicBookData data, int currentStack)
    {
        if (data == null)
        {
            Debug.LogError("Setup에 전달된 MagicBookData가 null입니다.");
            return;
        }

        // 배경 이미지를 책 데이터의 아이콘으로 설정
        if (_backgroundImage != null && data.Icon != null)
        {
            _backgroundImage.sprite = data.Icon;
        }

        // 각 텍스트 컴포넌트에 내용 할당
        if (_titleText != null) _titleText.text = data.Name;
        if (_rankText != null) _rankText.text = data.Rank.ToString();
        if (_descriptionText != null) _descriptionText.text = data.Description; // 기본 설명 표시
        if (_stackText != null) _stackText.text = $"Lv.{currentStack}";
    }
}
