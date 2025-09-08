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

        // 설명 텍스트 생성 로직 
        if (bookData.Effects == null || bookData.Effects.Count == 0)
        {
            _bookDescriptionText.text = bookData.Description;
            return;
        }

        // 현재 스택(레벨)에 맞는 최종 효과 값을 가져옵니다.
        // EffectValuesByStack 리스트의 크기보다 높은 스택을 요구하는 경우를 방지합니다.
        int stackIndex = Mathf.Clamp(currentStack - 1, 0, bookData.EffectValuesByStack.Count - 1);
        int stackValue = bookData.EffectValuesByStack[stackIndex];

        // 최종 설명을 담을 변수
        string finalDescription = "";

        // 책이 가진 모든 효과(Effects)를 순회하며 설명을 만듭니다.
        foreach (var effect in bookData.Effects)
        {
            // 각 효과의 ValueType에 따라 표시할 텍스트를 다르게 만듭니다.
            string valueText;
            // effect.Value와 현재 스택의 값을 곱하여 최종 수치를 계산합니다. (예: 기본값 1 * 스택값 20 = 20)
            int currentEffectValue = effect.Value * stackValue;

            if (effect.ValueType == EValueType.Percentage)
            {
                valueText = $"+{currentEffectValue}%"; // Percentage 타입이면 '%'를 붙임
            }
            else // Flat 타입이면
            {
                valueText = $"+{currentEffectValue}"; // 숫자만 표시
            }

            // Description의 {0} 부분을 위에서 만든 텍스트로 교체하고, 최종 설명에 추가합니다.
            // (여러 효과가 있다면 줄바꿈(\n)으로 구분합니다.)
            finalDescription += string.Format(bookData.Description, valueText) + "\n";
        }

        _bookDescriptionText.text = finalDescription.TrimEnd(); // 마지막 줄바꿈 제거
    }
}
