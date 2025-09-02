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

        // 1. 현재 레벨에 맞는 효과 값을 가져옵니다.
        int currentEffectValue = bookData.EffectValue[currentStack - 1];

        // 2. ValueType에 따라 표시할 텍스트를 다르게 만듭니다.
        string valueText;
        if (bookData.ValueType == EValueType.Percentage)
        {
            valueText = $"+{currentEffectValue}%"; // Percentage 타입이면 '%'를 붙임
        }
        else // Flat 타입이면
        {
            valueText = $"+{currentEffectValue}"; // 숫자만 표시
        }

        // 3. Description의 {0} 부분을 위에서 만든 텍스트로 교체합니다.
        // (Description 예시: "모든 타워의 공격 속도가 {0} 증가합니다.")
        string formattedDescription = string.Format(bookData.Description, valueText);

        _bookDescriptionText.text = formattedDescription;
    }
}
