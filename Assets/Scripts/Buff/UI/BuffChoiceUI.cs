using UnityEngine;

public class BuffChoiceUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject _buffChoicePanel;
    [SerializeField] private BuffChoiceButton[] _choiceButtons;

    private BuffData[] _currentChoices;

    private void Start()
    {
        _buffChoicePanel.SetActive(false);

        for (int i = 0; i < _choiceButtons.Length; i++)
        {
            var index = i;
            _choiceButtons[i].OnButtonClick += () => SelectBuff(index);
        }
    }

    /// <summary>
    /// 버프 선택창 보이기
    /// </summary>
    /// <param name="choices"></param>
    public void ShowBuffChoices(BuffData[] choices)
    {
        _currentChoices = choices;
        for (int i = 0; i < _choiceButtons.Length; i++)
        {
            if (i < choices.Length && choices[i] != null)
            {
                _choiceButtons[i].SetBuffData(choices[i]);
                _choiceButtons[i].gameObject.SetActive(true);
            }
            else
            {
                _choiceButtons[i].gameObject.SetActive(false);
            }
        }

        _buffChoicePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SelectBuff(int index)
    {
        if (_currentChoices != null && index < _currentChoices.Length && _currentChoices[index] != null)
        {
            BuffManager.Instance.ApplyBuff(_currentChoices[index]);

            foreach (var t in _currentChoices)
            {
                if (t != null)
                {
                    BuffManager.Instance.ReturnBuffToPool(t);
                }
            }

            _buffChoicePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
