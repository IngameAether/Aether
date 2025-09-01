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

    private GameObject _currentActivePopUpGameObject = null; // 현재 활성화된 팝업 UI 오브젝트
    private CanvasGroup _currentActivePopUpCanvasGroup = null; // 현재 활성화된 팝업의 CanvasGroup

    [Header("PopUp Settings")]
    [SerializeField] private float _animationDuration = 0.3f; // 팝업 애니메이션 시간 (조금 줄여서 더 빠른 느낌을 줌)
    [SerializeField] private AnimationCurve _openAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _closeAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 닫기 애니메이션 커브

    private bool _isAnimating = false; // 애니메이션 중복 실행 방지 플래그

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

    public void OpenPopUp(string popUpType)
    {
        if (_currentActivePopUpGameObject != null || _isAnimating) // [수정] 애니메이션 중이면 실행 안 함
        {
            Debug.LogWarning("이미 팝업이 열려있거나 애니메이션이 진행 중입니다.");
            return;
        }

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
            Destroy(_currentActivePopUpGameObject); // [수정] 즉시 닫는 대신 정리
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

    // 닫기 메서드
    public void CloseCurrentPopUp()
    {
        if (_currentActivePopUpGameObject == null || _isAnimating) // 애니메이션 중이면 실행 안 함
        {
            return;
        }

        // 모든 코루틴을 중지하는 대신 닫기 애니메이션을 시작합니다.
        StartCoroutine(AnimatePopUpOut(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));
        Debug.Log("현재 팝업을 닫기 시작합니다.");
    }

    private IEnumerator AnimatePopUpIn(CanvasGroup canvasGroup, Transform targetTransform)
    {
        _isAnimating = true; // 애니메이션 시작

        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one;

        // 초기 상태 설정
        canvasGroup.alpha = 0f;
        targetTransform.localScale = startScale;
        canvasGroup.blocksRaycasts = false; // 애니메이션 중에는 클릭 방지
        canvasGroup.interactable = false;

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);
            float curveValue = _openAnimationCurve.Evaluate(t);

            canvasGroup.alpha = curveValue;
            targetTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveValue); // LerpUnclamped로 더 탄력있는 느낌 가능

            yield return null;
        }

        canvasGroup.alpha = 1f;
        targetTransform.localScale = endScale;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        _isAnimating = false; // 애니메이션 종료
        Debug.Log("팝업 열기 애니메이션 완료.");
    }

    // 닫기 애니메이션 코루틴
    private IEnumerator AnimatePopUpOut(CanvasGroup canvasGroup, Transform targetTransform)
    {
        _isAnimating = true; // 애니메이션 시작

        float timer = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero; // 점으로 작아지도록 목표 크기를 0으로 설정

        // 닫기 시작할 때 상호작용 비활성화
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);
            float curveValue = _closeAnimationCurve.Evaluate(t);

            // 알파 값 (투명도) 조정: 1에서 0으로
            canvasGroup.alpha = 1f - curveValue;

            // 스케일 (크기) 조정: 1에서 0으로
            targetTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveValue);

            yield return null;
        }

        // 애니메이션이 끝난 후 정리 작업
        Debug.Log("팝업 닫기 애니메이션 완료. 오브젝트를 파괴합니다.");
        Destroy(_currentActivePopUpGameObject);
        _currentActivePopUpGameObject = null;
        _currentActivePopUpCanvasGroup = null;

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(false);
        }

        _isAnimating = false; // 애니메이션 및 정리 작업 종료
    }
}
