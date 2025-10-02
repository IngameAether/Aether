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

        if (_backgroundImage != null && data.Icon != null)
        {
            _backgroundImage.sprite = data.Icon;
        }

        if (_titleText != null)
            _titleText.text = data.Name;

        if (_rankText != null)
            _rankText.text = data.Rank.ToString();

        // data.Description 대신, data.GetFormattedDescription() 함수를 호출합니다.
        if (_descriptionText != null)
            _descriptionText.text = data.GetFormattedDescription(currentStack);

        if (_stackText != null)
            _stackText.text = $"Lv.{currentStack}";
    }
}
