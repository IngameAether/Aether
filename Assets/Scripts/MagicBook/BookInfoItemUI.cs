using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookInfoItemUI : MonoBehaviour
{
    [SerializeField] private Image _bookIconImage;
    [SerializeField] private TextMeshProUGUI _bookNameText;
    [SerializeField] private TextMeshProUGUI _bookRankText;
    [SerializeField] private TextMeshProUGUI _bookDescriptionText;

    public void Setup(MagicBookData bookData, int currentStack)
    {
        if (bookData.Icon != null) _bookIconImage.sprite = bookData.Icon;
        _bookNameText.text = bookData.Name;
        _bookRankText.text = bookData.Rank.ToString();

        // 포맷팅 함수를 호출합니다.
        _bookDescriptionText.text = bookData.GetFormattedDescription(currentStack);
    }
}
