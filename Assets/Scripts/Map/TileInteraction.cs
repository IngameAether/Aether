using System.Collections;
using System.Collections.Generic;
using Towers.Core;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    public Tile tile;
    public static GameObject[] staticElementPrefabs;  // 전역 변수로 선언(모든 tile이 공유할 내용이므로)
    public static bool isTowerJustCreated = false;  // Ÿ���� Ŭ���� Ÿ���� ��ġ�� ������ Ÿ���� Ŭ���� ������ �����ϱ� ����

    private BoxCollider2D _boxCollider2D;

    private void Start()
    {
        tile = GetComponent<Tile>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;   // 게임이 멈추면 클릭 등 상호작용 무시

        tile.ChangeCurrentTileColor();
        tile.PrintTileInfo();

        if (!tile.isBuild || !tile.isElementBuild) return;

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
            ec.Initialize(this, assignedElementType);
            ec.selectTile = tile;
        }
        else
        {
            Debug.LogWarning("생성된 elementObj에 ElementController 컴포넌트가 없음, 초기화 불가능");
        }

        Debug.Log($"소환된 원소: {staticElementPrefabs[ranNum]}");
        _boxCollider2D.enabled = false;
        tile.isElementBuild = false;
        tile.element = elementObj;
    }

    public GameObject PlacedTower(GameObject prefab)
    {
        var towerObj = Instantiate(prefab, tile.transform.position, Quaternion.identity);
        tile.isElementBuild = false;
        tile.tower = towerObj;

        TowerDragSale tds = towerObj.GetComponent<TowerDragSale>();
        tds.selectTile = tile;

        return towerObj;
    }

    public void TileReset()
    {
        tile.isElementBuild = true;
        Destroy(tile.element.gameObject);
        tile.element = null;
        _boxCollider2D.enabled = true;
    }
}
