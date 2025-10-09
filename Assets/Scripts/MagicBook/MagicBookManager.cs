using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public enum BookRequestType { Regular, Specific }

public class MagicBookManager : MonoBehaviour
{
    public static MagicBookManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private MagicBookData[] _allBooks;
    [SerializeField] private MagicBookRarityTable rarityTable; // [연결] 확률표 데이터를 연결할 변수

    public event Action<BookEffect, float> OnBookEffectApplied;

    // 내부 데이터 관리
    private Dictionary<string, MagicBookData> _allBooksDict;
    private Dictionary<string, int> _ownedBooksDict;

    // 등급별로 미리 분류해서 저장할 리스트
    private List<MagicBookData> _normalBooks;
    private List<MagicBookData> _rareBooks;
    private List<MagicBookData> _epicBooks;

    private BookRequestType _nextRequestType = BookRequestType.Regular;
    private string _specificBookCode;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        _allBooksDict = new Dictionary<string, MagicBookData>(_allBooks.Length);
        foreach (var book in _allBooks) _allBooksDict[book.Code] = book;

        _ownedBooksDict = new Dictionary<string, int>(_allBooks.Length);

        // 책을 등급별로 분류하는 로직
        _normalBooks = new List<MagicBookData>();
        _rareBooks = new List<MagicBookData>();
        _epicBooks = new List<MagicBookData>();

        foreach (var book in _allBooks)
        {
            switch (book.Rank)
            {
                case EBookRank.Normal: _normalBooks.Add(book); break;
                case EBookRank.Rare: _rareBooks.Add(book); break;
                case EBookRank.Epic: _epicBooks.Add(book); break;
                // Special 등급은 확률 뽑기에서 제외
            }
        }
    }

    /// <summary>
    /// 다음에 보여줄 매직북 선택의 종류를 미리 준비시킵니다. WaveManager가 호출합니다.
    /// </summary>
    public void PrepareSelection(BookRequestType type, string specificCode = null)
    {
        _nextRequestType = type;
        _specificBookCode = specificCode;
    }

    /// <summary>
    /// 준비된 요청에 따라 실제 매직북 데이터를 반환합니다. MagicBookSelectionUI가 호출합니다.
    /// </summary>
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

    /// <summary>
    /// [교체] 웨이브별 확률에 따라 랜덤 마법책 3개를 선택하는 새로운 함수
    /// </summary>
    public MagicBookData[] GetRandomBookSelection(int count = 3)
    {
        // 현재 웨이브에 맞는 확률 설정 찾기
        int currentWave = (GameManager.Instance != null) ? GameManager.Instance.CurrentWave : 0;
        RarityByWave currentRarity = rarityTable.raritySettings[0]; // 기본값은 첫 번째 설정
        foreach (var setting in rarityTable.raritySettings)
        {
            if (currentWave >= setting.waveThreshold)
            {
                currentRarity = setting;
            }
        }

        // 위 확률에 따라 3개의 등급을 먼저 뽑음
        List<EBookRank> ranksToPick = new List<EBookRank>();
        for (int i = 0; i < count; i++)
        {
            ranksToPick.Add(GetRandomRank(currentRarity));
        }

        // 뽑힌 등급에 맞춰 실제 책을 선택
        List<MagicBookData> finalChoices = new List<MagicBookData>();
        foreach (var rank in ranksToPick)
        {
            List<MagicBookData> sourceList = null;
            switch (rank)
            {
                case EBookRank.Normal: sourceList = _normalBooks; break;
                case EBookRank.Rare: sourceList = _rareBooks; break;
                case EBookRank.Epic: sourceList = _epicBooks; break;
            }

            if (sourceList != null && sourceList.Count > 0)
            {
                // 아직 선택되지 않았고, 최대 스택이 아닌 책을 찾아서 추가
                var availableBooksInRank = sourceList.Where(book =>
                    !finalChoices.Contains(book) &&
                    _ownedBooksDict.GetValueOrDefault(book.Code, 0) < book.MaxStack
                ).ToList();

                if (availableBooksInRank.Count > 0)
                {
                    finalChoices.Add(availableBooksInRank[Random.Range(0, availableBooksInRank.Count)]);
                }
            }
        }

        return finalChoices.ToArray();
    }

    /// <summary>
    /// [신규] 확률 설정에 따라 가중치 랜덤으로 등급 하나를 뽑는 헬퍼 함수
    /// </summary>
    private EBookRank GetRandomRank(RarityByWave rarity)
    {
        int total = rarity.normalChance + rarity.rareChance + rarity.epicChance;
        if (total == 0) return EBookRank.Normal; // 확률이 모두 0이면 노멀 반환

        int randomPoint = Random.Range(0, total);

        if (randomPoint < rarity.normalChance)
        {
            return EBookRank.Normal;
        }
        else if (randomPoint < rarity.normalChance + rarity.rareChance)
        {
            return EBookRank.Rare;
        }
        else
        {
            return EBookRank.Epic;
        }
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

    private void ApplyBookEffect(MagicBookData bookData, int stack)
    {
        int stackIndex = Mathf.Clamp(stack - 1, 0, bookData.EffectValuesByStack.Count - 1);

        // EffectValuesByStack 리스트가 비어있지 않다면 stackValue를 가져옴, 비어있다면 1로 간주
        float stackValue = (bookData.EffectValuesByStack.Count > 0) ? bookData.EffectValuesByStack[stackIndex] : 1f;

        foreach (var effect in bookData.Effects)
        {
            float finalValue = effect.Value * stackValue;
            OnBookEffectApplied?.Invoke(effect, finalValue);
            MagicBookBuffSystem.Instance.ApplyBookEffect(effect, finalValue);
        }
    }

    public List<OwnedBookInfo> GetOwnedBookInfos()
    {
        var ownedBookInfos = new List<OwnedBookInfo>();
        foreach (var ownedBookPair in _ownedBooksDict)
        {
            if (_allBooksDict.TryGetValue(ownedBookPair.Key, out MagicBookData bookData))
            {
                ownedBookInfos.Add(new OwnedBookInfo
                {
                    BookData = bookData,
                    CurrentStack = ownedBookPair.Value
                });
            }
        }
        return ownedBookInfos;
    }

    public MagicBookData[] GetSpecificBookChoice(string bookCode)
    {
        if (_allBooksDict.TryGetValue(bookCode, out var bookData))
        {
            return new MagicBookData[] { bookData };
        }
        Debug.LogWarning($"요청한 책 코드 '{bookCode}'를 찾을 수 없습니다.");
        return null;
    }

    public int GetCurrentStack(string bookCode)
    {
        return _ownedBooksDict.GetValueOrDefault(bookCode, 0);
    }

    public Dictionary<string, int> GetOwnedBooksDataForSave()
    {
        return _ownedBooksDict;
    }

    [System.Serializable]
    public struct OwnedBookInfo
    {
        public MagicBookData BookData;
        public int CurrentStack;
    }

    // 새로운 게임을 위해 MagicBookManager의 상태를 초기화하는 함수
    public void ResetManager()
    {
        // 플레이어가 보유한 책 목록을 완전히 비웁니다.
        _ownedBooksDict.Clear();

        // 조합책의 완료 상태(_combinations)도 초기화해야 한다면 아래 코드의 주석을 해제하세요.
        // foreach (var combo in _combinations)
        // {
        //     combo.Reset();
        // }
    }
}
