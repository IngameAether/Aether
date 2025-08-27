using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ElementType
{
    Fire, // 불 원소
    Water, // 물 원소
    Earth, // 땅 원소
    Air, // 공기 원소
    None, // 원소 없음
    Tower // 타워가 건설된 타일
}

public class TowerCombiner : MonoBehaviour
{
    public static TowerCombiner Instance { get; private set; }

    [Header("타워 데이터")]
    [SerializeField] private TowerData[] allTowerData; // 모든 타워 데이터

    [Header("기본 타워 프리팹")]
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject airPrefab;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] public Transform towerParent;

    private Dictionary<string, TowerData> _towerDataMap;
    private Dictionary<ElementType, GameObject> _elementTowerMap;

    // 선택된 아이템들 (원소 + 타워)
    private readonly List<ISelectable> _selectedItems = new();
    private TowerSpriteController _towerSpriteController;
    private SaleController _saleController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _towerSpriteController = FindObjectOfType<TowerSpriteController>();
        _saleController = FindObjectOfType<SaleController>();
    }

    private void Start()
    {
        InitializeTowerMapping();
        BuildTowerDataMap();
    }

    /// <summary>
    /// 타워 데이터 맵 구성 (성능 최적화)
    /// </summary>
    private void BuildTowerDataMap()
    {
        _towerDataMap = new Dictionary<string, TowerData>();

        foreach (var data in allTowerData)
        {
            _towerDataMap.TryAdd(data.ID, data);
        }

        Debug.Log($"타워 데이터 맵 구성 완료: {_towerDataMap.Count}개 타워");
    }

    private void InitializeTowerMapping()
    {
        _elementTowerMap = new Dictionary<ElementType, GameObject>
        {
            { ElementType.Fire, firePrefab },
            { ElementType.Water, waterPrefab },
            { ElementType.Earth, earthPrefab },
            { ElementType.Air, airPrefab }
        };
    }

    /// <summary>
    /// 통합 선택 로직 (원소/타워 모두 처리)
    /// </summary>
    public void SelectItem(ISelectable item)
{
    if (item == null) return;

    var itemTile = item.GetTile();
    if (itemTile == null)
    {
        Debug.LogWarning("선택된 아이템에 유효한 타일 정보가 없습니다.");
        return;
    }

    // 첫 번째 선택
    if (_selectedItems.Count == 0)
    {
        _selectedItems.Add(item);
        item.SetSelected(true);
        Debug.Log($"첫 번째 아이템 선택: {item.GetElementType()} Lv{item.GetLevel()}. 현재 선택: {_selectedItems.Count}/3");
    }
    // 이미 아이템이 선택되어 있는 경우
    else
    {
        // 이미 선택된 아이템을 다시 클릭한 경우만 무시
        if (_selectedItems.Contains(item))
        {
            Debug.Log("이미 선택된 아이템입니다. 중복 선택을 무시합니다.");
            return;
        }

        // 3개가 아직 안 찼으면 타입/레벨 상관없이 계속 선택
        if (_selectedItems.Count < 3)
        {
            _selectedItems.Add(item);
            item.SetSelected(true);
            Debug.Log($"아이템 추가 선택: {item.GetElementType()} Lv{item.GetLevel()}. 현재 선택: {_selectedItems.Count}/3");
        }
        // 3개가 이미 선택된 상태에서 추가 클릭시
        else
        {
            Debug.LogWarning("이미 3개의 아이템이 선택되었습니다. 조합을 진행하거나 기다려주세요.");
            return;
        }
    }

    // 3개가 모두 선택되면 즉시 조합 시도
    if (_selectedItems.Count == 3)
    {
        TryCombination();
    }
}

/// <summary>
/// 조합 시도 (3개 선택 완료 후 판정)
/// </summary>
private void TryCombination()
{
    if (_selectedItems.Count != 3) return;

    var item1 = _selectedItems[0];
    var item2 = _selectedItems[1];
    var item3 = _selectedItems[2];

    // 모든 아이템이 같은 타입과 레벨인지 확인
    bool allSameType = (item1.GetElementType() == item2.GetElementType() &&
                       item2.GetElementType() == item3.GetElementType());
    bool allSameLevel = (item1.GetLevel() == item2.GetLevel() &&
                        item2.GetLevel() == item3.GetLevel());

    if (allSameType && allSameLevel)
    {
        var elementType = item1.GetElementType();
        var level = item1.GetLevel();

        Debug.Log($"🎉 조합 성공! {elementType} Lv{level} 아이템 3개가 조합됩니다.");

        switch (level)
        {
            case 0: // 원소 → Lv1 타워
                CreateUpgradedTower(elementType, 1, false);
                break;
            case 1: // Lv1 타워 → Lv2 타워
                CreateUpgradedTower(elementType, 2, false);
                break;
            case 2: // Lv2 타워 → Lv3 타워
                CreateUpgradedTower(elementType, 3, false);
                break;
            case 3: // Lv3 타워 → Lv4 타워
                TryCreateLevel4Tower();
                break;
            default:
                Debug.LogWarning($"지원되지 않는 레벨: {level}");
                ClearSelection();
                break;
        }
    }
    else
    {
        // 원소와 동일한 실패 메시지
        Debug.LogWarning($"❌ 조합 실패! 같은 타입/레벨의 아이템 3개가 아닙니다.");
        Debug.LogWarning($"선택된 아이템들: " +
            $"{item1.GetElementType()} Lv{item1.GetLevel()}, " +
            $"{item2.GetElementType()} Lv{item2.GetLevel()}, " +
            $"{item3.GetElementType()} Lv{item3.GetLevel()}");
        ClearSelection();
    }
}

    /// <summary>
    /// 랜덤 Lv1 타워 생성 (기존 호환성 유지)
    /// </summary>
    public void CreateRandomLevel1Tower(TileInteraction tileInteraction, Tile tile)
    {
        // 랜덤 원소 타입 선택
        var keys = _elementTowerMap.Keys.ToArray();
        int randomIndex = Random.Range(0, keys.Length);
        var randomElementType = keys[randomIndex];

        CreateUpgradedTower(randomElementType, 1, true, tileInteraction, tile);
    }

    /// <summary>
    /// 통합 타워 생성 로직 (isRandom 매개변수 추가)
    /// </summary>
    /// <param name="elementType">원소 타입</param>
    /// <param name="targetLevel">목표 레벨</param>
    /// <param name="isRandom">랜덤 생성 여부</param>
    /// <param name="tileInteraction">타일 상호작용 (랜덤시 필요)</param>
    /// <param name="tile">타겟 타일 (랜덤시 필요)</param>
    private void CreateUpgradedTower(ElementType elementType, int targetLevel, bool isRandom = false,
        TileInteraction tileInteraction = null, Tile tile = null)
    {
        string towerId = GetTowerId(elementType, targetLevel);

        // 타워 데이터 검색 (스탯용)
        TowerData towerData = null;
        if (!string.IsNullOrEmpty(towerId) && _towerDataMap.TryGetValue(towerId, out var value))
        {
            towerData = value;
        }

        // Debug.LogError($"ElementType: {elementType}, level: {targetLevel}, towerId: {towerId}, towerData Level: {towerData.Level}");

        // 프리팹은 _elementTowerMap에서 가져오기
        if (!_elementTowerMap.TryGetValue(elementType, out var towerPrefab))
        {
            Debug.LogError($"프리팹을 찾을 수 없습니다: {elementType}");
            if (!isRandom) ClearSelection();
            return;
        }

        // 랜덤 생성이 아닌 경우 기존 아이템들 제거
        if (!isRandom && _selectedItems.Count > 0)
        {
            foreach (var item in _selectedItems)
            {
                var itemTile = item.GetTile();
                var itemTileInteraction = itemTile.GetComponent<TileInteraction>();

                if (itemTileInteraction != null)
                {
                    itemTileInteraction.TileReset(itemTile);
                }

                // 타워/원소 오브젝트 제거
                if (item.GetGameObject() != null)
                {
                    Destroy(item.GetGameObject());
                }
            }
        }

        // 타겟 타일 결정
        Tile targetTile = isRandom ? tile : _selectedItems[2].GetTile();
        TileInteraction targetTileInteraction = isRandom ? tileInteraction : targetTile.GetComponent<TileInteraction>();

        if (targetTileInteraction != null)
        {
            var newTower = targetTileInteraction.PlacedTower(towerPrefab, targetTile);
            if (towerParent) newTower.transform.SetParent(towerParent);

            newTower.name = $"{elementType}_Tower_Level{targetLevel}";

            // 타워 스탯 설정 (데이터가 있는 경우)
            var towerComponent = newTower.GetComponent<Tower>();
            towerComponent.SetTowerSetting(towerData);
            Debug.Log($"towerData: {towerData.Level}");

            TileInteraction.isTowerJustCreated = true;
            OnTowerCreated(newTower, elementType);
            _towerSpriteController?.SetSpritesByLevel(targetLevel);

            Debug.Log($"{elementType} 타입의 {targetLevel}단계 타워 생성 완료{(isRandom ? " (랜덤)" : "")}!");
        }
        else
        {
            Debug.LogError("TileInteraction을 찾을 수 없습니다!");
        }

        // 선택 초기화 (랜덤이 아닌 경우만)
        if (!isRandom)
        {
            ClearSelection();
        }
    }

    /// <summary>
    /// 원소 타입과 레벨에 따른 타워 ID 반환
    /// </summary>
    private string GetTowerId(ElementType elementType, int level)
    {
        string prefix = elementType switch
        {
            ElementType.Fire => "F",
            ElementType.Water => "A",
            ElementType.Air => "W",
            ElementType.Earth => "E",
            _ => ""
        };

        if (string.IsNullOrEmpty(prefix)) return "";

        return $"L{level}{prefix}";
    }

    /// <summary>
    /// Lv4 타워 생성 시도 (특별한 조합)
    /// </summary>
    private void TryCreateLevel4Tower()
    {
        // Lv4는 특정 Lv3 타워 3개의 조합이므로 별도 로직 필요
        Debug.Log("Lv4 타워 조합은 아직 구현되지 않았습니다.");
        ClearSelection();
    }

    /// <summary>
    /// 타워 생성 완료 시 호출
    /// </summary>
    private void OnTowerCreated(GameObject createdTower, ElementType elementType)
    {
        var towerComponent = createdTower.GetComponent<Tower>();
        if (towerComponent != null)
            Debug.Log($"타워 컴포넌트 설정 완료: {towerComponent.GetTowerSetting().Name}");

        _towerSpriteController = createdTower.GetComponent<TowerSpriteController>();
    }

    /// <summary>
    /// 선택 초기화
    /// </summary>
    public void ClearSelection()
    {
        foreach (var item in _selectedItems)
        {
            item?.SetSelected(false);
        }
        _selectedItems.Clear();
        _saleController.ShowSaleUI(false);
        Debug.Log("선택이 초기화되었습니다.");
    }
}
