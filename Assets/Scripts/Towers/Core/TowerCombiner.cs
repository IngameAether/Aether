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

// 원소 뭐 있는지 몰라서 생각나는거 다 적음

public class TowerCombiner : MonoBehaviour
{
    public static TowerCombiner Instance { get; private set; }

    [Header("타워 조합 설정")] [SerializeField] private GameObject waterPrefab;
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject airPrefab;
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Transform towerParent;

    private readonly List<Tile> _selectedTiles = new();
    private Dictionary<ElementType, GameObject> _elementTowerMap;

    private TowerSpriteController towerSpriteController;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        towerSpriteController = FindObjectOfType<TowerSpriteController>();
    }

    private void Start()
    {
        InitializeTowerMapping();
    }

    /// <summary>
    ///     원소별 타워 매핑을 초기화합니다
    ///     현재는 모든 원소가 ArrowTower로 설정되어 있습니다.
    /// </summary>
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
    ///     원소를 선택하는 함수 (클릭 이벤트에서 호출됩니다)
    ///     최대 3개까지 선택할 수 있습니다
    /// </summary>
    /// <param name="elementType">선택할 원소 타입</param>
    public void SelectElement(ElementController clickedElementController)
    {
        var clickedTile = clickedElementController.parentTile;

        if (clickedTile == null || clickedTile.CurrentLogicalElementType == ElementType.None)
        {
            Debug.LogWarning("클릭된 원소에 유효한 타일 정보가 없거나, 원소 타입이 None입니다.");
            return;
        }

        if (clickedTile.CurrentLogicalElementType == ElementType.Tower)
        {
            Debug.LogWarning("타워 타일을 클릭했습니다. 조합 대상이 아닙니다.");
            ClearSelectedElements();
            return;
        }

        // 첫 번쨰 원소 선택
        if (_selectedTiles.Count == 0)
        {
            _selectedTiles.Add(clickedTile);
            clickedElementController.SetSelected(true);
            Debug.Log($"첫 번째 원소 선택: {clickedTile.CurrentLogicalElementType}. 현재 선택된 원소 수: {_selectedTiles.Count}/3");
        }
        // 이미 원소가 선택되어 있는 경우
        else
        {
            var firstSelectedType = _selectedTiles[0].CurrentLogicalElementType;
            var clickedType = clickedTile.CurrentLogicalElementType;

            // 1. 다른 타입의 원소를 클릭했거나
            // 2. 이미 선택된 타일을 다시 클릭했다면
            if (clickedType != firstSelectedType)
            {
                Debug.Log($"선택된 원소 초기화: 다른 타입 ({clickedType}) 클릭 또는 이미 선택된 타일 재클릭.");
                ClearSelectedElements();
                // 새로운 원소를 첫 번째 클릭으로 재시작
                _selectedTiles.Add(clickedTile);
                clickedElementController.SetSelected(true); // 새 원소 하이라이트
                Debug.Log($"새로운 첫 번째 원소 선택: {clickedType}. 현재 선택된 원소 수: {_selectedTiles.Count}/3");
            }
            else if (_selectedTiles.Contains(clickedTile)) // 이미 선택된 타일을 다시 클릭한 경우
            {
                Debug.Log("이미 선택된 타일입니다. 중복 선택을 무시합니다.");
                // 아무것도 하지 않고 함수를 종료
                return;
            }
            // 3. 같은 타입의 다른 원소를 클릭했고, 아직 3개가 채워지지 않았다면
            else if (_selectedTiles.Count < 3)
            {
                _selectedTiles.Add(clickedTile);
                clickedElementController.SetSelected(true); // 해당 원소 하이라이트
                Debug.Log($"같은 원소 ({clickedType}) 추가 선택. 현재 선택된 원소 수: {_selectedTiles.Count}/3");
            }
            else // 3개가 이미 선택된 상태에서 같은 타입의 원소를 추가 클릭시 (물 4개 선택해버렸을 때)
            {
                Debug.LogWarning("이미 3개의 원소가 선택되었습니다. 조합을 시도하거나 초기화하세요.");
            }
        }

        // 3개가 모두 선택되면 즉시 조합 시도
        if (_selectedTiles.Count == 3) TryTowerCombination();
    }

    /// <summary>
    ///     타워 조합을 시도하는 함수
    ///     선택된 3개의 원소가 모두 같은지 확인하고 타워를 생성합니다
    /// </summary>
    public void TryTowerCombination()
    {
        if (_selectedTiles.Count < 3) return;

        var element1 = _selectedTiles[0].CurrentLogicalElementType;
        var element2 = _selectedTiles[1].CurrentLogicalElementType;
        var element3 = _selectedTiles[2].CurrentLogicalElementType;

        if (element1 == element2 && element2 == element3)
        {
            Debug.LogWarning($"조합 성공! {element1} 원소 3개가 조합되어 1단계 타워가 생성됩니다.");
            CreateLevel1Tower();
            towerSpriteController.SetSpritesByLevel(1);
        }
        else
        {
            Debug.LogWarning($"조합 실패! 같은 원소 3개가 아닙니다. 선택된 원소: {element1}, {element2}, {element3}");
            ClearSelectedElements();
        }
    }

    /// <summary>
    /// 랜덤 타워를 생성하는 함수
    /// </summary>
    /// <returns></returns>
    public void CreateRandomLevel1Tower(TileInteraction tileInteraction, Tile tile)
    {
        var keys = _elementTowerMap.Keys.ToArray();
        int randomIndex = Random.Range(0, keys.Length);

        var newTower = tileInteraction.PlacedTower(_elementTowerMap[keys[randomIndex]], tile);
        if (towerParent) newTower.transform.SetParent(towerParent);
        newTower.name = $"{keys[randomIndex]}_Tower_Level1";
        TileInteraction.isTowerJustCreated = true;
        OnTowerCreated(newTower, keys[randomIndex]);
        Debug.Log($"{keys[randomIndex]} 타입의 1단계 타워 생성");
        towerSpriteController.SetSpritesByLevel(1);
    }

    /// <summary>
    ///     1단계 타워를 생성하는 함수
    /// </summary>
    /// <param name="elementType">생성할 타워의 원소 타입</param>
    private void CreateLevel1Tower()
    {
        var typeOfTowerToBuild = _selectedTiles[2].CurrentLogicalElementType;
        if (_elementTowerMap.TryGetValue(typeOfTowerToBuild, out var towerPrefab))
        {
            if (towerPrefab)
            {
                foreach (var selectedTile in _selectedTiles)
                {
                    selectedTile.ApplyHighlight(false);
                    var tileInt = selectedTile.GetComponent<TileInteraction>();
                    if (tileInt != null)
                        tileInt.TileReset(selectedTile);
                    else
                        Debug.LogError($"Tile {selectedTile.name}에 TileInteraction 스크립트가 없습니다! 원소를 제거할 수 없습니다.");
                }

                var targetTile = _selectedTiles[2]; // 세 번째 타일에서 타워 생성
                var targetTileInteraction = targetTile.GetComponent<TileInteraction>();

                if (targetTileInteraction != null)
                {
                    var newTower = targetTileInteraction.PlacedTower(towerPrefab, targetTile);
                    if (towerParent) newTower.transform.SetParent(towerParent);
                    newTower.name = $"{typeOfTowerToBuild}_Tower_Level1";
                    TileInteraction.isTowerJustCreated = true;
                    OnTowerCreated(newTower, typeOfTowerToBuild);
                    Debug.Log($"{typeOfTowerToBuild} 타입의 1단계 타워 생성");
                }
                else
                {
                    Debug.LogError($"타겟 타일 {targetTile.name}에 TileInteraction 스크립트가 없습니다! 타워를 생성할 수 없습니다.");
                }

                ClearSelectedElements();
            }
            else
            {
                Debug.LogError($"{typeOfTowerToBuild} 원소에 대한 타워 프리팹이 설정되지 않았습니다!");
                ClearSelectedElements();
            }
        }
        else
        {
            Debug.LogError($"{typeOfTowerToBuild} 원소가 타워 매핑에 존재하지 않습니다!");
            ClearSelectedElements();
        }
    }

    /// <summary>
    ///     타워가 생성되었을 때 호출되는 함수
    ///     추가적인 로직을 여기에 구현할 수 있습니다
    /// </summary>
    /// <param name="createdTower">생성된 타워 게임오브젝트</param>
    /// <param name="elementType">타워의 원소 타입</param>
    private void OnTowerCreated(GameObject createdTower, ElementType elementType)
    {
        // 사운드 재생, 이펙트 생성, UI 업데이트 등

        var towerComponent = createdTower.GetComponent<Tower>();
        if (towerComponent != null)
            // etc
            Debug.Log($"타워 컴포넌트 설정 완료: {towerComponent.GetTowerSetting().Name}");

        towerSpriteController = createdTower.GetComponent<TowerSpriteController>();
    }

    /// <summary>
    ///     선택된 원소들을 초기화하는 함수
    ///     조합 후나 수동으로 초기화할 때 사용됩니다
    /// </summary>
    public void ClearSelectedElements()
    {
        // 선택된 모든 타이르이 하이라이트 해제
        foreach (var tile in _selectedTiles)
            if (tile != null && tile.element != null)
                tile.element.GetComponent<ElementController>()?.SetSelected(false);

        _selectedTiles.Clear();
        Debug.Log("선택된 원소들이 초기화되었습니다.");
    }
}
