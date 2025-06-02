using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManage : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapRenderer mapRenderer;

    // 생성된 맵 타일 데이터와 경로 인덱스를 저장할 속성 추가
    public int[,] MapTiles { get; private set; }
    public List<Vector2Int> PathIndices { get; private set; }

    void Start()
    {
        GenerateAndRenderMap(); // 맵 생성 및 렌더링 시작
    }

    // 맵을 생성하고 렌더링하는 내부 메서드
    private void GenerateAndRenderMap()
    {
        // 기존 맵 데이터 및 오브젝트 제거
        if (MapTiles != null)
        {
            mapRenderer.ClearMap();
        }

        // MapGenerator를 사용하여 맵 타일 데이터 생성
        MapTiles = mapGenerator.generateMap();

        // MapGenerator로부터 생성된 경로 인덱스 가져와 저장
        PathIndices = mapGenerator.GetPathIndices();

        // MapRenderer를 사용하여 맵 시각화
        if (MapTiles != null && MapTiles.GetLength(0) > 0)
        {
            mapRenderer.RenderMap(MapTiles);
            Debug.Log($"맵 생성 및 렌더링 완료. 맵 크기: {MapTiles.GetLength(0)}x{MapTiles.GetLength(1)}, 경로 길이: {PathIndices.Count}");
        }
        else
        {
            Debug.LogError("맵 생성 실패 또는 데이터가 비어있습니다.");
        }
    }

    // ResetMap 메서드는 그대로 유지 (맵을 지우고 다시 생성 및 렌더링)
    public void ResetMap()
    {
        Debug.Log("맵 리셋 요청.");
        GenerateAndRenderMap();
    }

    // 생성된 경로 인덱스 목록을 유니티 월드 좌표 목록으로 변환하여 반환하는 메서드 추가
    public List<Vector3> GetPathWorldPositions()
    {
        if (PathIndices == null || PathIndices.Count == 0)
        {
            Debug.LogWarning("MapManage: 생성된 경로 데이터가 없습니다.");
            return new List<Vector3>();
        }

        List<Vector3> worldPositions = new List<Vector3>();
        int mapWidth = MapTiles.GetLength(0);
        int mapHeight = MapTiles.GetLength(1);

        foreach (var tileIndex in PathIndices)
        {
            // MapRenderer의 헬퍼 메서드를 사용하여 타일 인덱스를 월드 좌표로 변환
            // 이 메서드는 MapRenderer에 추가될 것입니다.
            worldPositions.Add(mapRenderer.GetTileWorldPosition(tileIndex.x, tileIndex.y, mapWidth, mapHeight));
        }

        Debug.Log($"경로 인덱스 {PathIndices.Count}개를 월드 좌표 {worldPositions.Count}개로 변환했습니다.");
        return worldPositions;
    }

    // MapRenderer 인스턴스에 외부에서 접근 가능하도록 public 속성 추가
    // GetPathWorldPositions에서 MapRenderer의 메서드를 호출하기 위해 필요
    public MapRenderer MapRendererInstance { get { return mapRenderer; } }
}
