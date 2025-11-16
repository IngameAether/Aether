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
    public Color pathColor = new Color(0.3f, 0.3f, 0.3f);
    public Color wallColor = new Color(0.8f, 0.8f, 0.8f);
    public int minimapScale = 8;

    private static Dictionary<int, Texture2D> _minimapCache = new Dictionary<int, Texture2D>();
    private Texture2D _currentTexture = null;

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
    }

    private void OnDestroy()
    {
        _currentTexture = null;
    }

    public void UpdateUI(SaveSlot info)
    {
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
                Texture2D minimapTexture = GetOrCreateMinimapTexture(info.currentMapSeed);
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
    private Texture2D GetOrCreateMinimapTexture(int seed)
    {
        // 캐시에 이미 있으면 반환
        if (_minimapCache.ContainsKey(seed))
        {
            return _minimapCache[seed];
        }

        Texture2D newTexture = GenerateMinimapTexture(seed);
        if (newTexture != null)
        {
            _minimapCache[seed] = newTexture;
        }

        return newTexture;
    }

    private Texture2D GenerateMinimapTexture(int seed)
    {
        GameObject tempObj = new GameObject("TempMapGenerator");
        MapGenerator mapGen = tempObj.AddComponent<MapGenerator>();

        int[,] mapTiles = mapGen.generateMap(seed);

        Destroy(tempObj);

        if (mapTiles == null)
        {
            Debug.LogError("SaveSlotUI: 미니맵 생성 실패 - mapTiles is null");
            return null;
        }

        int mapSize = mapTiles.GetLength(0);
        int textureSize = mapSize * minimapScale;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                // 1 = path (적이 지나가는 길), 0 = wall (타워 배치 가능)
                Color pixelColor = (mapTiles[x, y] == 1) ? pathColor : wallColor;

                for (int sx = 0; sx < minimapScale; sx++)
                {
                    for (int sy = 0; sy < minimapScale; sy++)
                    {
                        int pixelX = y * minimapScale + sx;
                        int pixelY = (mapSize - 1 - x) * minimapScale + sy;
                        texture.SetPixel(pixelX, pixelY, pixelColor);
                    }
                }
            }
        }

        texture.Apply();
        return texture;
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
