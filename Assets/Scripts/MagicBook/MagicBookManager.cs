using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BookRequestType { Regular, Specific }

public class MagicBookManager : MonoBehaviour
{
    public static MagicBookManager Instance { get; private set; }

    [SerializeField] private MagicBookData[] _allBooks;
    public event Action<EBookEffectType, int> OnBookEffectApplied;

    private Dictionary<string, MagicBookData> _allBooksDict;
    private Dictionary<string, int> _ownedBooksDict;
    private List<MagicBookData> _availableBooks;

    [Header("Combinations")]
    [SerializeField] private List<MagicBookCombinationSO> _combinations; // 모든 조합법 SO 리스트
    // 조합 완성 시 WaveManager에 "보상 줄 시간이야!" 라고 알리기 위한 이벤트
    public event Action<string> OnCombinationCompleted;

    private BookRequestType _nextRequestType = BookRequestType.Regular;
    private string _specificBookCode;

    /// 다음에 보여줄 매직북 선택의 종류를 미리 준비시킵니다. WaveManager가 호출합니다.
    public void PrepareSelection(BookRequestType type, string specificCode = null)
    {
        _nextRequestType = type;
        _specificBookCode = specificCode;
    }

    /// 준비된 요청에 따라 실제 매직북 데이터를 반환합니다. MagicBookSelectionUI가 호출합니다.
    public MagicBookData[] GetPreparedSelection()
    {
        if (_nextRequestType == BookRequestType.Specific)
        {
            return GetSpecificBookChoice(_specificBookCode);
        }
        else // BookRequestType.Regular
        {
            return GetRandomBookSelection(3);
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        _allBooksDict = new Dictionary<string, MagicBookData>(_allBooks.Length);
        _ownedBooksDict = new Dictionary<string, int>(_allBooks.Length);
        _availableBooks = new List<MagicBookData>();

        foreach (var book in _allBooks) _allBooksDict[book.Code] = book;

        // 게임 시작 시 모든 조합의 완료 상태를 리셋
        foreach (var combo in _combinations)
        {
            combo.Reset();
        }
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
            // 책을 선택할 때마다 조합 완성 여부를 체크하는 로직 호출
            CheckForCombinations();
        }
    }

    // 조합 완성 여부를 체크하는 메서드
    private void CheckForCombinations()
    {
        // 현재 내가 가진 모든 책 코드 목록 (탐색 속도를 위해 HashSet 사용)
        var ownedBookCodes = new HashSet<string>(_ownedBooksDict.Keys);

        foreach (var combo in _combinations)
        {
            // 아직 완성되지 않은 조합이고, 보상 책을 아직 받지 않은 경우에만 체크
            if (!combo.isCompleted && !_ownedBooksDict.ContainsKey(combo.rewardBookCode))
            {
                // 조합에 필요한 모든 책을 내가 가지고 있는지 확인
                if (ownedBookCodes.IsSupersetOf(combo.requiredBookCodes))
                {
                    Debug.Log($"조합 [{combo.name}] 완성! 보상: {combo.rewardBookCode}");
                    combo.isCompleted = true; // 이 조합은 이번 게임에서 다시 발동하지 않도록 처리

                    // WaveManager에 이벤트 발송!
                    OnCombinationCompleted?.Invoke(combo.rewardBookCode);
                }
            }
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

    /// 특정 책 코드 하나만 선택지로 반환합니다. (보스 보상, 조합 보상용)
    public MagicBookData[] GetSpecificBookChoice(string bookCode)
    {
        if (_allBooksDict.TryGetValue(bookCode, out var bookData))
        {
            // 책 데이터 하나만 들어있는 배열을 만들어 반환
            return new MagicBookData[] { bookData };
        }
        Debug.LogWarning($"요청한 책 코드 '{bookCode}'를 찾을 수 없습니다.");
        return null;
    }
}
