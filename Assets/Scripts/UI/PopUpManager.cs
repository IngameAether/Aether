using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static PopUpManager Instance { get; private set; }
    public event Action OnPopUpClosed;

    // 확인 팝업 콜백
    private Action<bool> _confirmCallback;

    [Header("UI 레퍼런스")]
    [SerializeField] private GameObject _popUpUICanvasPrefab;
    private GameObject _currentPopUpUICanvas;
    private RectTransform _backgroundOverlayRect;
    private Button _backgroundOverlayButton;

    private GameObject _currentActivePopUpGameObject = null;
    private CanvasGroup _currentActivePopUpCanvasGroup = null;

    [Header("PopUp Settings")]
    [SerializeField] private float _animationDuration = 0.3f;
    [SerializeField] private AnimationCurve _openAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _closeAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _isAnimating = false;
    private bool _shouldPauseGame = true;

    // 팝업이 열리기 전의 게임 속도를 저장할 변수
    private float _previousTimeScale = 1f;

    [Header("Registered PopUps")]
    public List<PopUpData> popUpList = new List<PopUpData>();
    private Dictionary<string, GameObject> _popUpPrefabs = new Dictionary<string, GameObject>();

    [System.Serializable]
    public struct PopUpData
    {
        public string popUpName;
        public GameObject popUpPrefabs;
    }

    public static void ResetInitialBookFlag()
    {
        // _initialBookShown 플래그 제거로 인해 이 함수는 더 이상 필요하지 않음
        // 하위 호환성을 위해 빈 함수로 유지
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (PopUpData data in popUpList)
        {
            if (!_popUpPrefabs.ContainsKey(data.popUpName))
            {
                _popUpPrefabs.Add(data.popUpName, data.popUpPrefabs);
            }
        }
        InitializePopUpSystem();
    }

    private void InitializePopUpSystem()
    {
        if (_popUpUICanvasPrefab != null && _currentPopUpUICanvas == null)
        {
            _currentPopUpUICanvas = Instantiate(_popUpUICanvasPrefab);
            DontDestroyOnLoad(_currentPopUpUICanvas);

            Canvas popUpCanvas = _currentPopUpUICanvas.GetComponent<Canvas>();
            if (popUpCanvas != null)
            {
                popUpCanvas.sortingOrder = 100;
            }

            _backgroundOverlayRect = _currentPopUpUICanvas.transform.Find("BackgroundOverlay")?.GetComponent<RectTransform>();
            _backgroundOverlayButton = _backgroundOverlayRect?.GetComponent<Button>();

            if (_backgroundOverlayRect != null && _backgroundOverlayButton != null)
            {
                _backgroundOverlayButton.onClick.AddListener(CloseCurrentPopUp);
            }

            _currentPopUpUICanvas.SetActive(false);
        }
    }

    public void OpenPopUpInGame(string popUpType)
    {
        OpenPopUpInternal(popUpType, true);
        AudioManager.Instance.PlaySFX(SfxType.PopUp_open);
    }

    public void OpenPopUpMainMenu(string popUpType)
    {
        OpenPopUpInternal(popUpType, false);
        AudioManager.Instance.PlaySFX(SfxType.PopUp_open);
    }

    // 확인 팝업 열기 (예/아니오 선택)
    public void OpenConfirmPopUp(string message, Action<bool> callback, bool pauseGame = false)
    {
        if (_currentActivePopUpGameObject != null || _isAnimating) return;

        _confirmCallback = callback;
        _shouldPauseGame = pauseGame;

        if (!_popUpPrefabs.ContainsKey("ConfirmPopUp"))
        {
            Debug.LogError("'ConfirmPopUp' 프리팹이 등록되어 있지 않습니다.");
            return;
        }

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(true);
            if (_backgroundOverlayRect != null)
            {
                _backgroundOverlayRect.gameObject.SetActive(true);
            }
        }

        _currentActivePopUpGameObject = Instantiate(_popUpPrefabs["ConfirmPopUp"], _currentPopUpUICanvas.transform, false);
        _currentActivePopUpGameObject.name = "ConfirmPopUp";

        // ConfirmPopUpUI 컴포넌트 초기화
        ConfirmPopUpUI confirmUI = _currentActivePopUpGameObject.GetComponent<ConfirmPopUpUI>();
        if (confirmUI != null)
        {
            confirmUI.Initialize(message, OnConfirmYes, OnConfirmNo);
        }

        _currentActivePopUpCanvasGroup = _currentActivePopUpGameObject.GetComponent<CanvasGroup>();
        if (_currentActivePopUpCanvasGroup == null)
        {
            Debug.LogError("ConfirmPopUp에 CanvasGroup 컴포넌트가 없습니다.");
            Destroy(_currentActivePopUpGameObject);
            _currentPopUpUICanvas.SetActive(false);
            return;
        }

        StartCoroutine(AnimatePopUpIn(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));
        AudioManager.Instance.PlaySFX(SfxType.PopUp_open);
    }

    // 확인 팝업 "예" 버튼 콜백
    private void OnConfirmYes()
    {
        _confirmCallback?.Invoke(true);
        _confirmCallback = null;
        CloseCurrentPopUp();
    }

    // 확인 팝업 "아니오" 버튼 콜백
    private void OnConfirmNo()
    {
        _confirmCallback?.Invoke(false);
        _confirmCallback = null;
        CloseCurrentPopUp();
    }

    private void OpenPopUpInternal(string popUpType, bool pauseGame)
    {
        // _isAnimating 플래그로 이미 중복 호출 방지가 되고 있음
        if (_currentActivePopUpGameObject != null || _isAnimating) return;

        _shouldPauseGame = pauseGame;

        if (!_popUpPrefabs.ContainsKey(popUpType))
        {
            Debug.LogError($"'{popUpType}' 이름의 팝업 프리팹이 등록되어 있지 않습니다.");
            return;
        }

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(true);
            if (_backgroundOverlayRect != null)
            {
                _backgroundOverlayRect.gameObject.SetActive(true);
            }
        }

        _currentActivePopUpGameObject = Instantiate(_popUpPrefabs[popUpType], _currentPopUpUICanvas.transform, false);
        _currentActivePopUpGameObject.name = popUpType;

        // PopUpManager는 특정 UI(MagicBookSelectionUI)를 알 필요가 없으므로 관련 코드를 제거하여 독립성을 높입니다.

        _currentActivePopUpCanvasGroup = _currentActivePopUpGameObject.GetComponent<CanvasGroup>();
        if (_currentActivePopUpCanvasGroup == null)
        {
            Debug.LogError($"팝업 '{popUpType}'에 CanvasGroup 컴포넌트가 없습니다. 추가해주세요.");
            Destroy(_currentActivePopUpGameObject);
            _currentPopUpUICanvas.SetActive(false);
            return;
        }

        StartCoroutine(AnimatePopUpIn(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));
        AudioManager.Instance.PlaySFX(SfxType.PopUp_open);
    }

    public void CloseCurrentPopUp()
    {
        if (_currentActivePopUpGameObject == null || _isAnimating) return;
        StartCoroutine(AnimatePopUpOut(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));
        AudioManager.Instance.PlaySFX(SfxType.PopUp_close);
    }

    private IEnumerator AnimatePopUpIn(CanvasGroup canvasGroup, Transform targetTransform)
    {
        _isAnimating = true;

        if (_shouldPauseGame)
        {
            _previousTimeScale = Time.timeScale; // 현재 배속을 저장
            Time.timeScale = 0f;
        }

        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one;

        canvasGroup.alpha = 0f;
        targetTransform.localScale = startScale;

        while (timer < _animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);
            float curveValue = _openAnimationCurve.Evaluate(t);
            canvasGroup.alpha = curveValue;
            targetTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveValue);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        targetTransform.localScale = endScale;
        _isAnimating = false;
    }

    // 중복되었던 함수 중 하나를 삭제하고, OnPopUpClosed 이벤트를 호출하는 버전만 남깁니다.
    private IEnumerator AnimatePopUpOut(CanvasGroup canvasGroup, Transform targetTransform)
    {
        _isAnimating = true;

        float timer = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;

        while (timer < _animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);
            float curveValue = _closeAnimationCurve.Evaluate(t);
            canvasGroup.alpha = 1f - curveValue;
            targetTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveValue);
            yield return null;
        }

        string popUpName = _currentActivePopUpGameObject.name;

        Destroy(_currentActivePopUpGameObject);
        _currentActivePopUpGameObject = null;
        _currentActivePopUpCanvasGroup = null;

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(false);
        }

        if (_shouldPauseGame)
        {
            Time.timeScale = _previousTimeScale;
        }

        _isAnimating = false;

        // 코루틴 가장 마지막에 이벤트를 호출합니다.
        OnPopUpClosed?.Invoke();
    }
}
