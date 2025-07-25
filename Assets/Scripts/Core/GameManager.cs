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

    [Header("플레이어 목숨")]
    [SerializeField] private int initialLives = 5; // 기본 목숨은 5로 설정. 인스펙터에서 수정가능
    private int currentLives; // 남아있는 목숨

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
        if (scene.name == "GameScene") // 실제 게임 씬 이름들을 여기에 추가
        {
            // 목숨 텍스트 UI를 씬에서 찾습니다.
            // 1. 오브젝트 이름으로 찾기
            GameObject livesTextObj = GameObject.Find("LivesCountText"); 
            if (livesTextObj != null)
            {
                livesText = livesTextObj.GetComponent<TextMeshProUGUI>();
                if (livesText == null)
                {
                    Debug.LogError("GameManager: 'LivesCountText' 오브젝트에서 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("GameManager: 씬에서 'LivesCountText'라는 이름의 오브젝트를 찾을 수 없습니다.");
            }

            // 씬이 로드될 때마다 목숨을 초기화 (게임 시작)
            InitializeLives();
        }
        else if (scene.name == "MainMenuScene") // 메인 메뉴 씬에서는 목숨 UI를 비웁니다.
        {
            livesText = null; // 참조 해제
            Time.timeScale = 1f; // 혹시 게임 오버 후 메인 메뉴로 돌아왔다면 타임스케일 정상화
            IsGameOver = false;
        }
    }

    private void InitializeLives()
    {
        currentLives = initialLives;
        UpdateLivesUI();
        Debug.Log("게임 시작. 초기 목숨 부여");
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

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("GameManager: FadeManager 인스턴스를 찾을 수 없습니다. 게임 오버 UI를 표시할 수 없습니다.");
            SceneManager.LoadScene("MainMenuscene");
        }
    }

    public void ChangeScene(EScene scene)
    {
        SceneManager.LoadScene((int)scene);
    }
}
