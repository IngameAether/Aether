using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicBookButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _descText;

    public event Action OnButtonClick;

    private void Awake()
    {
        _button.onClick.AddListener(() => OnButtonClick?.Invoke());
    }

    public void SetBookData(string desc)
    {
        _descText.text = desc;
    }
}
