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
        TileInteraction.InitializeTileData();
        IsGameOver = false; // <- 게임 시작 시 게임 오버 아님으로 초기화
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
            GameObject livesTextObj = GameObject.Find("LivesCountText");
            if (livesTextObj != null)
                livesText = livesTextObj.GetComponent<TextMeshProUGUI>();

            InitializeLives();
        }
        else if (scene.name == "MainMenuScene") // 실제 메인 메뉴 씬 이름
        {
            livesText = null;
            Time.timeScale = 1f;
            IsGameOver = false;
        }
    }

    public void ResetForNewGame()
    {
        currentLives = GameDataDatabase.GetInt("life", 5);
        CurrentWave = GameDataDatabase.GetInt("wave", 1);
        TileInteraction.InitializeTileData();
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

    private void InitializeLives()
    {
        initialLives = GameDataDatabase.GetInt("life_max", 5);
        currentLives = GameDataDatabase.GetInt("life", 5);
        
        UpdateLivesUI();
        IsGameOver = false;
        CurrentWave = GameDataDatabase.GetInt("wave", 1);
    }


    public void LoseLife()
    {
        if(currentLives > 0)
        {
            currentLives--; // 목숨 1 감소
            UpdateLivesUI(); // UI 업데이트
            Debug.Log("Life Lost! Current Lives: {currentLives}");

            // 목숨 잃는 소리 재생
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SfxType.Lose_life);
            }

            if (currentLives <= 0) // 목숨이 0이면 게임오버
            {
                GameOver();
            }
        }
    }

    public void AddLife(int amout = 1)
    {
        if (amout < 0) throw new ArgumentOutOfRangeException(nameof(amout));
        currentLives += amout;
        if (currentLives > initialLives) currentLives = initialLives;
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
        if (livesText != null)
        {
            livesText.text = currentLives.ToString();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        IsGameOver = true; // <- 게임 오버 상태로 설정
        GameSaveManager.Instance.ClearGameDataForNewGame(GameSaveManager.Instance.SelectedSlotIndex);
        Time.timeScale = 0f; // <- 게임 시간 정지

        // 단순히 BGM을 끄는 게 아니라, WaveManager에게 게임오버 브금을 틀라고 시킴
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.PlayGameOverBGM();
        }
        // 혹시 WaveManager가 없다면 안전하게 끄기만 함
        else if (AudioManager.Instance != null)
        {
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

        if (CurrentWave == 100)
        {
            WaveManager.Instance.SetThankYouScreen();
            GameSaveManager.Instance.ClearGameDataForNewGame(GameSaveManager.Instance.SelectedSlotIndex);
        }

        Debug.Log($"Wave {CurrentWave} 시작!");
    }
}
