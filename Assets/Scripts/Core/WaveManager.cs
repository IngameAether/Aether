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
    public int CurrentWaveLevel => currentWaveLevel;

    private int _waveEndBonusCoin = 0;
    private bool _isWaitingForChoice = false;
    private bool _waitingForEnemies = false;

    private void Start()
    {
        spawnManager ??= FindObjectOfType<SpawnManager>();
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        // 1. MagicBookManager에게 '일반 선택'을 준비시킴
        MagicBookManager.Instance.PrepareSelection(BookRequestType.Regular);
        // 2. PopUpManager를 호출하고 선택을 기다림
        yield return StartCoroutine(WaitForChoice());

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StartTimer();
        }

        yield return new WaitForSeconds(initialDelay);

        for (int waveIndex = 0; waveIndex < spawnManager.waves.Count; waveIndex++)
        {
            currentWaveLevel = waveIndex;
            waveText.text = $"{waveIndex + 1} wave";

            if ((waveIndex + 1) % 10 == 0)
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

    private void OnEnable()
    {
        // PopUpManager는 DontDestroyOnLoad 객체일 수 있으므로 null 체크 후 구독
        if (PopUpManager.Instance != null)
            PopUpManager.Instance.OnPopUpClosed += OnPopupClosed;

        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
            MagicBookManager.Instance.OnCombinationCompleted += HandleCombinationCompleted;
        }

        SpawnManager.OnAllEnemiesCleared += HandleWaveCleared;
    }

    private void HandleWaveCleared() { _waitingForEnemies = false; }
    private void HandleBookEffectApplied(EBookEffectType type, int val) { _waveEndBonusCoin = val; }
    #endregion
}
