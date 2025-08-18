using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicBookButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _descText;

    private void Awake()
    {
        if (_button == null) _button = GetComponent<Button>();
    }

    public void SetBookData(string desc)
    {
        _descText.text = desc;
    }
}
