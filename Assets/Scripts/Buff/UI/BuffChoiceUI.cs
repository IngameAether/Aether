using UnityEngine;

public class BuffChoiceUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject _buffChoicePanel;
    [SerializeField] private BuffChoiceButton[] _choiceButtons;

    public event System.Action OnBuffChoiceCompleted;

    private BuffData[] _currentChoices;
    private bool _isSelecting = false;

    private void Start()
    {
        // 초기 상태: 버튼은 비활성 또는 빈 상태로 둠
        if (_choiceButtons == null) return;
        foreach (var btn in _choiceButtons)
        {
            btn.Initialize(null, OnBuffSelected); // 초기화로 안전하게 설정
        }
        _buffChoicePanel.SetActive(false);
    }

    /// <summary>
    /// 버프 선택창 보이기
    /// </summary>
    /// <param name="choices"></param>
    public void ShowBuffChoices(BuffData[] choices)
    {
        if (choices == null || choices.Length == 0)
        {
            Debug.LogWarning("ShowBuffChoices called with null/empty choices.");
            return;
        }

        _currentChoices = choices;
        _isSelecting = true;

        // 버튼들에 분배
        for (int i = 0; i < _choiceButtons.Length; i++)
        {
            if (i < choices.Length && choices[i] != null)
            {
                _choiceButtons[i].Initialize(choices[i], OnBuffSelected);
            }
            else
            {
                _choiceButtons[i].Initialize(null, null);
            }
        }

        _buffChoicePanel.SetActive(true);

        if (GameTimer.Instance != null) GameTimer.Instance.StopTimer();
        else Debug.LogWarning("GameTimer.Instance is null in BuffChoiceUI.ShowBuffChoices()");
    }

    // 버튼에서 선택되면 이 함수가 호출된다.
    private void OnBuffSelected(BuffData chosen)
    {
        if (!_isSelecting) return; // 이미 처리된 경우 방지
        _isSelecting = false;

        // 버튼들을 비활성화하여 중복 클릭 방지
        foreach (var btn in _choiceButtons) btn.SetInteractable(false);

        if (chosen == null)
        {
            Debug.LogWarning("OnBuffSelected called with null chosen.");
            CloseAndResume();
            return;
        }

        if (BuffManager.Instance == null)
        {
            Debug.LogError("BuffManager.Instance is null. Cannot apply buff.");
            CloseAndResume();
            return;
        }

        // 선택 적용
        BuffManager.Instance.ApplyBuff(chosen);

        // 모든 버프 객체 반환(풀에 돌려주기)
        if (_currentChoices != null)
        {
            foreach (var t in _currentChoices)
            {
                if (t != null)
                {
                    BuffManager.Instance.ReturnBuffToPool(t);
                }
            }
        }

        CloseAndResume();
        OnBuffChoiceCompleted?.Invoke();
    }

    private void CloseAndResume()
    {
        _buffChoicePanel.SetActive(false);
        if (GameTimer.Instance != null) GameTimer.Instance.StartTimer();
    }
}
