using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public int Coin { get; private set; }
    public int LightElement { get; private set; }
    public int DarkElement { get; private set; }
    public int EnemyKillBonusCoin { get; private set; } = 0;
    public int MaxElementUpgrade { get; private set; } = 0;
    public bool IsBossRewardDouble { get; private set; } = false;

    private int _lightElementChance = 5;
    private int _darkElementChance = 5;
    private int _lightElementRate = 50;
    private int _darkElementRate = 50;

    // 이벤트
    public static event Action<int> OnCoinChanged;
    public static event Action<int, int> OnElementChanged; // light, dark

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    private void OnEnable()
    {
        // MagicBookManager.Instance가 null일 때를 대비한 안전 코드 추가
        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
        }
    }

    private void OnDisable()
    {
        // MagicBookManager.Instance가 null일 때를 대비한 안전 코드 추가
        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied -= HandleBookEffectApplied;
        }
    }

    #region Coin Management

    public void AddCoin(int amount)
    {
        Coin += amount;
        OnCoinChanged?.Invoke(Coin);
    }

    public bool SpendCoin(int amount)
    {
        if (Coin >= amount)
        {
            Coin -= amount;
            OnCoinChanged?.Invoke(Coin);
            return true;
        }

        return false;
    }

    #endregion

    #region ReinforceType Element Management

    public void AddElement(ReinforceType type, int amount)
    {
        switch (type)
        {
            case ReinforceType.Light:
                LightElement += amount;
                break;
            case ReinforceType.Dark:
                DarkElement += amount;
                break;
        }

        OnElementChanged?.Invoke(LightElement, DarkElement);
    }

    public void GetElement(int amount)
    {
        LightElement += amount;
        DarkElement += amount;
        OnElementChanged?.Invoke(LightElement, DarkElement);
    }

    public bool SpendElement(ReinforceType type, int amount)
    {
        var canSpend = false;

        switch (type)
        {
            case ReinforceType.Light:
                if (LightElement >= amount)
                {
                    LightElement -= amount;
                    canSpend = true;
                }
                break;
            case ReinforceType.Dark:
                if (DarkElement >= amount)
                {
                    DarkElement -= amount;
                    canSpend = true;
                }
                break;
        }

        if (canSpend) OnElementChanged?.Invoke(LightElement, DarkElement);
        return canSpend;
    }

    /// <summary>
    /// 적 격퇴 시 원소 획득 체크 (마법도서 효과 적용)
    /// </summary>
    public void TryGetElementOnKill()
    {
        // 빛 원소 획득 체크 (GN3 효과)
        if (Random.Range(0f, 100f) < _lightElementChance) AddElement(ReinforceType.Light, 1);

        // 어둠 원소 획득 체크 (GN4 효과)
        if (Random.Range(0f, 100f) < _darkElementChance) AddElement(ReinforceType.Dark, 1);
    }

    /// <summary>
    /// 원소 드롭 시 타입 결정 (GR2, GR3 효과 적용)
    /// </summary>
    /// <returns></returns>
    public ReinforceType GetRandomElementType()
    {
        return Random.Range(0f, 100f) < _lightElementRate
            ? ReinforceType.Light
            : ReinforceType.Dark;
    }

    #endregion

    private void HandleBookEffectApplied(BookEffect effect, float finalValue)
    {
        // finalValue는 float이지만, 이 스크립트의 효과들은 대부분 정수 값을 사용하므로 int로 변환합니다.
        int value = (int)finalValue;

        // 이제 파라미터로 받은 effect의 EffectType을 사용합니다.
        switch (effect.EffectType)
        {
            case EBookEffectType.LightElementChance:
                _lightElementChance = value;
                break;
            case EBookEffectType.DarkElementChance:
                _darkElementChance = value;
                break;
            case EBookEffectType.LightElementRate:
                _lightElementRate = value;
                _darkElementRate = 100 - value;
                break;
            case EBookEffectType.DarkElementRate:
                _darkElementRate = value;
                _lightElementRate = 100 - value;
                break;
            case EBookEffectType.KillAetherBonus:
                EnemyKillBonusCoin = value;
                break;
            case EBookEffectType.ElementMaxUpgrade:
                MaxElementUpgrade += value;
                break;
            case EBookEffectType.BossRewardDouble:
                IsBossRewardDouble = true;
                break;
        }
    }

    public void ResetAllResources()
    {
        Coin = 0;
        LightElement = 0;
        DarkElement = 0;
        _lightElementChance = 5;
        _darkElementChance = 5;
        _lightElementRate = 50;
        _darkElementRate = 50;
        MaxElementUpgrade = 10;

        OnCoinChanged?.Invoke(Coin);
        OnElementChanged?.Invoke(LightElement, DarkElement);
    }
}
