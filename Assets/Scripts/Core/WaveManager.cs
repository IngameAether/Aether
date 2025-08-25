using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private MagicBookSelectionUI buffChoiceUI;
    [SerializeField] private TMP_Text waveText;

    [Header("Config")]
    [SerializeField] private float initialDelay = 1f; // 첫 번째 웨이브 대기 시간
    [SerializeField] private float nextWaveDelay = 3f; // 다음 웨이브 대기 시간

    private int currentWaveLevel = 0;
    public int CurrentWaveLevel => currentWaveLevel;
    private int _waveEndBonusCoin = 0;
    private bool _isWaitingForMagicBook = false;
    private bool _waitingForEnemies = false;


    private void Start()
    {
        spawnManager ??= FindObjectOfType<SpawnManager>();
        Debug.Log($"WaveManager: SpawnManager 연결됨? {(spawnManager != null)}");

        StartCoroutine(WaveRoutine());
    }

    private IEnumerator HandleBuffChoice()
    {
        _isWaitingForMagicBook = true;
        buffChoiceUI.ShowBookSelection();

        while (_isWaitingForMagicBook)
            yield return null;
    }

    private IEnumerator WaveRoutine()
    {
        // 첫 시작 버프 선택
        yield return StartCoroutine(HandleBuffChoice());

        GameTimer.Instance.StartTimer();
        yield return new WaitForSeconds(initialDelay);

        for (int waveIndex = 0; waveIndex < spawnManager.waves.Count; waveIndex++)
        {
            currentWaveLevel = waveIndex;
            waveText.text = $"{waveIndex + 1} wave";

            // 10웨이브마다 버프 선택
            if ((waveIndex + 1) % 10 == 0 && buffChoiceUI != null)
            {
                yield return StartCoroutine(HandleBuffChoice());
            }

            Debug.Log($"--- 웨이브 {waveIndex + 1} 시작 ---");
            yield return StartCoroutine(spawnManager.SpawnWaveEnemies(spawnManager.waves[waveIndex]));

            _waitingForEnemies = true;
            while (_waitingForEnemies) yield return null; // 적 전멸 기다림

            Debug.Log($"--- 웨이브 {waveIndex + 1} 종료 ---");

            if (waveIndex == 0)
            {
                // 1) 비동기 저장 작업 시작
                Task<bool> saveTask = GameSaveManager.Instance.SaveGameAsync(2);

                // 2) 완료까지 코루틴으로 대기 (메인 스레드 프레임은 계속 돎)
                yield return new WaitUntil(() => saveTask.IsCompleted);

                // 3) 결과 확인 및 예외 처리
                bool isSave = false;
                Exception ex = saveTask.Exception;
                if (saveTask.IsFaulted)
                {
                    Debug.LogError(ex);
                }
                else if (saveTask.IsCanceled)
                {
                    Debug.LogWarning("Save canceled");
                }
                else
                {
                    isSave = saveTask.Result;
                }

                // isSave 활용
                Debug.Log($"Save completed: {isSave}");
            }

            yield return null;

            ResourceManager.Instance.AddCoin(_waveEndBonusCoin);

            if (waveIndex < spawnManager.waves.Count - 1)
            {
                Debug.Log($"{nextWaveDelay}초 후 다음 웨이브 시작");
                yield return new WaitForSeconds(nextWaveDelay);
            }
            else
            {
                Debug.Log("모든 웨이브 완료!");
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }

    #region Action Handlers
    private void HandleBookSelectCompleted()
    {
        _isWaitingForMagicBook = false;
    }

    private void HandleBookEffectApplied(EBookEffectType bookEffectType, int value)
    {
        if (bookEffectType != EBookEffectType.WaveAether) return;
        _waveEndBonusCoin = value;
    }

    private void OnEnable()
    {
        if (buffChoiceUI != null)
            buffChoiceUI.OnBookSelectCompleted += HandleBookSelectCompleted;

        MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
        SpawnManager.OnAllEnemiesCleared += HandleWaveCleared;
    }

    private void OnDisable()
    {
        if (buffChoiceUI != null)
            buffChoiceUI.OnBookSelectCompleted -= HandleBookSelectCompleted;

        MagicBookManager.Instance.OnBookEffectApplied -= HandleBookEffectApplied;
        SpawnManager.OnAllEnemiesCleared -= HandleWaveCleared;
    }

    // ❗여기서 코루틴 시작 X. 플래그만 끕니다.
    private void HandleWaveCleared()
    {
        Debug.Log("WaveManager: 웨이브 클리어됨 (이벤트 수신) — 다음 루프 진행");
        _waitingForEnemies = false;
    }
    #endregion
}
