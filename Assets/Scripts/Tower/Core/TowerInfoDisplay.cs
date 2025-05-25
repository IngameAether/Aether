using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 타워 정보와 사거리를 표시하는 UI 매니저
/// </summary>
public class TowerInfoDisplay : MonoBehaviour
{
    [Header("Info Panel UI")]
    [SerializeField] private GameObject infoPanelUI;
    [SerializeField] private TMP_Text towerNameText;
    [SerializeField] private TMP_Text towerDescriptionText;
    [SerializeField] private TMP_Text towerStatsText;
    [SerializeField] private Button flipButton;
    [SerializeField] private Button sellButton; // (나중에 추가)
    
    [Header("Range Display")]
    [SerializeField] private GameObject rangeIndicator;
    [SerializeField] private SpriteRenderer rangeRenderer;
    
    private Tower currentSelectedTower;
    private Camera _camera;
    
    private void Start()
    {
        _camera = Camera.main;
        InitializeUI();
    }
    
    /// <summary>
    /// 빈 공간 클릭 감지용
    /// </summary>
    private void Update()
    {
        // 마우스 왼쪽 버튼 클릭했을 때
        if (!Input.GetMouseButtonDown(0)) return;
        
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
        // 타워가 아닌 다른 곳을 클릭했으면 UI 숨기기
        if (hit.collider == null || hit.collider.GetComponent<Tower>() == null)
        {
            HideUI();
        }
    }
    
    private void OnEnable()
    {
        Tower.OnTowerClicked += ShowTowerInfoAndRange;
    }

    private void OnDisable()
    {
        Tower.OnTowerClicked -= ShowTowerInfoAndRange;
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        infoPanelUI.SetActive(false);
        rangeIndicator.SetActive(false);
            
        flipButton.onClick.AddListener(FlipSelectedTower);
    }
    
    /// <summary>
    /// 타워 정보 표시
    /// </summary>
    private void ShowTowerInfoAndRange(Tower tower)
    {
        currentSelectedTower = tower;
            
        // 타워 정보 표시
        ShowTowerInfo(tower);
        
        // 사거리 표시
        ShowRange(tower);
    }
    
    /// <summary>
    /// 타워 정보 UI 업데이트
    /// </summary>
    private void ShowTowerInfo(Tower tower)
    {
        var towerSetting = tower.GetTowerSetting();
        
        towerNameText.text = towerSetting.name;
        towerDescriptionText.text = towerSetting.description;
            
        towerStatsText.text = $"등급: {towerSetting.rank}\n" +
                              $"공격력: {towerSetting.damage}\n" +
                              $"공격 딜레이: {towerSetting.attackDelay}초\n" +
                              $"사거리: {towerSetting.range}";
        
        infoPanelUI.SetActive(true);
    }
    
    /// <summary>
    /// 사거리 표시
    /// </summary>
    private void ShowRange(Tower tower)
    {
        rangeIndicator.transform.position = tower.GetPosition();
        
        // 사거리에 맞게 크기 조정
        float range = tower.GetTowerSetting().range;
        rangeIndicator.transform.localScale = Vector3.one * range * 2f;
        
        rangeIndicator.SetActive(true);
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
        infoPanelUI.SetActive(false);
        rangeIndicator.SetActive(false);
            
        currentSelectedTower = null;
    }
}
