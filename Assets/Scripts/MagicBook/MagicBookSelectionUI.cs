using System;
using UnityEngine;

public class MagicBookSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _bookSelectionPanel;
    [SerializeField] private MagicBookButton[] _bookButtons; // 3개 버튼

    public event Action OnBookSelectCompleted;

    private MagicBookManager _bookManager;
    private MagicBookData[] _currentChoices;

    private void Start()
    {
        _bookManager = FindObjectOfType<MagicBookManager>();

        for (int i = 0; i < _bookButtons.Length; i++)
        {
            int index = i;
            _bookButtons[i].OnButtonClick += () => OnBookSelected(index);
        }
    }

    public void ShowBookSelection()
    {
        _currentChoices = _bookManager.GetRandomBookSelection(3);

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
        GameTimer.Instance.StopTimer();
    }

    private void OnBookSelected(int buttonIndex)
    {
        var bookData = _currentChoices[buttonIndex];
        _bookManager.SelectBook(bookData.Code);

        _bookSelectionPanel.SetActive(false);
        GameTimer.Instance.StartTimer();
        OnBookSelectCompleted?.Invoke();
    }
}
