using System;
using UnityEngine;

public class MagicBookSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _bookSelectionPanel;
    [SerializeField] private MagicBookButton[] _bookButtons; // 3개 버튼

    public event Action OnBookSelectCompleted;
    private MagicBookData[] _currentChoices;

    private void Start()
    {
        for (int i = 0; i < _bookButtons.Length; i++)
        {
            int index = i;
            _bookButtons[i].OnButtonClick += () => OnBookSelected(index);
        }
    }

    public void ShowBookSelection()
    {
        _currentChoices = MagicBookManager.Instance.GetRandomBookSelection(3);
        if (_currentChoices == null || _currentChoices.Length == 0)
        {
            Debug.LogError("MagicBookSelectionUI: 추천 목록이 비어 있습니다.");
            return;
        }

        if (_bookButtons == null || _bookButtons.Length == 0)
        {
            Debug.LogError("MagicBookSelectionUI: 버튼 배열이 비어 있습니다.");
            return;
        }

        // UI 업데이트
        for (int i = 0; i < _bookButtons.Length; i++)
        {
            if (i < _currentChoices.Length)
            {
                _bookButtons[i].SetBookData(_currentChoices[i].Description);
                _bookButtons[i].gameObject.SetActive(true);
            }
            else
            {
                _bookButtons[i].gameObject.SetActive(false);
            }
        }

        _bookSelectionPanel.SetActive(true);
        //Time.timeScale = 0f;
        GameTimer.Instance.StopTimer();
    }

    private void OnBookSelected(int buttonIndex)
    {
        var bookData = _currentChoices[buttonIndex];
        MagicBookManager.Instance.SelectBook(bookData.Code);

        _bookSelectionPanel.SetActive(false);

        //Time.timeScale = 1f;
        GameTimer.Instance.StartTimer();
        OnBookSelectCompleted?.Invoke();
    }
}
