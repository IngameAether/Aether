using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileInteraction : MonoBehaviour
{
    public Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)
    public static bool isTowerJustCreated = false;  // 타워 생성 플래그

    public static int ElementSummonCost = 10;
    public static float FireProb = 0.25f;
    public static float WaterProb = 0.25f;
    public static float EarthProb = 0.25f;
    public static float AirProb = 0.25f;

    public static void InitializeTileData()
    {
        ElementSummonCost = GameDataDatabase.GetInt("element_summon", 10);
        FireProb = GameDataDatabase.GetFloat("fire_element_probability", 0.25f);
        WaterProb = GameDataDatabase.GetFloat("water_element_probability", 0.25f);
        EarthProb = GameDataDatabase.GetFloat("earth_element_probability", 0.25f);
        AirProb = GameDataDatabase.GetFloat("air_element_probability", 0.25f);
    }

    private BoxCollider2D _boxCollider2D;
    private bool _isDirectPlaceTower = false;

    private void Start()
    {
        tile = GetComponent<Tile>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// InputManager에서 호출하는 클릭 처리 (OnMouseDown 대체)
    /// </summary>
    public void OnClick()
    {
        if (Time.timeScale == 0f) return;

        if(tile.element != null) return;

        if (!tile.isBuild || !tile.isElementBuild)
        {
            tile.PrintTileInfo();
            return;
        }

        if (_isDirectPlaceTower)
        {
            TileReset(tile);
            TowerCombiner.Instance.CreateRandomLevel1Tower(this, tile);
            return;
        }
        else
        {
            // 비용 확인 및 차감
            if (ResourceManager.Instance == null || !ResourceManager.Instance.SpendCoin(ElementSummonCost))
            {
                Debug.Log($"에테르가 부족합니다. (필요: {ElementSummonCost})");
                return;
            }

            // 가중치 확률로 원소 선택
            float rand = Random.value;
            ElementType selectedType = ElementType.Fire;

            if (rand < FireProb) selectedType = ElementType.Fire;
            else if (rand < FireProb + WaterProb) selectedType = ElementType.Water;
            else if (rand < FireProb + WaterProb + EarthProb) selectedType = ElementType.Earth;
            else selectedType = ElementType.Air;

            // 선택된 타입에 맞는 프리팹 찾기
            GameObject selectedPrefab = null;
            if (staticElementPrefabs != null)
            {
                foreach (var prefab in staticElementPrefabs)
                {
                    if (prefab.TryGetComponent<ElementController>(out var elementController))
                    {
                        if (elementController.type == selectedType)
                        {
                            selectedPrefab = prefab;
                            break;
                        }
                    }
                }
            }

            // Fallback: 못 찾으면 랜덤 or 첫번째
            if (selectedPrefab == null && staticElementPrefabs != null && staticElementPrefabs.Length > 0)
            {
                Debug.LogWarning($"선택된 원소 타입({selectedType})의 프리팹을 찾지 못해 기본값 사용");
                selectedPrefab = staticElementPrefabs[0];
            }

            if (selectedPrefab == null) return;

            ElementType assignedElementType = selectedType;

            GameObject elementObj = Instantiate(selectedPrefab, tile.transform.position, Quaternion.identity);

            ElementController ec = elementObj.GetComponent<ElementController>();
            if (ec != null)
            {
                ec.Initialize(tile, assignedElementType);
            }
            else
            {
                Debug.LogWarning("생성된 elementObj에 ElementController 컴포넌트가 없음, 초기화 불가능");
            }

            // 마법도서 효과 적용: 원소응용학 (GR1~4) - 타워 소환 시 에테르 획득
            if (MagicBookBuffSystem.Instance != null && ResourceManager.Instance != null)
            {
                float bonusEther = 0f;
                switch (assignedElementType)
                {
                    case ElementType.Fire:
                        bonusEther = MagicBookBuffSystem.Instance.GetGlobalBuffValue(EBookEffectType.FireTowerSummonEther);
                        break;
                    case ElementType.Water:
                        bonusEther = MagicBookBuffSystem.Instance.GetGlobalBuffValue(EBookEffectType.WaterTowerSummonEther);
                        break;
                    case ElementType.Air:
                        bonusEther = MagicBookBuffSystem.Instance.GetGlobalBuffValue(EBookEffectType.AirTowerSummonEther);
                        break;
                    case ElementType.Earth:
                        bonusEther = MagicBookBuffSystem.Instance.GetGlobalBuffValue(EBookEffectType.EarthTowerSummonEther);
                        break;
                }

                if (bonusEther > 0)
                {
                    ResourceManager.Instance.AddCoin((int)bonusEther);
                }
            }

            Debug.Log($"소환된 원소: {selectedPrefab.name} (Type: {selectedType})");
            _boxCollider2D.enabled = false;
            tile.isElementBuild = false;
            tile.element = elementObj;
            tile.PrintTileInfo();
        }
    }

    public GameObject PlacedTower(GameObject prefab, Tile targetTile)
    {
        var towerObj = Instantiate(prefab, targetTile.transform.position, Quaternion.identity);
        targetTile.isElementBuild = false;
        targetTile.tower = towerObj;

        TowerDragSale tds = towerObj.GetComponent<TowerDragSale>();
        if(tds != null)
        {
            tds.selectTile = targetTile;
        }

        var towerSelectable = towerObj.GetComponent<TowerSelectable>();
        towerSelectable.SetTile(targetTile);

        return towerObj;
    }

    public void TileReset(Tile targetTile)
    {
        targetTile.isElementBuild = true;
        if(targetTile.element != null)
        {
            Destroy(targetTile.element.gameObject);
            targetTile.element = null;
        }

        BoxCollider2D targetCollider = targetTile.GetComponent<BoxCollider2D>();
        if(targetCollider != null)
        {
            targetCollider.enabled = true;
        }
    }
}
