using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private TMP_Text waveText;

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

    public int CurrentWaveLevel => currentWaveLevel;

    private void OnEnable()
    {
        spawnManager ??= FindObjectOfType<SpawnManager>();

        // FadeManager가 준비될 때까지 기다린 후, 씬 전환이 끝나면 게임 루틴을 시작합니다.
        StartCoroutine(WaitForFadeManagerAndSubscribe());

        // PopUpManager의 팝업 닫힘 이벤트를 구독합니다.
        if (PopUpManager.Instance != null)
            PopUpManager.Instance.OnPopUpClosed += OnPopupClosed;

        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
        }

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
        // --- 1. 최초 마법책 선택 ---
        Debug.Log("WaveManager: 최초 마법책 선택을 시작합니다.");
        MagicBookManager.Instance.PrepareSelection(BookRequestType.Regular);
        yield return StartCoroutine(WaitForChoice()); // 팝업을 띄우고 선택을 기다립니다.
        Debug.Log("WaveManager: 최초 선택 완료! 게임을 시작합니다.");

        // --- 2. 게임 시작 준비 ---
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StartTimer();
        }
        yield return new WaitForSeconds(initialDelay);

        // --- 3. 웨이브 루프 시작 ---
        for (int waveIndex = 0; waveIndex < spawnManager.waves.Count; waveIndex++)
        {
            // ... (기존의 for 루프 안의 모든 코드는 그대로 유지) ...
            currentWaveLevel = waveIndex;
            int displayWave = waveIndex + 1;
            waveText.text = $"{displayWave} wave";
            GameManager.Instance.SetWave(displayWave);

            if (displayWave % 10 == 0)
            {
                MagicBookManager.Instance.PrepareSelection(BookRequestType.Regular);
                yield return StartCoroutine(WaitForChoice());
            }

            _waitingForEnemies = true;
            yield return StartCoroutine(spawnManager.SpawnWaveEnemies(spawnManager.waves[waveIndex]));
            while (_waitingForEnemies) yield return null;

            if (waveIndex == bossWaveIndex)
            {
                Debug.Log("보스 처치! 특별 보상을 제공합니다.");
                MagicBookManager.Instance.PrepareSelection(BookRequestType.Specific, bossRewardBookCode);
                yield return StartCoroutine(WaitForChoice());
            }

            if (waveIndex == 0)
            {
                Task<bool> saveTask = GameSaveManager.Instance.SaveGameAsync(2);
                yield return new WaitUntil(() => saveTask.IsCompleted);
                if (saveTask.IsFaulted) Debug.LogError(saveTask.Exception);
                else Debug.Log($"Save completed: {saveTask.Result}");
            }

            ResourceManager.Instance.AddCoin(_waveEndBonusCoin);

            if (waveIndex < spawnManager.waves.Count - 1)
            {
                yield return new WaitForSeconds(nextWaveDelay);
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
        PopUpManager.Instance.OpenPopUpInGame("MagicBookPopup");
        while (_isWaitingForChoice)
            yield return null;
    }

    #region Action Handlers
    // PopUpManager의 OnPopUpClosed 이벤트에 연결될 핸들러
    private void OnPopupClosed()
    {
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
        // 효과 타입이 '웨이브 종료 시 에테르 보너스'일 경우에만 작동
        if (effect.EffectType == EBookEffectType.WaveAether)
        {
            // finalValue는 float이지만, 코인 값은 정수이므로 int로 변환
            _waveEndBonusCoin = (int)finalValue;
        }
    }
    #endregion
}
