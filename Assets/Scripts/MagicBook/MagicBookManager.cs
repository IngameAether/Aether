using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagicBookManager : MonoBehaviour
{
    [SerializeField] private MagicBookData[] _allBooks;

    public static event Action<EBookEffectType, int> OnBookEffectApplied;

    private Dictionary<string, MagicBookData> _allBooksDict;
    private Dictionary<string, int> _ownedBooksDict;
    private List<MagicBookData> _availableBooks;

    private void Awake()
    {
        _allBooksDict = new Dictionary<string, MagicBookData>(_allBooks.Length);
        _ownedBooksDict = new Dictionary<string, int>(_allBooks.Length);
        _availableBooks = new List<MagicBookData>();

        foreach (var book in _allBooks) _allBooksDict[book.Code] = book;
    }

    /// <summary>
    /// 10 Wave마다 마법도서 랜덤으로 3개 꺼내기
    /// </summary>
    /// <param name="count">선택 가능한 마법도서 개수</param>
    /// <returns></returns>
    public MagicBookData[] GetRandomBookSelection(int count = 3)
    {
        UpdateAvailableBooks();

        var result = new List<MagicBookData>(count);
        var tempList = new List<MagicBookData>(_availableBooks);

        // Fisher-Yates 셔플
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, tempList.Count);
            result.Add(tempList[randomIndex]);
            (tempList[i], tempList[randomIndex]) = (tempList[randomIndex], tempList[i]);
        }

        return result.ToArray();
    }

    public void SelectBook(string bookCode)
    {
        if (!_allBooksDict.TryGetValue(bookCode, out var bookData)) return;

        int currentStack = _ownedBooksDict.GetValueOrDefault(bookCode, 0);
        if (currentStack < bookData.MaxStack)
        {
            _ownedBooksDict[bookCode] = currentStack + 1;
            ApplyBookEffect(bookData, currentStack + 1);
        }
    }

    /// <summary>
    /// 선택할 수 있는 마법도서 업데이트
    /// </summary>
    private void UpdateAvailableBooks()
    {
        _availableBooks.Clear();

        foreach (var book in _allBooks)
        {
            int currentStack = _ownedBooksDict.GetValueOrDefault(book.Code, 0);
            if (currentStack < book.MaxStack) _availableBooks.Add(book);
        }
    }

    private void ApplyBookEffect(MagicBookData bookData, int stack)
    {
        OnBookEffectApplied?.Invoke(bookData.EffectType, bookData.EffectValue[stack - 1]);
    }
}
