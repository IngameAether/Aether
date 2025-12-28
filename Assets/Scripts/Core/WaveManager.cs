using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private TMP_Text waveText;

    // 맵 배경 매니저 참조
    [SerializeField] private MapBackgroundManager mapBackgroundManager;

    [Header("Config")]
    [SerializeField] private float initialDelay = 1f;
    [SerializeField] private float nextWaveDelay = 3f;

    [Header("Boss Config")]
    [SerializeField] private int bossWaveIndex = 29;
    [SerializeField] private string bossRewardBookCode = "YourBossRewardBookCode";

    private int currentWaveLevel = 0;
    private int _waveEndBonusCoin = 0;
    private bool _isWaitingForChoice = false;
    private bool _waitingForEnemies = false;
    private bool _isExtraLife = false;
    private bool _initialChoiceMade = false;

    public int CurrentWaveLevel => currentWaveLevel;

    private void Start()
    {
        // FadeManager가 준비될 때까지 기다린 후, 씬 전환이 끝나면 게임 루틴을 시작합니다.
        spawnManager ??= FindObjectOfType<SpawnManager>();

        // 씬에서 MapBackgroundManager를 자동으로 찾아옵니다.
        if (mapBackgroundManager == null)
        {
            mapBackgroundManager = FindObjectOfType<MapBackgroundManager>();
        }

        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForFadeManagerAndSubscribe());

        // PopUpManager의 팝업 닫힘 이벤트를 구독합니다.
        if (PopUpManager.Instance != null)
            PopUpManager.Instance.OnPopUpClosed += OnPopupClosed;

        SpawnManager.OnAllEnemiesCleared += HandleWaveCleared;
    }

    private void OnDisable()
    {
        // 구독했던 모든 이벤트를 해제합니다.
        if (FadeManager.Instance != null)
            FadeManager.OnSceneTransitionComplete -= StartWaveRoutine;
        if (PopUpManager.Instance != null)
            PopUpManager.Instance.OnPopUpClosed -= OnPopupClosed;

        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied -= HandleBookEffectApplied;
        }

        SpawnManager.OnAllEnemiesCleared -= HandleWaveCleared;
    }

    // 초기화 함수
    public void ResetForNewGame()
    {
        // 최초 선택 상태를 '미완료'로 되돌립니다.
        _initialChoiceMade = false;

        // 현재 웨이브 레벨도 0으로 초기화합니다.
        currentWaveLevel = 0;

        // 실행 중인 모든 코루틴을 멈춰서 이전 게임의 웨이브 진행을 완전히 중단시킵니다.
        StopAllCoroutines();
    }

    // FadeManager가 씬에 나타날 때까지 기다렸다가 이벤트를 구독하는 코루틴
    private IEnumerator WaitForFadeManagerAndSubscribe()
    {
        // FadeManager 인스턴스가 생성될 때까지 기다립니다.
        yield return new WaitUntil(() => FadeManager.Instance != null);
        FadeManager.OnSceneTransitionComplete += StartWaveRoutine;
    }

    // FadeManager로부터 "씬 전환 완료" 신호를 받으면 호출될 함수
    private void StartWaveRoutine()
    {
        // 이전에 구독했던 이벤트를 해제하여 중복 실행을 방지합니다.
        FadeManager.OnSceneTransitionComplete -= StartWaveRoutine;
        // 실제 게임 웨이브 코루틴을 시작합니다.
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        int startWaveIndex = 0;
        bool isLoadedGame = false;
        if (GameSaveManager.Instance != null && GameSaveManager.Instance.CurrentGameData != null)
        {
            startWaveIndex = GameSaveManager.Instance.CurrentGameData.currentWave;
            isLoadedGame = true;
            Debug.Log($"저장된 게임 로드: Wave {startWaveIndex}부터 시작");
        }

        // --- 1. 최초 마법책 선택 (새 게임인 경우만) ---
        if (!isLoadedGame)
        {
            Debug.Log("WaveManager: 최초 마법책 선택을 시작합니다.");
            MagicBookManager.Instance.PrepareSelection(BookRequestType.Regular);
            yield return StartCoroutine(WaitForChoice()); // 팝업을 띄우고 선택을 기다립니다.
            Debug.Log("WaveManager: 최초 선택 완료! 게임을 시작합니다.");
        }

        // --- 게임 시작 준비 ---
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StartTimer();
        }

        yield return new WaitForSeconds(initialDelay);

        // --- 3. 웨이브 루프 시작 ---
        for (int waveIndex = startWaveIndex; waveIndex < spawnManager.waveDatas.Count; waveIndex++)
        {
            currentWaveLevel = waveIndex;
            int displayWave = waveIndex + 1;
            waveText.text = $"{displayWave} wave";
            GameManager.Instance.SetWave(displayWave);

            // 웨이브가 시작되기 직전에 배경 업데이트를 요청합니다.
            if (mapBackgroundManager != null)
            {
                mapBackgroundManager.UpdateBackground(displayWave);
            }

            if (displayWave % 10 == 0)
            {
                MagicBookManager.Instance.PrepareSelection(BookRequestType.Regular);
                yield return StartCoroutine(WaitForChoice());
            }

            // 첫 웨이브가 아니면 타이머를 다시 시작
            if (waveIndex > 0 && GameTimer.Instance != null)
            {
                GameTimer.Instance.StartTimer();
            }

            _waitingForEnemies = true;
            yield return StartCoroutine(spawnManager.SpawnWaveEnemies(spawnManager.waveDatas[waveIndex]));

            float waveTimeLimit = 60f; // Game_Data의 wave_time
            float elapsedTime = 0f;
            bool waveTimeLimitReached = false;

            while (_waitingForEnemies)
            {
                elapsedTime += Time.deltaTime;

                if (!waveTimeLimitReached && elapsedTime >= waveTimeLimit)
                {
                    waveTimeLimitReached = true;
                    GameTimer.Instance.StopTimer(); // 60초 경과 시 타이머 멈춤
                }

                yield return null;
            }

            // 웨이브가 끝나면 타이머를 멈춤
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StopTimer();
            }

            if (_isExtraLife)
            {
                GameManager.Instance.AddLife();
            }

            if (waveIndex == bossWaveIndex)
            {
                Debug.Log("보스 처치! 특별 보상을 제공합니다.");
                MagicBookManager.Instance.PrepareSelection(BookRequestType.Specific, bossRewardBookCode);
                yield return StartCoroutine(WaitForChoice());
            }

            // 매 웨이브 종료 시 선택된 슬롯에 저장
            int slotIndex = GameSaveManager.Instance.SelectedSlotIndex;
            if (slotIndex >= 0 && slotIndex < 3)
            {
                bool saveSuccess = false;
                yield return StartCoroutine(GameSaveManager.Instance.SaveGame(slotIndex, (success) =>
                {
                    saveSuccess = success;
                }));

                if (saveSuccess)
                {
                    Debug.Log($"Wave {waveIndex + 1} 저장 완료 (슬롯 {slotIndex})");
                }
                else
                {
                    Debug.LogError($"Wave {waveIndex + 1} 저장 실패 (슬롯 {slotIndex})");
                }
            }
            else
            {
                Debug.LogWarning($"유효하지 않은 슬롯 인덱스: {slotIndex}. 저장을 건너뜁니다.");
            }

            ResourceManager.Instance.AddCoin(_waveEndBonusCoin);

            if (waveIndex < spawnManager.waveDatas.Count - 1)
            {
                yield return new WaitForSeconds(6f);
            }
            else
            {
                Debug.Log("모든 웨이브 완료!");
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }

    // 팝업을 띄우고 닫힐 때까지 기다리는 단일 코루틴
    private IEnumerator WaitForChoice()
    {
        _isWaitingForChoice = true;
        Debug.Log("WaitForChoice: 팝업 열기 시작...");

        if (PopUpManager.Instance == null)
        {
            Debug.LogError("WaitForChoice: PopUpManager.Instance가 null입니다!");
            _isWaitingForChoice = false;
            yield break;
        }

        PopUpManager.Instance.OpenPopUpInGame("MagicBookPopup");
        Debug.Log("WaitForChoice: 팝업 열기 요청 완료, 닫힐 때까지 대기...");

        float startTime = Time.unscaledTime;
        while (_isWaitingForChoice)
        {
            // 30초 타임아웃 추가 (안전장치)
            if (Time.unscaledTime - startTime > 30f)
            {
                Debug.LogError("WaitForChoice: 30초 타임아웃! 팝업이 열리지 않았거나 닫히지 않았습니다.");
                _isWaitingForChoice = false;
                break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }

        Debug.Log("WaitForChoice: 팝업 닫힘 확인, 대기 종료");
    }

    #region Action Handlers
    // PopUpManager의 OnPopUpClosed 이벤트에 연결될 핸들러
    private void OnPopupClosed()
    {
        Debug.Log("OnPopupClosed: 팝업 닫힘 이벤트 수신");
        _isWaitingForChoice = false;
    }

    // 조합 완성 이벤트 핸들러
    private void HandleCombinationCompleted(string rewardBookCode)
    {
        Debug.Log($"WaveManager: 조합 완성 이벤트 수신! 보상({rewardBookCode})을 즉시 제공합니다.");
        MagicBookManager.Instance.PrepareSelection(BookRequestType.Specific, rewardBookCode);
        StartCoroutine(WaitForChoice());
    }

    private void HandleWaveCleared() { _waitingForEnemies = false; }
    private void HandleBookEffectApplied(BookEffect effect, float finalValue)
    {
        switch (effect.EffectType)
        {
            // 효과 타입이 '웨이브 종료 시 에테르 보너스'일 경우에만 작동
            case EBookEffectType.WaveAether:
                // finalValue는 float이지만, 코인 값은 정수이므로 int로 변환
                _waveEndBonusCoin = (int)finalValue;
                break;
            case EBookEffectType.ExtraLife:
                GameManager.Instance.AddLife((int)finalValue);
                break;
            case EBookEffectType.FullLife:
                GameManager.Instance.SetMaxLives((int)finalValue);
                break;
        }
    }
    #endregion
}
