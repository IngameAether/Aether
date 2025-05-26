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
        // 기존 타일 오브젝트가 있다면 모두 제거
        ClearMap();

        tileObjects = new GameObject[mapTiles.GetLength(0), mapTiles.GetLength(1)];

        float setx = (mapTiles.GetLength(0) - 1) / 2;
        float sety = (mapTiles.GetLength(1) - 1) / 2;

        for (int x = 0; x < mapTiles.GetLength(0); x++)
        {
            for (int y = 0; y < mapTiles.GetLength(1); y++)
            {
                isBuild = (mapTiles[x, y] == 0);

                GameObject prefab = mapTiles[x, y] == 1 ? pathTile : mapTile;
                Vector2 position = new Vector2(y - setx, -x + sety);
                GameObject tileObject = Instantiate(prefab, position, Quaternion.identity);

                tileObject.GetComponent<Tile>().Initialize(x, y, isBuild);  // �� Ÿ���� ������ ����ϱ� ����
                tileObjects[x, y] = tileObject;
            }
        }
    }

    public void ClearMap()
    {
        if (tileObjects == null) return;
        for (int x = 0; x < tileObjects.GetLength(0); x++)
        {
            for (int y = 0; y < tileObjects.GetLength(1); y++)
            {
                if (tileObjects[x, y] != null)
                    Destroy(tileObjects[x, y]);
            }
        }
    tileObjects = null; // 배열 초기화
    Debug.Log("기존 맵 오브젝트 제거 완료.");
    }

public Vector3 GetTileWorldPosition(int tileX, int tileY, int mapWidth, int mapHeight)
{
        float offsetX = (mapWidth - 1) / 2.0f;
        float offsetY = (mapHeight - 1) / 2.0f;

        // RenderMap에서 사용된 타일 기준점 위치 계산
        float baseX = tileY - offsetX;
        float baseY = -tileX + offsetY;

        // 타일 크기가 1x1 유닛이므로 타일 중앙 좌표 계산 시 0.5f 오프셋 적용
        float tileCenterX = baseX + 0.5f;
        float tileCenterY = baseY + 0.5f - 1.0f;

        float tileZ = 0f; // 또는 필요한 Z 값

        return new Vector3(tileCenterX, tileCenterY, tileZ);
    }
}