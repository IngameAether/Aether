using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    const int SIZE = 8;
    const int PATH_LENGTH = 22;
    int[,] mapTiles = new int[SIZE, SIZE];  // 1=�� �������� ���, 0=Ÿ�� ��ġ ������ ��
    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    Vector2Int startTile, endTile;  // �Ա�, �ⱸ

    // ���� �Ա�: 1, 2, 3, 5 �� �ش��ϴ� �ε���
    private List<Vector2Int> allowedStartIndices = new List<Vector2Int>()
    {
        new Vector2Int(0, 1), new Vector2Int(0, 6), new Vector2Int(1, 0), new Vector2Int(1, 7)
    };

    private List<Vector2Int> pathTiles = new List<Vector2Int>();
    private HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

    public int CurrentSeed { get; private set; }

    public int[,] generateMap(int seed = -1)
    {
        if (seed == -1)
            seed = System.Environment.TickCount;

        CurrentSeed = seed;
        Random.InitState(seed);

        // �ʱ�ȭ
        mapTiles = new int[SIZE, SIZE];

        bool successMap = false;
        int attemptsMap = 0;

        while (!successMap && attemptsMap < 10)
        {
            successMap = findvalidTile();
            attemptsMap++;
        }

        if (successMap)
        {
            Debug.Log(startTile);
            Debug.Log(endTile);
            return mapTiles;
        }

        else
        {
            Debug.Log("��� ���� ����");
            return null;
        }
    }

    // ������ ��θ� �� ã�� ��쿡 ��,�ⱸ���� �ٽ� �����ϰ� �ϱ� ���� generateMap() �޼ҵ�� �и�
    private bool findvalidTile()
    {
        // �ʱ�ȭ
        pathTiles.Clear();
        visited.Clear();
        // �� �׸��� ���� �Ա� �� �ϳ��� �������� ����
        //int randomIndex = Random.Range(0, allowedStartIndices.Count);
        //startTile = allowedStartIndices[randomIndex];

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
                // mapTiles �ʱ�ȭ
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

        return success;
    }

    // �Ա�, �ⱸ Ÿ�� ���� ����
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
        if (pathTiles.Count == PATH_LENGTH - 1)   // ��� 22ĭ �ϼ�
        {
            foreach (var dir in directions)
            {
                if (current + dir == end) return true;  // ������ ��ΰ� �ⱸ�� ����Ǿ� �ִ��� Ȯ��
            }
            return false;
        }

        Vector2Int[] sortedDirs = SortedDirections(current, end);
        foreach (var dir in sortedDirs)
        {
            Vector2Int next = current + dir;
            if (isValid(next))
            {
                pathTiles.Add(next);
                visited.Add(next);

                if (generatePath(next, end)) return true;

                // ��ΰ� ���� ��� �ǵ��ư��� ����
                pathTiles.RemoveAt(pathTiles.Count - 1);
                visited.Remove(next);
            }
        }
        return false;   // ������ ��θ� ã�� ���� ���
    }

    // �ⱸ�� ����� ������ ����ġ�� �Ű� ������
    private Vector2Int[] SortedDirections(Vector2Int current, Vector2Int end)
    {
        List<Vector2Int> dirList = new List<Vector2Int>(directions);
        dirList.Sort((a, b) =>  // Ŀ���� ����
        {
            Vector2Int next1 = current + a;
            Vector2Int next2 = current + b;
            float dist1 = Vector2Int.Distance(next1, end);
            float dist2 = Vector2Int.Distance(next2, end);
            return dist1.CompareTo(dist2);  // �������� ����
        });
        return dirList.ToArray();   // list�� �迭�� ��ȯ�� ��ȯ
    }

    // ���� Ÿ�� ��ȿ�� �˻�
    bool isValid(Vector2Int tile)
    {
        if (tile.x < 1 || tile.x > 6 || tile.y < 1 || tile.y > 6) return false;    // ���� ���
        if (visited.Contains(tile)) return false;   // �̹� ��ο� ���ԵǾ� �ִ� ���

        // ��ο��� 2X2 ������ �����Ǵ��� Ȯ��
        int neighbors = 0;
        foreach (var dir in directions)
        {
            var sub = tile + dir;
            if (pathTiles.Contains(sub)) neighbors++;
        }
        return neighbors <= 1;
    }

    // ������ ��� Ÿ�� �ε��� ����� ��ȯ�ϴ� public �޼���
    public List<Vector2Int> GetPathIndices()
    {
        // pathTiles ����Ʈ�� generateMap �Ǵ� findvalidTile ���� �� ��ȿ�մϴ�.
        // generateMap�� false�� ��ȯ�ϸ� pathTiles�� ����ְų� �ҿ����� �� �ֽ��ϴ�.
        return pathTiles;
    }

    // 3X3 ������ ���� ������ true ��ȯ
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
}
