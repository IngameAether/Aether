using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using TMPro;

public enum EScene
{
    MainMenu,
    InGame,
    Game,
    // 필요한 다른 씬 추가
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsGameOver { get; private set; }
    public int CurrentWave { get; private set; } // 현재 웨이브 정보를 저장할 변수

    [Header("플레이어 목숨")]
    [SerializeField] private int initialLives = 3; // 기본 목숨은 5로 설정. 인스펙터에서 수정가능

    public int currentLives { get; private set; } // 남아있는 목숨

    [Header("UI에 목숨을 표시")]
    [SerializeField] private TextMeshProUGUI livesText; // 텍스트로 남은 목숨을 표시

    private string BGM_main;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeLives();
        IsGameOver = false; // <- 게임 시작 시 게임 오버 아님으로 초기화
        PlayBgmIfNotPlaying("BGM_main");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") // 실제 게임 씬 이름
        {
            // 인게임 BGM 재생
            PlayBgmIfNotPlaying("BGM_InGame"); // 인게임 BGM 이름

            GameObject livesTextObj = GameObject.Find("LivesCountText");
            if (livesTextObj != null)
                livesText = livesTextObj.GetComponent<TextMeshProUGUI>();

            InitializeLives();
        }
        else if (scene.name == "MainMenuScene") // 실제 메인 메뉴 씬 이름
        {
            // 메인 메뉴 BGM 재생
            PlayBgmIfNotPlaying("BGM_main"); // 메인 메뉴 BGM 이름

            livesText = null;
            Time.timeScale = 1f;
            IsGameOver = false;
        }
    }

    public void ResetForNewGame()
    {
        currentLives = initialLives; // 목숨을 최대로
        CurrentWave = 1;             // 웨이브를 1로
        IsGameOver = false;          // 게임오버 상태 해제
        Time.timeScale = 1f;         // 시간 흐르게
        UpdateLivesUI();
        Debug.Log("GameManager 상태가 초기화되었습니다.");
    }

    //새로운 게임을 시작하기 위해 모든 관련 매니저의 상태를 초기화합니다.
    public void PrepareNewGame()
    {
        ResetForNewGame();
        // 게임 시간을 다시 흐르게 설정
        Time.timeScale = 1f;

        if (PopUpManager.Instance != null)
            PopUpManager.ResetInitialBookFlag();

        if (WaveManager.Instance != null)
            WaveManager.Instance.ResetForNewGame();

        if (MagicBookManager.Instance != null)
            MagicBookManager.Instance.ResetManager();

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.ResetAllResources();
    }

    private void PlayBgmIfNotPlaying(string bgmName)
    {
        // 재생하려는 BGM이 이미 재생 중이라면 아무것도 하지 않음
        if (BGM_main == bgmName) return;

        // AudioManager가 준비되었다면 BGM 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(bgmName);
            BGM_main = bgmName; // 현재 재생 중인 BGM 이름 기록
        }
    }

    private void InitializeLives()
    {
        currentLives = initialLives;
        UpdateLivesUI();
        IsGameOver = false;
        CurrentWave = 1; // 게임 시작 시 웨이브를 1로 초기화
        Debug.Log("게임 시작. 초기 목숨 및 웨이브 설정 완료.");
    }


    public void LoseLife()
    {
        if(currentLives > 0)
        {
            currentLives--; // 목숨 1 감소
            UpdateLivesUI(); // UI 업데이트
            Debug.Log("Life Lost! Current Lives: {currentLives}");

            if(currentLives <= 0) // 목숨이 0이면 게임오버
            {
                GameOver();
            }
        }
    }

    public void AddLife(int amout = 1)
    {
        if (amout < 0) throw new ArgumentOutOfRangeException(nameof(amout));
        currentLives += amout;
        UpdateLivesUI();
    }

    public void SetMaxLives(int maxLives)
    {
        if (maxLives < 1) throw new ArgumentOutOfRangeException(nameof(maxLives));
        initialLives = maxLives;
        currentLives = maxLives;
        UpdateLivesUI();
    }

    private void UpdateLivesUI() // 목숨 UI 업데이트
    {
        if(livesText != null)
        {
            livesText.text = currentLives.ToString(); // 남은 목숨은 텍스트로
        }
        else
        {
            Debug.LogWarning("GameManager: livesText (TextMeshProUGUI)가 Inspector에 할당되지 않았습니다." +
            "목숨 UI를 표시할 수 없습니다.");
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        IsGameOver = true; // <- 게임 오버 상태로 설정
        Time.timeScale = 0f; // <- 게임 시간 정지

        if (AudioManager.Instance != null)
        {
            // BGM 정지
            AudioManager.Instance.StopBGM();
        }

        if (FadeManager.Instance != null)
            FadeManager.Instance.GameOver();
        else
            SceneManager.LoadScene("MainMenuScene");
    }
    public void ChangeScene(EScene scene)
    {
        SceneManager.LoadScene((int)scene);
    }

    // 현재 웨이브 숫자 업데이트
    public void SetWave(int waveNumber)
    {
        CurrentWave = waveNumber;
        Debug.Log($"Wave {CurrentWave} 시작!");
    }
}
