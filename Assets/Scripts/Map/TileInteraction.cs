using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileInteraction : MonoBehaviour
{
    public Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)
    public static bool isTowerJustCreated = false;  // 타워 생성 플래그
    private BoxCollider2D _boxCollider2D;
    private bool _isDirectPlaceTower = false;

    private void Start()
    {
        tile = GetComponent<Tile>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        MagicBookManager.OnBookEffectApplied += HandleBookEffectApplied;
    }

    private void OnDisable()
    {
        MagicBookManager.OnBookEffectApplied -= HandleBookEffectApplied;
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
            int ranNum = Random.Range(0, staticElementPrefabs.Length);
            ElementType assignedElementType = ElementType.None;
            GameObject selectedPrefab = staticElementPrefabs[ranNum];
            ElementController ecFromPrefab = selectedPrefab.GetComponent<ElementController>();

            if (ecFromPrefab != null)
            {
                assignedElementType = ecFromPrefab.type;
            }
            else
            {
                Debug.LogWarning("프리팹에 ElementController가 없음 or type 할당되지 않음");
                assignedElementType = ElementType.None;
            }

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

            Debug.Log($"소환된 원소: {staticElementPrefabs[ranNum]}");
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

    private void HandleBookEffectApplied(EBookEffectType bookEffectType, int value)
    {
        if (bookEffectType != EBookEffectType.DirectTowerPlace) return;
        _isDirectPlaceTower = (value == 1);
    }
}
