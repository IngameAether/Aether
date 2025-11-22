using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    public static event Action OnSceneTransitionComplete;

    [Header("UI References")]
    [SerializeField] private Image fadePanel;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingContainer;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    // 게임오버 후 터치를 기다리는 상태인지 확인하는 변수
    private bool _isWaitingForGameOverTouch = false;

    [Header("Fade Settings")]
    [SerializeField] private float sceneTransitionFadeDuration = 1.0f;
    [SerializeField] private float gameOverFadeDuration = 1.5f;
    [SerializeField] private float gameOverTextAnimationDuration = 2.0f;
    [SerializeField] private float minLoadingTime = 1.5f;
    private bool isTransitioning = false;
    private float fadeDuration; // Fade 코루틴에서 사용할 변수

    private bool isFading = false;
    private Coroutine _loadingTextAnimation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeUI();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // OnSceneLoaded가 화면을 밝히는 역할을 하도록 수정
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 중이 아닐 때만 페이드 인 실행
        if (!isTransitioning)
        {
            StartCoroutine(Fade(0f, true));
        }
    }

    // InitializeUI의 초기 색상 값 수정
    private void InitializeUI()
    {
        // 시작 시 검은 화면에서 시작하도록 설정
        if (fadePanel != null) fadePanel.color = Color.black;
        if (loadingContainer != null) loadingContainer.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
    }

    public void TransitionToScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true; // 전환 시작! OnSceneLoaded가 개입하지 못하도록 막음

        // 화면을 부드럽게 어둡게 만듭니다 (페이드 아웃).
        yield return StartCoroutine(Fade(1f, false));

        // 화면이 완전히 어두워진 후 로딩 UI를 켜고 애니메이션을 시작합니다.
        if (loadingContainer != null)
        {
            loadingContainer.SetActive(true);
            _loadingTextAnimation = StartCoroutine(AnimateLoadingText());
        }

        // 다음 프레임에 씬 로딩을 시작하도록 하여 렉 방지
        yield return null;

        // 백그라운드에서 다음 씬을 불러옵니다.
        float loadStartTime = Time.realtimeSinceStartup;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 최소 로딩 시간을 보장합니다. (로딩이 너무 빨라도 최소 시간만큼 기다림)
        float loadTime = Time.realtimeSinceStartup - loadStartTime;
        if (loadTime < minLoadingTime)
        {
            yield return new WaitForSecondsRealtime(minLoadingTime - loadTime);
        }

        // 로딩이 끝나면 로딩 UI를 숨깁니다.
        if (_loadingTextAnimation != null) StopCoroutine(_loadingTextAnimation);
        if (loadingContainer != null) loadingContainer.SetActive(false);

        // 화면을 다시 부드럽게 밝힙니다 (페이드 인).
        yield return StartCoroutine(Fade(0f, true));

        isTransitioning = false; // 전환 완료
    }

    public void GameOver()
    {
        if (isFading) return;
        StartCoroutine(GameOverCoroutine());
    }

    // 매 프레임마다 터치 입력을 확인합니다.
    private void Update()
    {
        // 게임오버 후 터치를 기다리는 상태이고, 마우스 왼쪽 버튼을 눌렀다면
        if (_isWaitingForGameOverTouch && Input.GetMouseButtonDown(0))
        {
            // 더 이상 터치를 기다리지 않도록 상태를 변경하고 메인 메뉴로 이동
            _isWaitingForGameOverTouch = false;
            GoToMainMenu();
        }
    }

    private IEnumerator GameOverCoroutine()
    {
        isFading = true;

        // 화면을 검게 만듭니다.
        yield return StartCoroutine(Fade(1f, false));

        // GameOver 텍스트 애니메이션을 실행합니다.
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateGameOverText());
        }

        // 씬을 바로 전환하는 대신, 터치를 기다리는 상태로 변경합니다.
        _isWaitingForGameOverTouch = true;
        SetInputBlocking(false); // 화면 전체의 입력 방지를 해제하여 터치가 가능하게 함
        isFading = false;
    }

    // GoToMainMenu 함수. 버튼 대신 Update에서 호출됩니다.
    private void GoToMainMenu()
    {
        if (isFading) return; // 중복 실행 방지

        Debug.Log("메인메뉴로 이동합니다.");

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
        TransitionToScene("MainMenuScene");
    }

    private IEnumerator Fade(float targetAlpha, bool notifyOnComplete)
    {
        if (fadePanel == null) yield break;

        SetInputBlocking(true); // 페이드 시작 시 항상 입력을 막음

        float duration = sceneTransitionFadeDuration;
        float startAlpha = fadePanel.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, targetAlpha, progress));
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, targetAlpha);

        // 페이드가 끝난 후, 목표 투명도에 따라 입력 상태를 최종 결정
        SetInputBlocking(targetAlpha >= 1f);

        // 페이드 인이 끝나고 notifyOnComplete가 true일 때만 이벤트 호출
        if (notifyOnComplete && targetAlpha == 0f)
        {
            OnSceneTransitionComplete?.Invoke();
        }
    }
    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 1;
        float timer = 0f;
        float interval = 0.4f; // 점이 바뀌는 간격 (0.4초)

        while (true)
        {
            timer += Time.unscaledDeltaTime;

            // 타이머가 정해진 간격(0.4초)을 넘으면 텍스트를 업데이트
            if (timer >= interval)
            {
                timer = 0f; // 타이머 초기화

                // 점 개수를 1 -> 2 -> 3 -> 1 순서로 반복
                dotCount = (dotCount % 3) + 1;
                string dots = new string('.', dotCount);

                if (loadingText != null)
                {
                    loadingText.text = baseText + dots;
                }
            }

            // 다음 프레임까지 대기
            yield return null;
        }
    }

    private IEnumerator AnimateGameOverText()
    {
        float timer = 0f;
        Color startColor = Color.white;
        Color endColor = Color.red;
        if (gameOverText == null) yield break;

        gameOverText.color = startColor;
        while (timer < gameOverTextAnimationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / gameOverTextAnimationDuration;
            gameOverText.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        gameOverText.color = endColor;
    }

    // 입력 차단 / 허용
    private void SetInputBlocking(bool block)
    {
        if (fadePanel != null)
        {
            fadePanel.raycastTarget = block;
        }
    }
}
