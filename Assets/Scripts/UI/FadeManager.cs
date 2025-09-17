using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class FadeManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static FadeManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image fadePanel; // 화면 전환용 
    [SerializeField] private TextMeshProUGUI gameOverText; // 유다희
    [SerializeField] private GameObject gameOverButtonContainer; // "다시하기", "메인메뉴로 나가기"
    [SerializeField] private Button mainMenuButton; // 메인메뉴로 나가기

    [Header("Fade Settings")]
    [SerializeField] private float sceneTransitionFadeDuration = 3.0f; // 씬 전환 페이드시간
    [SerializeField] private float gameOverFadeDuration = 3.0f; // 게임 오버 페이드 아웃 시간
    [SerializeField] private float blackScreenHoldDuration = 3.0f; // 검은 화면 유지 시간
   
    private Canvas fadeCanvas; // 페이드 패널이 속한 캔버스
    private bool isFading = false; // 페이드 중인지 확인

    // 씬 전환 완료 이벤트를 선언합니다.
    public static event Action OnSceneTransitionComplete;

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
        // MainMenuScene이 로드될 때만 Main Menu 버튼을 찾고 연결합니다.
        if (scene.name == "MainMenuScene") // <-- 실제 메인 메뉴 씬 이름
        {
            MainMenuUI mainMenuUI = FindObjectOfType<MainMenuUI>();
        }
    }
    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록
        }
        else
        {
            Destroy(gameObject); // 인스턴스 있으면 자기자신을 파괴
            return;
        }

        // 오류 검사
        if (fadePanel == null)
        {
            Debug.LogError("fadePanel이 연결되지 않음");
            enabled = false;
            return;
        }

        if(gameOverText == null) // gameOverText가 null인지 별도로 확인
        {
            Debug.LogError("FadeManager: Game Over Text가 연결되지 않았습니다. 인스펙터에서 연결해주세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        if(gameOverButtonContainer == null)
        {
            Debug.LogError("GameOverButtonContainer가 연결되지 않음");
            enabled = false;
            return;
        }

        if (mainMenuButton == null)
        {
            Debug.LogError("MainMenuButton이 연결되지 않음");
            enabled = false;
            return;
        }

        fadeCanvas = fadePanel.GetComponentInParent<Canvas>();
        if (fadeCanvas == null)
        {
            Debug.LogError("Fade Panel이 Canvas 내부에 없음");
            enabled = false;
            return;
        }

        // 초기에는 페이드 패널 투명하게 하기
        fadePanel.gameObject.SetActive(true); // 활성화는 하지만 투명한거임
        Color panelColor = fadePanel.color;
        panelColor.a = 0f;
        fadePanel.color = panelColor;

        gameOverText.gameObject.SetActive(false); // 게임오버 텍스트는 비활성화
        gameOverButtonContainer.SetActive(false); // 게임오버 버튼도 비활성화

        SetInputBlocking(false); // 초기에는 입력 허용
        isFading = false;

        if (mainMenuButton != null) 
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogError("FadeManager: mainMenuButton이 Inspector에 할당되지 않았습니다. OnClick 이벤트 연결 불가.");
        }
    }

    // 씬 전환 페이드 (페이드 아웃 -> 새 씬 로드 -> 새 씬 페이드인)
    public void TransitionToScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(SceneTransitionCoroutine(sceneName));
    }

    private IEnumerator SceneTransitionCoroutine(string sceneName)
    {
        isFading = true;
        SetInputBlocking(true); // 씬 전환 시 입력 차단

        // 페이드 아웃
        yield return StartCoroutine(Fade(1f, sceneTransitionFadeDuration));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로딩이 끝날 때까지 기다림 (로딩이 90%에서 멈추는 것은 자연스러운 현상)
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 페이드 인 (새로운 씬이 로드된 후 화면을 밝게 함)
        yield return StartCoroutine(Fade(0f, sceneTransitionFadeDuration));

        SetInputBlocking(false);
        isFading = false;

        // 모든 과정이 끝났음을 외부에 알립니다.
        OnSceneTransitionComplete?.Invoke();
        Debug.Log("FadeManager: Scene transition complete.");
    }

    // 게임 오버 페이드
    public void GameOver()
    {
        if (isFading) return;
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        isFading = true;
        SetInputBlocking(true);

        // 1. 페이드 아웃
        yield return StartCoroutine(Fade(1f, gameOverFadeDuration));

        // 2. GameOver 텍스트 표시
        gameOverText.gameObject.SetActive(true);
        gameOverButtonContainer.SetActive(true);

        SetInputBlocking(false); // 버튼 클릭 가능하도록
        isFading = false; // 다음 동작은 버튼 클릭으로 시작
    }

    private void GoToMainMenu()
    {
        Debug.Log("메인메뉴로 이동합니다.");
        HideGameOverUI();
        Time.timeScale = 1f;
        TransitionToScene("MainMenuScene");
    }

    private void HideGameOverUI()
    {
        gameOverText.gameObject.SetActive(false);
        gameOverButtonContainer.SetActive(false);
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        Color currentColor = fadePanel.color;
        float startAlpha = currentColor.a;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;
            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadePanel.color = currentColor;
            yield return null; // 다음 프레임까지 대기
        }
        currentColor.a = targetAlpha;
        fadePanel.color = currentColor;
    }
    // 입력 차단 / 허용
    private void SetInputBlocking(bool block)
    {
        if (fadePanel != null)
        {
            // 페이드 패널이 클릭 이벤트 가져감
            fadePanel.raycastTarget = block;
        }
    }
}
