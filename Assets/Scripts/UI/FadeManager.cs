using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static FadeManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image fadePanel; // ȭ�� ��ȯ�� 
    [SerializeField] private TextMeshProUGUI gameOverText; // ������
    [SerializeField] private GameObject gameOverButtonContainer; // "�ٽ��ϱ�", "���θ޴��� ������"
    [SerializeField] private Button restartButton; // �ٽ��ϱ�
    [SerializeField] private Button mainMenuButton; // ���θ޴��� ������

    [Header("Fade Settings")]
    [SerializeField] private float sceneTransitionFadeDuration = 3.0f; // �� ��ȯ ���̵�ð�
    [SerializeField] private float gameOverFadeDuration = 3.0f; // ���� ���� ���̵� �ƿ� �ð�
    [SerializeField] private float blackScreenHoldDuration = 3.0f; // ���� ȭ�� ���� �ð�
   
    private Canvas fadeCanvas; // ���̵� �г��� ���� ĵ����
    private bool isFading = false; // ���̵� ������ Ȯ��

    void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı����� �ʵ���
        }
        else
        {
            Destroy(gameObject); // �ν��Ͻ� ������ �ڱ��ڽ��� �ı�
            return;
        }

        // ���� �˻�
        if (fadePanel == null)
        {
            Debug.LogError("fadePanel�� ������� ����");
            enabled = false;
            return;
        }

        if(gameOverText == null) // gameOverText�� null���� ������ Ȯ��
        {
            Debug.LogError("FadeManager: Game Over Text�� ������� �ʾҽ��ϴ�. �ν����Ϳ��� �������ּ���.");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
            return;
        }

        if(gameOverButtonContainer == null)
        {
            Debug.LogError("GameOverButtonContainer�� ������� ����");
            enabled = false;
            return;
        }

        if(restartButton == null)
        {
            Debug.LogError("restartButton�� ������� ����");
            enabled = false;
            return;
        }

        if (mainMenuButton == null)
        {
            Debug.LogError("MainMenuButton�� ������� ����");
            enabled = false;
            return;
        }

        fadeCanvas = fadePanel.GetComponentInParent<Canvas>();
        if (fadeCanvas == null)
        {
            Debug.LogError("Fade Panel�� Canvas ���ο� ����");
            enabled = false;
            return;
        }

        // �ʱ⿡�� ���̵� �г� �����ϰ� �ϱ�
        fadePanel.gameObject.SetActive(true); // Ȱ��ȭ�� ������ �����Ѱ���
        Color panelColor = fadePanel.color;
        panelColor.a = 0f;
        fadePanel.color = panelColor;

        gameOverText.gameObject.SetActive(false); // ���ӿ��� �ؽ�Ʈ�� ��Ȱ��ȭ
        gameOverButtonContainer.SetActive(false); // ���ӿ��� ��ư�� ��Ȱ��ȭ

        SetInputBlocking(false); // �ʱ⿡�� �Է� ���
        isFading = false;

        // ���ӿ��� ��ư ����
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // �� ��ȯ ���̵� (���̵� �ƿ� -> �� �� �ε� -> �� �� ���̵���)
    public void TransitionToScene(string sceneName)
    {
        if (isFading) return;
        StartCoroutine(SceneTransitionCoroutine(sceneName));
    }

    private IEnumerator SceneTransitionCoroutine(string sceneName)
    {
        isFading = true;
        SetInputBlocking(true); // �� ��ȯ �� �Է� ����

        // 1. ���̵� �ƿ�
        yield return StartCoroutine(Fade(1f, sceneTransitionFadeDuration));

        // 2. ���� ȭ�� ����
        yield return new WaitForSeconds(blackScreenHoldDuration);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // ���ο� ���� ���� ������ �Ͻ� ����

        while (!asyncLoad.isDone)
        {
            if(asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        yield return StartCoroutine(Fade(0f, sceneTransitionFadeDuration));

        SetInputBlocking(false);
        isFading = false;
    }

    // ���� ���� ���̵�
    public void GameOver()
    {
        if (isFading) return;
        StartCoroutine(GameOverCoroutine());
    }

    private IEnumerator GameOverCoroutine()
    {
        isFading = true;
        SetInputBlocking(true);

        // 1. ���̵� �ƿ�
        yield return StartCoroutine(Fade(1f, gameOverFadeDuration));

        // 2. GameOver �ؽ�Ʈ ǥ��
        gameOverText.gameObject.SetActive(true);
        gameOverButtonContainer.SetActive(true);

        SetInputBlocking(false); // ��ư Ŭ�� �����ϵ���
        isFading = false; // ���� ������ ��ư Ŭ������ ����
    }

    private void RestartGame()
    {
        if (isFading) return; // �̹� ���̵� ���̸� ����
        Debug.Log("������ ó������ �ٽ� �����մϴ�.");
        HideGameOverUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        Debug.Log("���θ޴��� �̵��մϴ�.");
        HideGameOverUI();
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
            timer += Time.deltaTime;
            float progress = timer / duration;
            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadePanel.color = currentColor;
            yield return null; // ���� �����ӱ��� ���
        }
        currentColor.a = targetAlpha;
        fadePanel.color = currentColor;
    }
    // �Է� ���� / ���
    private void SetInputBlocking(bool block)
    {
        if (fadePanel != null)
        {
            // ���̵� �г��� Ŭ�� �̺�Ʈ ������
            fadePanel.raycastTarget = block;
        }
    }
}
