using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagicBookManager : MonoBehaviour
{
    public static MagicBookManager Instance { get; private set; }

    [SerializeField] private MagicBookData[] _allBooks;
    public event Action<EBookEffectType, int> OnBookEffectApplied;

    private Dictionary<string, MagicBookData> _allBooksDict;
    private Dictionary<string, int> _ownedBooksDict;
    private List<MagicBookData> _availableBooks;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

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

    // 어떤 마법책을 가지고 있는지 띄우는 메소드들
    // 1. 소유한 책의 정보를 담아 전달할 간단한 구조체 추가
    [System.Serializable]
    public struct OwnedBookInfo
    {
        public MagicBookData BookData; // 책의 모든 원본 데이터 (이름, 아이콘, 설명 등)
        public int CurrentStack;       // 현재 소유한 중첩(레벨)
    }

    // 2. 현재 소유한 모든 책의 정보를 List로 반환하는 public 메서드 추가
    public List<OwnedBookInfo> GetOwnedBookInfos()
    {
        var ownedBookInfos = new List<OwnedBookInfo>();

        // _ownedBooksDict에 있는 모든 책에 대해 반복
        foreach (var ownedBookPair in _ownedBooksDict)
        {
            string bookCode = ownedBookPair.Key;
            int currentStack = ownedBookPair.Value;

            // 책 코드에 해당하는 MagicBookData를 _allBooksDict에서 찾음
            if (_allBooksDict.TryGetValue(bookCode, out MagicBookData bookData))
            {
                // 새로운 OwnedBookInfo를 생성하여 리스트에 추가
                ownedBookInfos.Add(new OwnedBookInfo
                {
                    BookData = bookData,
                    CurrentStack = currentStack
                });
            }
        }

        return ownedBookInfos;
    }
}
