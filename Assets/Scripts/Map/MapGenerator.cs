using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    const int SIZE = 8;
    const int PATH_LENGTH = 22;
    int[,] mapTiles = new int[SIZE, SIZE];  // 1=타일 지나가는 경로, 0=벽
    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    Vector2Int startTile, endTile;  // 입구, 출구

    // 허용된 입구: 1, 2, 3, 5 에 해당하는 인덱스
    private List<Vector2Int> allowedStartIndices = new List<Vector2Int>()
    {
        new Vector2Int(0, 1), new Vector2Int(0, 6), new Vector2Int(1, 0), new Vector2Int(1, 7)
    };

    private List<Vector2Int> pathTiles = new List<Vector2Int>();
    private HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

    //void Start()
    //{
    //    mapRenderer = GetComponent<MapRenderer>();
    //    generateMap();
    //    printGrid();
    //    mapRenderer.RenderMap(mapTiles);
    //}

    public int[,] generateMap()
    {
        // 초기화
        mapTiles = new int[SIZE, SIZE];

        bool successMap = false;
        int attemptsMap = 0;

        while (!successMap && attemptsMap < 5)
        {
            successMap = findvalidTile();
            attemptsMap++;
        }

        if (successMap)
        {
            Debug.Log(startTile);
            Debug.Log(endTile);

        }

        else
        {
            Debug.Log("경로 생성 실패");
        }

        return mapTiles;
    }

    // 적합한 경로를 못 찾을 경우에 입,출구부터 다시 지정하게 하기 위해 generateMap() 메소드와 분리
    private bool findvalidTile()
    {
        // 초기화
        pathTiles.Clear();
        visited.Clear();
        // 맵 그림의 허용된 입구 중 하나를 랜덤으로 선택
        int randomIndex = Random.Range(0, allowedStartIndices.Count);
        startTile = allowedStartIndices[randomIndex];

        do
        {
            startTile = generateEdgeTile();
            endTile = generateEdgeTile();
        }
        while (startTile == endTile || startTile.x == endTile.x || startTile.y == endTile.y || startTile.x == 7);

        bool success = false;
        int attempts = 0;

        while (!success && attempts < 20)
        {
            pathTiles.Clear();
            visited.Clear();

            pathTiles.Add(startTile);
            visited.Add(startTile);

            success = generatePath(startTile, endTile);
            attempts++;

            if (success)
            {
                // mapTiles 초기화
                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                        mapTiles[x, y] = 0;
                }

                pathTiles.Add(endTile);
                foreach (var tile in pathTiles)
                    mapTiles[tile.x, tile.y] = 1;

                if (has3X3Wall()) success = false;
            }
        }

        //if (success)
        //{
        //    pathTiles.Add(endTile);
        //    foreach (var tile in pathTiles)
        //    {
        //        mapTiles[tile.x, tile.y] = 1;
        //    }
        //}

        return success;
    }

    // 입구, 출구 타일 랜덤 생성
    private Vector2Int generateEdgeTile()
    {
        bool ran = Random.value < 0.5f;

        if (ran)
        {
            int x = (Random.value < 0.5f) ? 0 : 7;
            int y = (Random.value < 0.5f) ? 1 : 6;
            return new Vector2Int(x, y);
        }
        else
        {
            int y = (Random.value < 0.5f) ? 0 : 7;
            int x = (Random.value < 0.5f) ? 1 : 6;
            return new Vector2Int(x, y);
        }
    }

    private bool generatePath(Vector2Int current, Vector2Int end)
    {
        if (pathTiles.Count == PATH_LENGTH - 1)   // 통로 22칸 완성
        {
            foreach (var dir in directions)
            {
                if (current + dir == end) return true;  // 마지막 통로가 출구와 연결되어 있는지 확인
            }
            return false;
        }

        Vector2Int[] sortedDirs = SortedDirections(current, end);
        //Shuffle(directions);
        foreach (var dir in sortedDirs)
        {
            Vector2Int next = current + dir;
            if (isValid(next))
            {
                pathTiles.Add(next);
                visited.Add(next);

                if (generatePath(next, end)) return true;

                // 경로가 막힌 경우 되돌아가기 위해
                pathTiles.RemoveAt(pathTiles.Count - 1);
                visited.Remove(next);
            }
        }
        return false;   // 적합한 경로를 찾지 못한 경우
    }

    // 출구와 가까운 방향을 가중치를 매겨 정렬함
    private Vector2Int[] SortedDirections(Vector2Int current, Vector2Int end)
    {
        List<Vector2Int> dirList = new List<Vector2Int>(directions);
        dirList.Sort((a, b) =>  // 커스텀 정렬
        {
            Vector2Int next1 = current + a;
            Vector2Int next2 = current + b;
            float dist1 = Vector2Int.Distance(next1, end);
            float dist2 = Vector2Int.Distance(next2, end);
            return dist1.CompareTo(dist2);  // 오름차순 정렬
        });
        return dirList.ToArray();   // list를 배열로 변환후 반환
    }

    // 피셔-에이츠 셔플 알고리즘 사용
    private void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int ran = Random.Range(i, array.Length);
            (array[i], array[ran]) = (array[ran], array[i]);
        }
    }

    // 다음 타일 유효성 검사
    bool isValid(Vector2Int tile)
    {
        if (tile.x < 1 || tile.x > 6 || tile.y < 1 || tile.y > 6) return false;    // 벽인 경우
        if (visited.Contains(tile)) return false;   // 이미 경로에 포함되어 있는 경우

        // 통로에서 2X2 광장이 생성되는지 확인
        int neighbors = 0;
        foreach (var dir in directions)
        {
            var sub = tile + dir;
            if (pathTiles.Contains(sub)) neighbors++;
        }
        return neighbors <= 1;
    }

    // 생성된 경로 타일 인덱스 목록을 반환하는 public 메서드
    public List<Vector2Int> GetPathIndices()
    {
        // pathTiles 리스트는 generateMap 또는 findvalidTile 실행 후 유효합니다.
        // generateMap이 false를 반환하면 pathTiles는 비어있거나 불완전할 수 있습니다.
        return pathTiles;
    }

    // 3X3 형태인 벽이 있으면 true 반환
    bool has3X3Wall()
    {
        for (int w = 0; w <= SIZE - 3; w++)
        {
            for (int h = 0; h <= SIZE - 3; h++)
            {
                bool isWall = true;
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (mapTiles[w + x, h + y] == 1)
                        {
                            isWall = false;
                            break;
                        }
                    }
                    if (!isWall) break;
                }
                if (isWall) return true;
            }
        }
        return false;
    }

    void printGrid()
    {
        string output = "";
        for (int x=0; x<SIZE; x++)
        {
            for (int y=0; y<SIZE; y++)
            {
                output += mapTiles[x, y] == 1 ? "ㅇ" : "ㅁ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}
