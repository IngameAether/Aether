using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private BuffValueRange[] _buffRanges;
    [SerializeField] private int _buffChoiceCount = 3; // 버프 선택지 개수

    public event Action<float> OnElementSoulRateChanged;
    public event Action<float> OnAllTowerAttackSpeedChanged;
    public event Action<ElementType, float> OnElementDamageChanged;

    private const int INITIAL_POOL_SIZE = 5;
    private readonly Dictionary<EBuffType, float> _activeBuffs = new Dictionary<EBuffType, float>();
    private readonly Dictionary<ElementType, float> _elementDamageBuffs = new Dictionary<ElementType, float>();
    private readonly Queue<BuffData> _buffPool = new Queue<BuffData>();

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            InitializeBuffs();
        }
    }

    private void InitializeBuffs()
    {
        _activeBuffs[EBuffType.ElementSoulRate] = 0f;
        _activeBuffs[EBuffType.AllTowerAttackSpeed] = 0f;

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            if (element is ElementType.None or ElementType.Tower) continue;
            _elementDamageBuffs[element] = 0f;
        }

        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            _buffPool.Enqueue(new BuffData());
        }
    }

    /// <summary>
    /// 랜덤 버프 선택권 3개 얻기
    /// </summary>
    /// <returns></returns>
    public BuffData[] GetRandomBuffChoices()
    {
        int choiceCount = _buffChoiceCount; // 3
        List<BuffData> pool = new List<BuffData>();

        // 모든 버프 타입에 대해 가능한 버프 후보(여러 element 포함) 생성 가능한지 확인
        EBuffType[] allBuffTypes = (EBuffType[])Enum.GetValues(typeof(EBuffType));
        foreach (var bt in allBuffTypes)
        {
            var range = Array.Find(_buffRanges, r => r.BuffType == bt);
            if (range == null) continue;

            if (bt == EBuffType.ElementDamage)
            {
                // ElementDamage일 경우 사용 가능한 요소별로 후보 생성
                if (range.AvailableElements != null && range.AvailableElements.Length > 0)
                {
                    foreach (var elem in range.AvailableElements)
                    {
                        var buff = CreateRandomBuff(bt, range, elem);
                        pool.Add(buff);
                    }
                }
                else
                {
                    // 요소 미지정이면 전체 요소(혹은 None)로 하나 생성
                    var buff = CreateRandomBuff(bt, range, ElementType.None);
                    pool.Add(buff);
                }
            }
            else
            {
                var buff = CreateRandomBuff(bt, range, ElementType.None);
                pool.Add(buff);
            }
        }

        // pool에서 중복(같은 BuffType+ElementType) 없이 랜덤으로 선택
        BuffData[] choices = new BuffData[choiceCount];
        if (pool.Count == 0) return choices; // 모두 null

        int maxAttempts = 50;
        int selected = 0;
        while (selected < choiceCount && pool.Count > 0 && maxAttempts-- > 0)
        {
            int idx = Random.Range(0, pool.Count);
            choices[selected++] = pool[idx];
            pool.RemoveAt(idx);
        }

        // 만약 pool이 부족해서 null이 포함될 수 있음(버튼쪽에서 null 처리)
        return choices;
    }

    /// <summary>
    /// 버프 적용
    /// </summary>
    /// <param name="buffData"></param>
    public void ApplyBuff(BuffData buffData)
    {
        switch (buffData.BuffType)
        {
            case EBuffType.ElementDamage:
                _elementDamageBuffs[buffData.ElementType] += buffData.Value;
                OnElementDamageChanged?.Invoke(buffData.ElementType, _elementDamageBuffs[buffData.ElementType]);
                Debug.Log($"{buffData.ElementType} 데미지 버프 적용: +{buffData.Value}% (총 {_elementDamageBuffs[buffData.ElementType]}%)");
                break;

            case EBuffType.ElementSoulRate:
                _activeBuffs[EBuffType.ElementSoulRate] += buffData.Value;
                OnElementSoulRateChanged?.Invoke(_activeBuffs[EBuffType.ElementSoulRate]);
                Debug.Log($"혼 획득량 버프 적용: +{buffData.Value}% (총 {_activeBuffs[EBuffType.ElementSoulRate]}%)");
                break;

            case EBuffType.AllTowerAttackSpeed:
                _activeBuffs[EBuffType.AllTowerAttackSpeed] += buffData.Value;
                OnAllTowerAttackSpeedChanged?.Invoke(_activeBuffs[EBuffType.AllTowerAttackSpeed]);
                Debug.Log($"전체 타워 공격 속도 버프 적용: +{buffData.Value}% (총 {_activeBuffs[EBuffType.AllTowerAttackSpeed]}%)");
                break;
        }
    }

    /// <summary>
    /// Active Buff 다 가져오기
    /// </summary>
    /// <param name="elementType"></param>
    /// <returns></returns>
    public BuffDataResult GetActiveBuffData(ElementType elementType)
    {
        return new BuffDataResult(
            _elementDamageBuffs[elementType],
            _activeBuffs[EBuffType.AllTowerAttackSpeed],
            _activeBuffs[EBuffType.ElementSoulRate]
        );
    }

    /// <summary>
    /// 모든 버프 초기화
    /// </summary>
    public void ResetAllBuffs()
    {
        foreach (var key in _activeBuffs.Keys.ToList())
        {
            _activeBuffs[key] = 0f;
        }

        foreach (var key in _elementDamageBuffs.Keys.ToList())
        {
            _elementDamageBuffs[key] = 0f;
        }

        Debug.Log("모든 버프가 초기화되었습니다.");
    }

    #region

    // CreateRandomBuff 오버로드: ElementType을 인자로 받아 정수 퍼센트 생성
    private BuffData CreateRandomBuff(EBuffType buffType, BuffValueRange range, ElementType element)
    {
        BuffData buff = GetBuffFromPool();

        buff.BuffType = buffType;

        // 정수 퍼센트만 허용: Min,Max는 인스펙터에서 정수 값으로 설정해두세요.
        int min = Mathf.RoundToInt(range.MinValue);
        int max = Mathf.RoundToInt(range.MaxValue);
        if (max < min) max = min;

        // Random.Range(int, int) 상한은 exclusive이므로 +1
        buff.Value = Random.Range(min, max + 1);
        buff.ElementType = element;

        return buff;
    }

    /// <summary>
    /// 오브젝트 풀에서 버프 가져오기
    /// </summary>
    /// <returns></returns>
    private BuffData GetBuffFromPool()
    {
        if (_buffPool.Count > 0)
        {
            return _buffPool.Dequeue();
        }
        return new BuffData();
    }

    /// <summary>
    /// 오브젝트 풀로 버프 반환
    /// </summary>
    /// <param name="buff"></param>
    public void ReturnBuffToPool(BuffData buff)
    {
        if (buff == null) return;
        buff.Value = 0f;
        buff.BuffType = default;
        buff.ElementType = default;
        _buffPool.Enqueue(buff);
    }

    // 버프 저장
    public List<BuffSaveEntry> GetBuffSaveEntries()
    {
        var list = new List<BuffSaveEntry>();
        // 일반 _activeBuffs (ElementSoulRate, AllTowerAttackSpeed)
        foreach (var kv in _activeBuffs)
        {
            list.Add(new BuffSaveEntry { buffType = kv.Key, elementType = ElementType.None, value = kv.Value });
        }
        // element damage buffs
        foreach (var kv in _elementDamageBuffs)
        {
            list.Add(new BuffSaveEntry { buffType = EBuffType.ElementDamage, elementType = kv.Key, value = kv.Value });
        }
        return list;
    }

    public void LoadFromSaveEntries(List<BuffSaveEntry> entries)
    {
        // 초기화
        ResetAllBuffs();
        if (entries == null) return;

        foreach (var e in entries)
        {
            if (e.buffType == EBuffType.ElementDamage)
            {
                _elementDamageBuffs[e.elementType] = e.value;
                OnElementDamageChanged?.Invoke(e.elementType, e.value);
            }
            else
            {
                _activeBuffs[e.buffType] = e.value;
                if (e.buffType == EBuffType.AllTowerAttackSpeed) OnAllTowerAttackSpeedChanged?.Invoke(e.value);
                if (e.buffType == EBuffType.ElementSoulRate) OnElementSoulRateChanged?.Invoke(e.value);
            }
        }
        #endregion
    }
}
