using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
                                                               // 애니메이션 및 상호작용 제어용

    [Header("PopUp Settings")]
    [SerializeField] private float _animationDuration = 1f; // 팝업 열리는 애니메이션 시간
    [SerializeField] private AnimationCurve _openAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 점점 빠르게 열림

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
        // 인스턴스 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 씬 전환 시 파괴되지 않도록
        DontDestroyOnLoad(gameObject);

        // 팝업 프리팹들을 딕셔너리에 등록합니다.
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
        Debug.Log("PopUpManager 인스턴스가 성공적으로 초기화되었습니다. 씬 전환 시 유지됩니다.");
        // 팝업들을 관리한 초기화 로직을 이곳에 추가할 수 있음
        InitializePopUpSystem();
    }

    private void InitializePopUpSystem()
    {
        if (_popUpUICanvasPrefab != null && _currentPopUpUICanvas == null)
        {
            _currentPopUpUICanvas = Instantiate(_popUpUICanvasPrefab);
            DontDestroyOnLoad(_currentPopUpUICanvas); // 캔버스도 파괴되지 않도록 설정

            Canvas popUpCanvas = _currentPopUpUICanvas.GetComponent<Canvas>();
            if (popUpCanvas != null)
            {
                popUpCanvas.sortingOrder = 100;
            }
            _backgroundOverlayRect = _currentPopUpUICanvas.transform.Find("BackgroundOverlay")?.GetComponent<RectTransform>();
            _backgroundOverlayButton = _backgroundOverlayRect?.GetComponent<Button>();

            if (_backgroundOverlayRect == null || _backgroundOverlayButton == null)
            {
                Debug.LogError("PopUpUICanvas 프리팹에서 'BackgroundOverlay' 또는 그 안에 Button 컴포넌트를 찾을 수 없습니다. UI 구조를 확인해주세요.");
                return;
            }
            // 배경 오버레이 클릭 시 팝업 닫기 이벤트 등록
            _backgroundOverlayButton.onClick.AddListener(CloseCurrentPopUp);
            // 초기에는 팝업 UI Canvas를 비활성화합니다.
            _currentPopUpUICanvas.SetActive(false);
            Debug.Log("PopUpManager의 UI 시스템 설정이 완료되었습니다.");
        }
        else if (_popUpUICanvasPrefab == null)
        {
            Debug.LogError("PopUpUICanvas 프리팹이 PopUpManager에 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
        }
    }

    public void OpenPopUp(string popUpType)
    {
        // 한 번에 하나의 팝업만 허용하므로, 이미 팝업이 열려있다면 경고 메시지 출력
        if (_currentActivePopUpGameObject != null)
        {
            Debug.LogWarning($"이미 팝업 '{_currentActivePopUpGameObject.name}'이(가) 열려 있습니다. 새로운 팝업 '{popUpType}'을 열기 전에 기존 팝업을 닫아주세요.");
            return;
        }
        // 팝업 프리팹이 딕셔너리에 등록되어 있는지 확인
        if (!_popUpPrefabs.ContainsKey(popUpType))
        {
            Debug.LogError($"'{popUpType}' 이름의 팝업 프리팹이 등록되어 있지 않습니다. PopUpManager의 'PopUp List'를 확인해주세요.");
            return;
        }
        // 팝업 UI Canvas 활성화
        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(true);
            //  BackgroundOverlay를 명시적으로 활성화합니다.
            if (_backgroundOverlayRect != null && !_backgroundOverlayRect.gameObject.activeSelf)
            {
                _backgroundOverlayRect.gameObject.SetActive(true);
            }
        }
        // 새 팝업 인스턴스 생성 및 준비
        _currentActivePopUpGameObject = Instantiate(_popUpPrefabs[popUpType], _currentPopUpUICanvas.transform, false);
        _currentActivePopUpGameObject.name = popUpType;

        CanvasGroup canvasGroup = _currentActivePopUpGameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError($"팝업 '{popUpType}'에 CanvasGroup 컴포넌트가 없습니다.");
            CloseCurrentPopUp();
            return;
        }

        RectTransform rectTransform = _currentActivePopUpGameObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(324, 324); // 원하는 설정창 크기 조절
            rectTransform.anchoredPosition = Vector2.zero; // 위치를 (0,0)으로 초기화
            rectTransform.localScale = Vector3.one; // 스케일을 (1,1,1)로 초기화
            rectTransform.localRotation = Quaternion.identity; // 회전 초기화
        }
        _currentActivePopUpCanvasGroup = canvasGroup;

        // 초기 상태 설정: 투명하고 매우 작게 시작 (점점 커지면서 나타남)
        _currentActivePopUpCanvasGroup.alpha = 0f;
        _currentActivePopUpCanvasGroup.blocksRaycasts = false; // 애니메이션 중에는 클릭 방지
        _currentActivePopUpCanvasGroup.interactable = false;

        _currentActivePopUpGameObject.transform.localScale = Vector3.one * 0.5f; // 초기 스케일 (0.5배)

        // 열리는 애니메이션 코루틴 시작
        StartCoroutine(AnimatePopUpIn(_currentActivePopUpCanvasGroup, _currentActivePopUpGameObject.transform));

        Debug.Log($"팝업 '{popUpType}'이(가) 열립니다.");
    }

    public void CloseCurrentPopUp()
    {
        if (_currentActivePopUpGameObject == null)
        {
            return;
        }

        StopAllCoroutines();

        Destroy(_currentActivePopUpGameObject);
        _currentActivePopUpGameObject = null;
        _currentActivePopUpCanvasGroup = null;

        if (_currentPopUpUICanvas != null)
        {
            _currentPopUpUICanvas.SetActive(false);
        }
        Debug.Log("현재 팝업이 닫혔습니다");
    }

    private IEnumerator AnimatePopUpIn(CanvasGroup canvasGroup, Transform targetTransform)
    {
        float timer = 0f;
        Vector3 startScale = Vector3.one * 0.5f; // 초기 크기
        Vector3 endScale = Vector3.one;          // 최종 크기

        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _animationDuration);

            // 애니메이션 커브를 적용하여 '점점 빠르게' 효과를 만듭니다.
            float curveValue = _openAnimationCurve.Evaluate(t);

            // 알파 값 (투명도) 조정
            canvasGroup.alpha = curveValue;

            // 스케일 (크기) 조정
            targetTransform.localScale = Vector3.Lerp(startScale, endScale, curveValue);

            yield return null;
        }
        // 애니메이션 완료 후 최종 상태 설정
        canvasGroup.alpha = 1f;
        targetTransform.localScale = Vector3.one;

        // 애니메이션 완료 후 상호작용 가능하도록 설정
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        Debug.Log("팝업 열기 애니메이션 완료.");
    }

    public void OnBackgroundOverlayClick()
    {
        if (_currentActivePopUpGameObject == null) return;

        Vector2 screenPoint = Input.mousePosition;
        RectTransform popUpRectTransform = _currentActivePopUpGameObject.GetComponent<RectTransform>();

        if (!RectTransformUtility.RectangleContainsScreenPoint(popUpRectTransform, screenPoint, null))
        {
            CloseCurrentPopUp();
            Debug.Log("팝업 바깥 영역 클릭: 팝업 닫음.");
        }
        else
        {
            Debug.Log("팝업 내부 영역 클릭: 팝업 유지.");
        }
    }
}
