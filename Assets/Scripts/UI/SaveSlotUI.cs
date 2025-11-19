using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI saveTimeText;
    public TextMeshProUGUI aetherText;
    public RawImage minimapImage;

    [Header("미니맵 설정")]
    public int minimapResolution = 512; // 미니맵 텍스처 해상도
    public float cameraSize = 30f; // 카메라 Orthographic Size (맵 크기에 맞춰 조정)
    public LayerMask minimapLayerMask = -1; // 미니맵에 렌더링할 레이어

    [Header("프리팹 참조")]
    public GameObject[] elementPrefabs = new GameObject[4]; // Fire, Water, Earth, Air 순서
    public TowerData[] allTowerData; // 모든 타워 데이터 (Inspector에서 설정)

    [Header("타워 프리팹 (ElementType별)")]
    public GameObject fireTowerPrefab;
    public GameObject waterTowerPrefab;
    public GameObject earthTowerPrefab;
    public GameObject airTowerPrefab;

    private static Dictionary<int, Texture2D> _minimapCache = new Dictionary<int, Texture2D>();
    private Texture2D _currentTexture = null;
    private int _currentSlotIndex = -1;
    private Dictionary<ElementType, GameObject> _towerPrefabMap;

    private void Awake()
    {
        if (waveText != null)
            waveText.text = "Empty Slot";
        if (saveTimeText != null)
            saveTimeText.text = "-";
        if (aetherText != null)
            aetherText.text = "에테르: -";

        if (minimapImage != null)
            minimapImage.gameObject.SetActive(false);

        // TowerCombiner 쓸려고 했는데 MainMenu 씬에 없어서 그냥 수동으로 매핑
        _towerPrefabMap = new Dictionary<ElementType, GameObject>
        {
            { ElementType.Fire, fireTowerPrefab },
            { ElementType.Water, waterTowerPrefab },
            { ElementType.Earth, earthTowerPrefab },
            { ElementType.Air, airTowerPrefab }
        };
    }

    private void OnDestroy()
    {
        _currentTexture = null;
    }

    public void UpdateUI(SaveSlot info, int slotIndex)
    {
        _currentSlotIndex = slotIndex;

        if (waveText != null)
        {
            int displayWave = info.currentWave + 1;
            waveText.text = info.isEmpty ? "Empty Slot" : $"Wave {displayWave}";
        }

        if (saveTimeText != null)
            saveTimeText.text = info.isEmpty ? "-" : info.lastModified.ToString("yyyy-MM-dd HH:mm:ss");

        if (aetherText != null)
            aetherText.text = info.isEmpty ? "에테르: -" : $"에테르: {info.aetherAmount}";

        if (minimapImage != null)
        {
            if (!info.isEmpty && info.currentMapSeed != 0)
            {
                Texture2D minimapTexture = GetOrCreateMinimapTexture(slotIndex);
                if (minimapTexture != null)
                {
                    _currentTexture = minimapTexture;
                    minimapImage.texture = minimapTexture;
                    minimapImage.gameObject.SetActive(true);
                }
                else
                {
                    minimapImage.gameObject.SetActive(false);
                }
            }
            else
            {
                minimapImage.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 캐시에서 미니맵 텍스처를 가져오거나, 없으면 생성하여 캐시에 저장
    /// </summary>
    private Texture2D GetOrCreateMinimapTexture(int slotIndex)
    {
        if (_minimapCache.TryGetValue(slotIndex, out var texture))
        {
            return texture;
        }

        Texture2D newTexture = GenerateMinimapWithRenderTexture(slotIndex);
        if (newTexture != null)
        {
            _minimapCache[slotIndex] = newTexture;
        }

        return newTexture;
    }

    /// <summary>
    /// RenderTexture를 사용하여 맵/타워/원소를 실제로 렌더링해서 미니맵 생성
    /// </summary>
    private Texture2D GenerateMinimapWithRenderTexture(int slotIndex)
    {
        GameSaveDataInfo saveData = GameSaveManager.Instance?.GetSaveDataInfo(slotIndex);
        if (saveData == null)
        {
            Debug.LogWarning($"SaveSlotUI: 슬롯 {slotIndex}의 세이브 데이터를 로드할 수 없습니다.");
            return null;
        }

        GameObject tempContainer = new GameObject("TempMinimapContainer");
        tempContainer.transform.position = new Vector3(10000, 10000, 0); // 화면 밖으로 이동

        try
        {
            GameObject mapObj = new GameObject("TempMap");
            mapObj.transform.SetParent(tempContainer.transform);
            MapGenerator mapGen = mapObj.AddComponent<MapGenerator>();

            int[,] mapTiles = mapGen.generateMap(saveData.currentMapSeed);
            if (mapTiles == null)
            {
                Debug.LogError("SaveSlotUI: 맵 생성 실패");
                return null;
            }

            int mapSize = mapTiles.GetLength(0);

            Dictionary<(int x, int y), Tile> tileMap = CreateTileMap(mapObj.transform, mapTiles);

            if (saveData.elements != null)
            {
                PlaceElements(saveData.elements, tileMap, tempContainer.transform);
            }
            if (saveData.towers != null)
            {
                PlaceTowers(saveData.towers, tileMap, tempContainer.transform);
            }

            float mapCenterX = 10000 + (mapSize - 1) * 0.5f;
            float mapCenterY = 10000 + (mapSize - 1) * 0.5f;

            GameObject camObj = new GameObject("TempMinimapCamera");
            camObj.transform.SetParent(tempContainer.transform);
            camObj.transform.position = new Vector3(mapCenterX, mapCenterY, -10);

            Camera minimapCam = camObj.AddComponent<Camera>();
            minimapCam.orthographic = true;
            minimapCam.orthographicSize = (mapSize / 2f);
            minimapCam.cullingMask = minimapLayerMask;
            minimapCam.clearFlags = CameraClearFlags.SolidColor;
            minimapCam.backgroundColor = Color.black;

            RenderTexture renderTexture = new RenderTexture(minimapResolution, minimapResolution, 24);
            minimapCam.targetTexture = renderTexture;
            minimapCam.Render();

            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(minimapResolution, minimapResolution, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, minimapResolution, minimapResolution), 0, 0);
            texture.Apply();

            FlipTextureVertically(texture);
            RenderTexture.active = null;
            renderTexture.Release();
            Destroy(renderTexture);

            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveSlotUI: 미니맵 생성 중 오류 발생: {e.Message}\n{e.StackTrace}");
            return null;
        }
        finally
        {
            // 임시 오브젝트 삭제
            if (tempContainer != null)
            {
                Destroy(tempContainer);
            }
        }
    }

    /// <summary>
    /// 맵 타일을 생성하고 딕셔너리로 반환
    /// </summary>
    private Dictionary<(int x, int y), Tile> CreateTileMap(Transform parent, int[,] mapTiles)
    {
        var tileMap = new Dictionary<(int x, int y), Tile>();
        int mapSize = mapTiles.GetLength(0);

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                tileObj.transform.SetParent(parent);
                tileObj.transform.position = new Vector3(10000 + y, 10000 + x, 0);

                SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
                sr.sprite = CreateSquareSprite(mapTiles[x, y] == 1 ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.8f, 0.8f, 0.8f));
                sr.sortingOrder = 0;

                Tile tile = tileObj.AddComponent<Tile>();
                tile.x = x;
                tile.y = y;

                tileMap[(x, y)] = tile;
            }
        }

        return tileMap;
    }

    /// <summary>
    /// 단색 사각형 스프라이트 생성
    /// </summary>
    private Sprite CreateSquareSprite(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    /// <summary>
    /// 원소 배치
    /// </summary>
    private void PlaceElements(List<ElementSaveInfo> elements, Dictionary<(int x, int y), Tile> tileMap, Transform parent)
    {
        foreach (var elementInfo in elements)
        {
            if (!tileMap.TryGetValue((elementInfo.tileX, elementInfo.tileY), out Tile tile))
                continue;

            ElementType elementType = (ElementType)elementInfo.elementType;
            if (elementType < 0 || (int)elementType >= elementPrefabs.Length || elementPrefabs[(int)elementType] == null)
                continue;

            GameObject elementObj = Instantiate(elementPrefabs[(int)elementType], tile.transform.position, Quaternion.identity, parent);
            tile.element = elementObj;
        }
    }

    /// <summary>
    /// 타워 배치
    /// </summary>
    private void PlaceTowers(List<TowerSaveInfo> towers, Dictionary<(int x, int y), Tile> tileMap, Transform parent)
    {
        foreach (var towerInfo in towers)
        {
            if (!tileMap.TryGetValue((towerInfo.tileX, towerInfo.tileY), out Tile tile))
                continue;

            TowerData towerData = GetTowerDataById(towerInfo.towerId);
            if (towerData == null)
            {
                Debug.LogWarning($"SaveSlotUI: TowerData {towerInfo.towerId}를 찾을 수 없습니다.");
                continue;
            }

            if (!_towerPrefabMap.TryGetValue(towerData.ElementType, out GameObject towerPrefab) || towerPrefab == null)
            {
                Debug.LogWarning($"SaveSlotUI: ElementType {towerData.ElementType}의 프리팹이 설정되지 않았습니다.");
                continue;
            }

            GameObject towerObj = Instantiate(towerPrefab, tile.transform.position, Quaternion.identity, parent);
            tile.tower = towerObj;

            Tower towerComponent = towerObj.GetComponent<Tower>();
            if (towerComponent != null)
            {
                towerComponent.Setup(towerData);
            }
        }
    }

    /// <summary>
    /// 타워 ID로 TowerData 찾기
    /// </summary>
    private TowerData GetTowerDataById(string towerId)
    {
        if (allTowerData == null) return null;

        foreach (var data in allTowerData)
        {
            if (data.name == towerId)
                return data;
        }

        return null;
    }

    /// <summary>
    /// 텍스처를 Y축으로 뒤집기
    /// </summary>
    private void FlipTextureVertically(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        Color[] pixels = texture.GetPixels();
        Color[] flippedPixels = new Color[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flippedPixels[x + y * width] = pixels[x + (height - y - 1) * width];
            }
        }

        texture.SetPixels(flippedPixels);
        texture.Apply();
    }

    /// <summary>
    /// 미니맵 캐시를 완전히 정리
    /// </summary>
    public static void ClearMinimapCache()
    {
        foreach (var texture in _minimapCache.Values)
        {
            if (texture != null)
            {
                Destroy(texture);
            }
        }
        _minimapCache.Clear();
    }
}
