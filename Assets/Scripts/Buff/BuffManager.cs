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
    private readonly Dictionary<ElementType, float>  _elementDamageBuffs = new Dictionary<ElementType, float>();
    private readonly Queue<BuffData> _buffPool = new Queue<BuffData>();

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            InitializeBuffs();
        }
        else
        {
            Destroy(gameObject);
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

    public BuffData[] GetRandomBuffChoices()
    {
        BuffData[] choices = new BuffData[_buffChoiceCount];
        EBuffType[] allBuffTypes = (EBuffType[])Enum.GetValues(typeof(EBuffType));

        for (var i = 0; i < _buffChoiceCount; i++)
        {
            var randomBuffType = allBuffTypes[Random.Range(0, allBuffTypes.Length)];
            var range = Array.Find(_buffRanges, r => r.BuffType == randomBuffType);

            choices[i] = CreateRandomBuff(randomBuffType, range);
        }

        return choices;
    }

    // 랜덤 버프 생성 메서드
    private BuffData CreateRandomBuff(EBuffType buffType, BuffValueRange range)
    {
        BuffData buff = GetBuffFromPool();

        buff.BuffType = buffType;
        buff.Value = Random.Range(range.MinValue, range.MaxValue);

        if (buffType == EBuffType.ElementDamage && range.AvailableElements != null && range.AvailableElements.Length > 0)
        {
            buff.ElementType = range.AvailableElements[Random.Range(0, range.AvailableElements.Length)];
        }

        return buff;
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
        buff.Value = 0f;
        _buffPool.Enqueue(buff);
    }

   private ElementType GetRandomElementType()
    {
        ElementType[] elements = (ElementType[])Enum.GetValues(typeof(ElementType));
        return elements[Random.Range(0, elements.Length)];
    }

    /// <summary>
    /// 테스트용
    /// </summary>
    /// <returns></returns>
    public string GetBuffStatusString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 현재 버프 상태 ===");
        sb.AppendLine($"혼 획득량: +{_activeBuffs[EBuffType.ElementSoulRate]}%");
        sb.AppendLine($"전체 타워 공격속도: +{_activeBuffs[EBuffType.AllTowerAttackSpeed]}%");

        foreach (var element in _elementDamageBuffs)
        {
            if (element.Value > 0)
            {
                sb.AppendLine($"{element.Key} 데미지: +{element.Value}%");
            }
        }

        return sb.ToString();
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
}
