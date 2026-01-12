using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 타워 정보와 사거리를 표시하는 UI 매니저 (더블클릭 방식)
/// </summary>
public class TowerInfoDisplay : MonoBehaviour
{
    // 어디서든 접근하기 쉽게 싱글톤 패턴 적용
    public static TowerInfoDisplay Instance { get; private set; }

    [Header("Info Panel UI")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject infoPanelUI;
    [SerializeField] private TMP_Text towerStatsText;

    [Header("Indicators")]
    [SerializeField] private GameObject towerIndicateImg;
    [SerializeField] private GameObject towerRangeIndicator;

    [Header("Position Settings")]
    [SerializeField] private Vector2 offsetFromTower = new Vector2(0f, 250f);

    // 화면 밖으로 나가는 것을 방지하기 위한 여백
    [SerializeField] private Vector2 screenMargin = new Vector2(50f, 50f);

    private Tower currentSelectedTower;
    private Camera _camera;
    private RectTransform _infoPanelRect;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _camera = Camera.main;
        InitializeUI();
    }

    private void Update()
    {
        // 정보창이 켜져 있고 타워가 선택된 상태라면, 매 프레임 위치를 갱신하여 따라다니게 함
        if (infoPanelUI.activeSelf && currentSelectedTower != null)
        {
            PositionInfoPanelNearTower(currentSelectedTower);

            // 사거리 표시도 타워를 따라다녀야 함
            if (towerRangeIndicator.activeSelf)
            {
                towerRangeIndicator.transform.position = currentSelectedTower.transform.position;
            }
        }
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        infoPanelUI.SetActive(false);
        if(towerIndicateImg != null)
            towerIndicateImg.SetActive(false);
        if (towerRangeIndicator != null)
            towerRangeIndicator.SetActive(false);

        _infoPanelRect = infoPanelUI.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 정보창을 타워 밑에 위치시키는 함수
    /// </summary>
    private void PositionInfoPanelNearTower(Tower tower)
    {
        Vector3 towerWorldPos = tower.transform.position;
        Vector3 towerScreenPos = _camera.WorldToScreenPoint(towerWorldPos);

        // 타워 위치에서 Y축으로 offset만큼 위로 올림
        Vector2 targetScreenPos = new Vector2(towerScreenPos.x, towerScreenPos.y) + offsetFromTower;

        // 화면 밖으로 나가지 않도록 Clamp(가두기) 처리
        // 정보창의 크기 절반을 구합니다.
        Vector2 panelSize = _infoPanelRect.sizeDelta;
        float halfWidth = panelSize.x * 0.5f;
        float halfHeight = panelSize.y * 0.5f;

        // X축 제한 (화면 좌우 밖으로 안 나가게)
        float minX = screenMargin.x + halfWidth;
        float maxX = Screen.width - screenMargin.x - halfWidth;
        targetScreenPos.x = Mathf.Clamp(targetScreenPos.x, minX, maxX);

        // Y축 제한 (화면 위 밖으로 안 나가게)
        // 위쪽으로 너무 올라가서 잘리면 안 되므로 상단 한계점 설정
        float maxY = Screen.height - screenMargin.y - halfHeight;
        // 보통 드래그 중인 타워는 화면 중앙~하단에 있으므로 위쪽 제한(Clamp)만
        targetScreenPos.y = Mathf.Clamp(targetScreenPos.y, 0, maxY);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)mainCanvas.transform,
            targetScreenPos,
            mainCanvas.worldCamera,
            out Vector2 localPoint
        );

        _infoPanelRect.localPosition = localPoint;
    }

    /// <summary>
    /// 외부(TowerDragSale)에서 호출할 정보 표시 함수
    /// </summary>
    public void ShowTowerInfo(Tower tower)
    {
        currentSelectedTower = tower;

        // 텍스트 업데이트
        UpdateTowerStatsText(tower);

        // 위치 잡기 (Update에서도 계속 호출됨)
        PositionInfoPanelNearTower(tower);

        // UI 켜기
        infoPanelUI.SetActive(true);
        ShowRange(tower);
    }

    private void UpdateTowerStatsText(Tower tower)
    {
        towerStatsText.text = $"<size=120%>{tower.TowerName}</size>\n\n" +
                              $"공격력: {tower.Damage}\n" +
                              $"공격 속도: {tower.AttackSpeed}\n" +
                              $"치명타 확률: {tower.CriticalHit}\n" +
                              $"사거리: {tower.Range}";
    }

    private void ShowRange(Tower tower)
    {
        if(towerRangeIndicator == null)
        {
            Debug.LogWarning("Tower Range Indicator가 할당되지 않았습니다.");
            return;
        }

        towerRangeIndicator.transform.position = tower.transform.position;

        // 사거리에 맞게 크기 조정
        float range = tower.Range;
        towerRangeIndicator.transform.localScale = Vector3.one * (range * 1.85f);

        towerRangeIndicator.SetActive(true);
    }

    /// <summary>
    /// UI 숨기기
    /// </summary>
    public void HideUI()
    {
        // infoPanelUI가 파괴되지 않고 존재할 때만 SetActive(false)를 호출합니다.
        if (infoPanelUI != null) infoPanelUI.SetActive(false);

        // towerIndicateImg가 파괴되지 않고 존재할 때만 SetActive(false)를 호출합니다.
        if (towerIndicateImg != null) towerIndicateImg.SetActive(false);

        // 사거리 표시 끄기
        if (towerRangeIndicator != null) towerRangeIndicator.SetActive(false); 

        currentSelectedTower = null;
    }
}
