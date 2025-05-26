using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public GameObject pathTile;  // ���� �����ٴϴ� ��� Ÿ��
    public GameObject mapTile;   // Ÿ�� ��ġ�� �� �ִ�(��) Ÿ��
    GameObject[,] tileObjects;   // Ÿ�� ������ �� �ְ� ������ �迭
    bool isBuild;   // ��ġ ������ Ÿ������ Ȯ�ο�

    public void RenderMap(int[,] mapTiles)
    {
        tileObjects = new GameObject[mapTiles.GetLength(0), mapTiles.GetLength(1)];

        float setx = (mapTiles.GetLength(0) - 1) / 2;
        float sety = (mapTiles.GetLength(1) - 1) / 2;

        for (int x=0; x<mapTiles.GetLength(0); x++)
        {
            for (int y=0; y<mapTiles.GetLength(1); y++)
            {
                isBuild = (mapTiles[x, y]==0);

                GameObject prefab = mapTiles[x, y] == 1 ? pathTile : mapTile;
                Vector2 position = new Vector2(y-setx,-x+sety);
                GameObject tileObject = Instantiate(prefab, position, Quaternion.identity);

                tileObject.GetComponent<Tile>().Initialize(x, y, isBuild);  // �� Ÿ���� ������ ����ϱ� ����
                tileObjects[x, y] = tileObject;
            }
        }
    }

    public void ClearMap()
    {
        if (tileObjects == null) return;
        for (int x=0; x<tileObjects.GetLength(0); x++)
        {
            for (int y=0; y<tileObjects.GetLength(1); y++)
            {
                if (tileObjects[x,y] != null)
                    Destroy(tileObjects[x,y]);
            }
        }
    }
}
