using System.Collections;
using System.Collections.Generic;
using Towers.Core;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    public Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)
    public static bool isTowerJustCreated = false;  // 타워 생성 플래그

    private BoxCollider2D _boxCollider2D;

    private void Start()
    {
        tile = GetComponent<Tile>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;   // 게임이 멈추면 클릭 등 상호작용 무시

        if(tile.element != null) // 빈 타일 클릭시에만 로직 수행
        {
            return;
        }
        if (!tile.isBuild || !tile.isElementBuild)
        {
            tile.PrintTileInfo();
            return;
        }

        ElementType assignedElementType = ElementType.None;

        int ranNum = Random.Range(0, staticElementPrefabs.Length);
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

        // 원소가 배치된 타일 저장
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
