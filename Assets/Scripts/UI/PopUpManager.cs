using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static PopUpManager Instance { get; private set; }

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
    // 메인 메뉴 등 게임을 멈추면 안 되는 씬을 위한 변수
    private bool _shouldPauseGame = true;

    [Header("Registered PopUps")]
    public List<PopUpData> popUpList = new List<PopUpData>();
    private Dictionary<string, GameObject> _popUpPrefabs = new Dictionary<string, GameObject>();

    [System.Serializable]
    public struct PopUpData
    {
        public string popUpName;
        public GameObject popUpPrefabs;
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
            else
            {
                Debug.LogWarning($"팝업 '{data.popUpName}'이(가) 이미 등록되어 있습니다. 중복 등록을 확인하세요.");
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

            if (_backgroundOverlayRect == null || _backgroundOverlayButton == null)
            {
                Debug.LogError("PopUpUICanvas 프리팹에서 'BackgroundOverlay' 또는 그 안에 Button 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            _backgroundOverlayButton.onClick.AddListener(CloseCurrentPopUp);
            _currentPopUpUICanvas.SetActive(false);
        }
        else if (_popUpUICanvasPrefab == null)
        {
            Debug.LogError("PopUpUICanvas 프리팹이 PopUpManager에 할당되지 않았습니다.");
        }
    }

    // 인게임용 함수: 인게임 설정 버튼에 이 함수를 연결하세요.
    public void OpenPopUpInGame(string popUpType)
    {
        // 게임을 항상 멈추도록(true) 내부 함수를 호출합니다.
        OpenPopUpInternal(popUpType, true);
    }

    // 메인 메뉴용 함수: 메인 메뉴 설정 버튼에 이 함수를 연결하세요.
    public void OpenPopUpMainMenu(string popUpType)
    {
        // 게임을 멈추지 않도록(false) 내부 함수를 호출합니다.
        OpenPopUpInternal(popUpType, false);
    }
    // 이 함수는 인스펙터에 노출되지 않고 위 두 함수에 의해서만 호출됩니다.
    private void OpenPopUpInternal(string popUpType, bool pauseGame)
    {
        if (_currentActivePopUpGameObject != null || _isAnimating)
        {
            Debug.LogWarning("이미 팝업이 열려있거나 애니메이션이 진행 중입니다.");
            return;
        }

        _shouldPauseGame = pauseGame;

        if (!_popUpPrefabs.ContainsKey(popUpType))
        {
            Debug.LogError($"'{popUpType}' 이름의 팝업 프리팹이 등록되어 있지 않습니다.");
            return;
        }

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(true);
            if (_backgroundOverlayRect != null && !_backgroundOverlayRect.gameObject.activeSelf)
            {
                _backgroundOverlayRect.gameObject.SetActive(true);
            }
        }

        _currentActivePopUpGameObject = Instantiate(_popUpPrefabs[popUpType], _currentPopUpUICanvas.transform, false);
        _currentActivePopUpGameObject.name = popUpType;

        _currentActivePopUpCanvasGroup = _currentActivePopUpGameObject.GetComponent<CanvasGroup>();
        if (_currentActivePopUpCanvasGroup == null)
        {
            Debug.LogError($"팝업 '{popUpType}'에 CanvasGroup 컴포넌트가 없습니다. 추가해주세요.");
            Destroy(_currentActivePopUpGameObject);
            _currentPopUpUICanvas.SetActive(false);
            return;
        }

        RectTransform rectTransform = _currentActivePopUpGameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(324, 324);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        StartCoroutine(AnimatePopUpIn(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));
        Debug.Log($"팝업 '{popUpType}'이(가) 열립니다.");
    }

    public void CloseCurrentPopUp()
    {
        if (_currentActivePopUpGameObject == null || _isAnimating)
        {
            return;
        }

        StartCoroutine(AnimatePopUpOut(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));
        Debug.Log("현재 팝업을 닫기 시작합니다.");
    }

    private IEnumerator AnimatePopUpIn(CanvasGroup canvasGroup, Transform targetTransform)
    {
        _isAnimating = true;

        // 팝업이 열리기 시작할 때 게임을 멈춤
        if (_shouldPauseGame)
        {
            Time.timeScale = 0f;
        }

        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one;

        canvasGroup.alpha = 0f;
        targetTransform.localScale = startScale;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        while (timer < _animationDuration)
        {
            // Time.deltaTime 대신 Time.unscaledDeltaTime 사용
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);
            float curveValue = _openAnimationCurve.Evaluate(t);

            canvasGroup.alpha = curveValue;
            targetTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveValue);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        targetTransform.localScale = endScale;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        _isAnimating = false;
        Debug.Log("팝업 열기 애니메이션 완료.");
    }

    private IEnumerator AnimatePopUpOut(CanvasGroup canvasGroup, Transform targetTransform)
    {
        _isAnimating = true;

        float timer = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        while (timer < _animationDuration)
        {
            // Time.deltaTime 대신 Time.unscaledDeltaTime 사용
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);
            float curveValue = _closeAnimationCurve.Evaluate(t);

            canvasGroup.alpha = 1f - curveValue;
            targetTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveValue);

            yield return null;
        }

        Debug.Log("팝업 닫기 애니메이션 완료. 오브젝트를 파괴합니다.");
        Destroy(_currentActivePopUpGameObject);
        _currentActivePopUpGameObject = null;
        _currentActivePopUpCanvasGroup = null;

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(false);
        }

        // 팝업이 완전히 닫힌 후 게임 시간을 되돌림
        if (_shouldPauseGame)
        {
            Time.timeScale = 1f;
        }

        _isAnimating = false;
    }
}
