using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteraction : MonoBehaviour
{
    Tile tile;
    public static GameObject[] staticElementPrefabs;  // ���� ������ ����(��� tile�� ������ �����̹Ƿ�)
    public static GameObject[] staticTowerPrefabs;
    public static int clickNum = 0;  // ��ü���� Ŭ�� Ƚ���� �����ؾ� �ϹǷ� static ����
    public static bool isTowerJustCreated = false;  // Ÿ���� Ŭ���� Ÿ���� ��ġ�� ������ Ÿ���� Ŭ���� ������ �����ϱ� ����

    void Start()
    {
        tile = GetComponent<Tile>();
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;   // ������ ���߸� Ŭ�� �� ��ȣ�ۿ� ����

        tile.ChangeCurrentTileColor();
        tile.PrintTileInfo();

        if (!tile.isBuild || !tile.isElementBuild) return;

        // Ÿ�� ù Ŭ���� Ÿ�� ��ġ�ǰ�: ���߿� �ǵ��� ����
        int ranNum = 0; GameObject elementObj = null;
        if (clickNum == 0)
        {
            elementObj = Instantiate(staticTowerPrefabs[0], tile.transform.position, Quaternion.identity);
            clickNum++;
            isTowerJustCreated = true;
        }
        else
        {
            ranNum = Random.Range(0, staticElementPrefabs.Length);
            elementObj = Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);
            clickNum++;
        }
        //int ranNum = Random.Range(0, staticElementPrefabs.Length);
        //GameObject elementObj = Instantiate(staticElementPrefabs[ranNum], tile.transform.position, Quaternion.identity);

        // ���Ұ� ��ġ�� Ÿ�� ����
        ElementController ec = elementObj.GetComponent<ElementController>();
        if (ec != null) ec.selectTile = tile;

        Debug.Log($"��ȯ�� ����: {staticElementPrefabs[ranNum]}");
        tile.isElementBuild = false;
        tile.element = elementObj;
    }
}
