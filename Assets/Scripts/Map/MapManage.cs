using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManage : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapRenderer mapRenderer;

    // ������ �� Ÿ�� �����Ϳ� ��� �ε����� ������ �Ӽ� �߰�
    public int[,] MapTiles { get; private set; }
    public List<Vector2Int> PathIndices { get; private set; }

    void Start()
    {
        GenerateAndRenderMap(); // �� ���� �� ������ ����
    }

    // ���� �����ϰ� �������ϴ� ���� �޼���
    private void GenerateAndRenderMap()
    {
        // ���� �� ������ �� ������Ʈ ����
        if (MapTiles != null)
        {
            mapRenderer.ClearMap();
        }

        int seed = -1; // 기본값
        if (GameSaveManager.Instance != null && GameSaveManager.Instance.CurrentGameData != null)
        {
            seed = GameSaveManager.Instance.CurrentGameData.currentMapSeed;
            Debug.Log($"저장된 게임 로드 중: Seed = {seed}");
        }

        // MapGenerator�� ����Ͽ� �� Ÿ�� ������ ����
        MapTiles = mapGenerator.generateMap(seed);

        // MapGenerator�κ��� ������ ��� �ε��� ������ ����
        if (MapTiles != null) PathIndices = mapGenerator.GetPathIndices();

        // MapRenderer�� ����Ͽ� �� �ð�ȭ
        if (MapTiles != null && MapTiles.GetLength(0) > 0)
        {
            mapRenderer.RenderMap(MapTiles);
            Debug.Log($"�� ���� �� ������ �Ϸ�. �� ũ��: {MapTiles.GetLength(0)}x{MapTiles.GetLength(1)}, ��� ����: {PathIndices.Count}");

            StartCoroutine(RestoreGameStateAfterDelay());
        }
        else
        {
            Debug.LogError("�� ���� ���� �Ǵ� �����Ͱ� ����ֽ��ϴ�.");
        }
    }

    private IEnumerator RestoreGameStateAfterDelay()
    {
        yield return null;

        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.RestoreGameState();
        }
    }

    // ResetMap �޼���� �״�� ���� (���� ����� �ٽ� ���� �� ������)
    public void ResetMap()
    {
        Debug.Log("�� ���� ��û.");
        GenerateAndRenderMap();
    }

    // ������ ��� �ε��� ����� ����Ƽ ���� ��ǥ ������� ��ȯ�Ͽ� ��ȯ�ϴ� �޼��� �߰�
    public List<Vector3> GetPathWorldPositions()
    {
        if (PathIndices == null || PathIndices.Count == 0)
        {
            Debug.LogWarning("MapManage: ������ ��� �����Ͱ� �����ϴ�.");
            return new List<Vector3>();
        }

        List<Vector3> worldPositions = new List<Vector3>();
        int mapWidth = MapTiles.GetLength(0);
        int mapHeight = MapTiles.GetLength(1);

        foreach (var tileIndex in PathIndices)
        {
            // MapRenderer�� ���� �޼��带 ����Ͽ� Ÿ�� �ε����� ���� ��ǥ�� ��ȯ
            // �� �޼���� MapRenderer�� �߰��� ���Դϴ�.
            worldPositions.Add(mapRenderer.GetTileWorldPosition(tileIndex.x, tileIndex.y, mapWidth, mapHeight));
        }

        Debug.Log($"��� �ε��� {PathIndices.Count}���� ���� ��ǥ {worldPositions.Count}���� ��ȯ�߽��ϴ�.");
        return worldPositions;
    }

    // MapRenderer �ν��Ͻ��� �ܺο��� ���� �����ϵ��� public �Ӽ� �߰�
    // GetPathWorldPositions���� MapRenderer�� �޼��带 ȣ���ϱ� ���� �ʿ�
    public MapRenderer MapRendererInstance { get { return mapRenderer; } }
}
