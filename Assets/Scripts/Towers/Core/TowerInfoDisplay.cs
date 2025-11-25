using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 타워 정보와 사거리를 표시하는 UI 매니저 (더블클릭 방식)
/// </summary>
public class TowerInfoDisplay : MonoBehaviour
{
    [Header("Info Panel UI")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject infoPanelUI;
    [SerializeField] private TMP_Text towerStatsText;
    [SerializeField] private Button flipButton;
    [SerializeField] private Button lightButton;
    [SerializeField] private Button darkButton;
    [SerializeField] private Button reinforceButton;
    [SerializeField] private TMP_Text reinforceText;
    [SerializeField] private Button hideButton;
    [SerializeField] private GameObject towerIndicateImg;

    [Header("Position Settings")]
    [SerializeField] private Vector2 offsetFromTower = new Vector2(10f, 0f);
    [SerializeField] private Vector2 screenMargin = new Vector2(50f, 50f);

    private Tower currentSelectedTower;
    private Camera _camera;
    private RectTransform _infoPanelRect;
    private Color reinforceBtnColor;

    private void Start()
    {
        _camera = Camera.main;
        InitializeUI();
    }

    private void OnEnable()
    {
        ClickManager.Instance.OnTowerDoubleClicked += ShowTowerInfoAndRange;
        ClickManager.Instance.OnClickOutside += HideUI;

        Tower.OnTowerDestroyed += HideUI;
    }

    private void OnDisable()
    {
        ClickManager.Instance.OnTowerDoubleClicked -= ShowTowerInfoAndRange;
        ClickManager.Instance.OnClickOutside -= HideUI;

        Tower.OnTowerDestroyed -= HideUI;
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        infoPanelUI.SetActive(false);
        towerIndicateImg.SetActive(false);

        flipButton.onClick.AddListener(FlipSelectedTower);
        lightButton.onClick.AddListener(LightReinforce);
        darkButton.onClick.AddListener(DarkReinforce);
        reinforceButton.onClick.AddListener(ReinforceLevelUpgrade);
        hideButton.onClick.AddListener(HideUI);

        _infoPanelRect = infoPanelUI.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 타워 정보 표시 (더블클릭시에만 호출)
    /// </summary>
    private void ShowTowerInfoAndRange(Tower tower)
    {
        currentSelectedTower = tower;
        PositionInfoPanelNearTower(tower);
        ShowTowerInfo(tower);
        ShowTowerCircle(tower);
    }

    /// <summary>
    /// 정보창을 타워 밑에 위치시키는 함수
    /// </summary>
    private void PositionInfoPanelNearTower(Tower tower)
    {
        Vector3 towerWorldPos = tower.transform.position;
        Vector3 towerScreenPos = _camera.WorldToScreenPoint(towerWorldPos);
        Vector2 targetScreenPos = new Vector2(towerScreenPos.x, towerScreenPos.y) + offsetFromTower;
        Vector2 panelSize = _infoPanelRect.sizeDelta;

        float maxX = Screen.width - screenMargin.x - panelSize.x * 0.5f;

        // 화면 오른쪽으로 정보창 벗어나면 타워 왼쪽으로 위치
        if (targetScreenPos.x > maxX)
            targetScreenPos.x = towerScreenPos.x - offsetFromTower.x;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)mainCanvas.transform,
            targetScreenPos,
            mainCanvas.worldCamera,
            out Vector2 localPoint
        );

        _infoPanelRect.localPosition = localPoint;
    }

    /// <summary>
    /// 타워 정보 UI 업데이트
    /// </summary>
    private void ShowTowerInfo(Tower tower)
    {
        towerStatsText.text = $"공격력: {tower.Damage}\n" +
                              $"공격 속도: {tower.AttackSpeed}\n" +
                              $"치명타 확률: {tower.CriticalHit}\n" +
                              $"사거리: {tower.Range}";

        UpdateReinforceUI(tower);
        infoPanelUI.SetActive(true);
    }

    // 타워 강화 타입에 따라 UI 조정
    private void UpdateReinforceUI(Tower tower)
    { 
        if (tower.reinforceType == ReinforceType.None)
        {
            lightButton.gameObject.SetActive(true);
            darkButton.gameObject.SetActive(true);
            reinforceButton.gameObject.SetActive(false);
        }
        else
        {
            lightButton.gameObject.SetActive(false);
            darkButton.gameObject.SetActive(false);
            reinforceButton.gameObject.SetActive(true);
        }

        reinforceBtnColor = (tower.reinforceType == ReinforceType.Light) ?
            lightButton.colors.normalColor : darkButton.colors.normalColor;
        ColorBlock cb = reinforceButton.colors;
        cb.normalColor = reinforceBtnColor;
        reinforceButton.colors = cb;

        reinforceText.text = $"{tower.TowerName} + {tower.CurrentReinforceLevel}";
    }

    // 타워 가리키는 원 표시
    private void ShowTowerCircle(Tower tower)
    {
        Vector3 towerWorldPos = tower.transform.position;
        Vector3 towerScreenPos = _camera.WorldToScreenPoint(towerWorldPos);
        Vector2 targetScreenPos = new Vector2(towerScreenPos.x, towerScreenPos.y);

        towerIndicateImg.transform.position = targetScreenPos;
        towerIndicateImg.SetActive(true);
    }

    /// <summary>
    /// 선택된 타워 좌우반전
    /// </summary>
    private void FlipSelectedTower()
    {
        if (currentSelectedTower != null)
        {
            currentSelectedTower.FlipTower();
        }
    }

    /// <summary>
    /// UI 숨기기
    /// </summary>
    public void HideUI()
    {
        // infoPanelUI가 파괴되지 않고 존재할 때만 SetActive(false)를 호출합니다.
        if (infoPanelUI != null)
        {
            infoPanelUI.SetActive(false);
        }

        // towerIndicateImg가 파괴되지 않고 존재할 때만 SetActive(false)를 호출합니다.
        if (towerIndicateImg != null)
        {
            towerIndicateImg.SetActive(false);
        }

        currentSelectedTower = null;
    }

    // 빛 강화 선택
    private void LightReinforce()
    {
        if (currentSelectedTower != null)
        {
            var towerReinforce = currentSelectedTower.GetComponent<TowerReinforce>();
            towerReinforce.AssignReinforceType(ReinforceType.Light);
            towerReinforce.ReinforceTower();

            currentSelectedTower.type = "빛";
            UpdateReinforceUI(currentSelectedTower);
        }
    }

    // 어둠 강화 선택
    private void DarkReinforce()
    {
        if (currentSelectedTower != null)
        {
            var towerReinforce = currentSelectedTower.GetComponent<TowerReinforce>();
            towerReinforce.AssignReinforceType(ReinforceType.Dark);
            towerReinforce.ReinforceTower();

            currentSelectedTower.type = "어둠";
            UpdateReinforceUI(currentSelectedTower);
        }
    }

    // 강화 레벨업
    private void ReinforceLevelUpgrade()
    {
        if (currentSelectedTower != null)
        {
            var towerReinforce = currentSelectedTower.GetComponent<TowerReinforce>();
            if (towerReinforce != null)
            {
                // 강화 실행 '요청' (이 부분은 그대로)
                towerReinforce.ReinforceTower();
            }

            // 강화 레벨을 Tower.cs에서 직접 가져오기
            int reinforce = currentSelectedTower.CurrentReinforceLevel;

            // 텍스트 업데이트 (tower.type 대신 tower.TowerName 사용 권장)
            reinforceText.text = $"{currentSelectedTower.TowerName} + {reinforce + 1}"; // reinforce가 0부터 시작하므로 +1 해줘야 UI에 1레벨로 표시됩니다.
        }
    }
}
