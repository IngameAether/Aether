using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    public GameSaveDataInfo CurrentGameData { get; private set; }
    public int SelectedSlotIndex { get; set; } = -1;

    private const int MAX_SAVE_SLOTS = 3;
    private const string SAVE_FOLDER = "GameSaves";
    private const string FILE_EXTENSION = ".gamesave";
    private string _persistentDataPath;

    private Transform _towerParent;
    private MapGenerator _mapGenerate;
    private WaveManager _waveManager;

    public bool save = false;

    [System.Serializable]
    public class TowerSaveData
    {
        public string towerDataID;
        public int reinforceLevel;
        public float positionX;
        public float positionY;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (save)
        {
            _ = SaveGameAsync(0);
            save = false;
        }
    }

    private void InitializeSaveSystem()
    {
        _persistentDataPath = Application.persistentDataPath;
        string savePath = Path.Combine(_persistentDataPath, SAVE_FOLDER);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    /// <summary>
    /// 비동기 게임 저장
    /// </summary>
    public async Task<bool> SaveGameAsync(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"GameSaveManager: Invalid save slot: {slotIndex}");
            return false;
        }

        try
        {
            GameSaveDataInfo saveData = CollectCurrentGameData(slotIndex);
            CurrentGameData = saveData;

            string filePath = GetSaveFilePath(slotIndex);

            string jsonData = await Task.Run(() => JsonConvert.SerializeObject(saveData, Formatting.Indented)).ConfigureAwait(false);

            await File.WriteAllTextAsync(filePath, jsonData).ConfigureAwait(false);

            Debug.Log($"GameSaveManager: Game saved successfully to slot {slotIndex}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"GameSaveManager: Save failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 비동기 게임 로드
    /// </summary>
    public async Task<GameSaveDataInfo> LoadGameAsync(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            return null;
        }

        string filePath = GetSaveFilePath(slotIndex);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"GameSaveManager: No save file found at slot {slotIndex}");
            return null;
        }

        try
        {
            string jsonData = await File.ReadAllTextAsync(filePath);
            GameSaveDataInfo loadedData = await Task.Run(() => JsonConvert.DeserializeObject<GameSaveDataInfo>(jsonData));

            if (ValidateSaveData(loadedData))
            {
                CurrentGameData = loadedData;
                Debug.Log($"GameSaveManager: Game loaded successfully from slot {slotIndex}");
                return loadedData;
            }
            else
            {
                Debug.LogError("GameSaveManager: Save data validation failed");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GameSaveManager: Load failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 현재 게임 상태를 SaveData로 수집
    /// </summary>
    private GameSaveDataInfo CollectCurrentGameData(int slotIndex)
    {
        try
        {
            if (!_towerParent)
            {
                var towerCombiner = FindObjectOfType<TowerCombiner>();
                _towerParent = towerCombiner ? towerCombiner.towerParent : null;
            }

            if (!_waveManager)
                _waveManager = FindObjectOfType<WaveManager>();

            if (!_mapGenerate)
                _mapGenerate = FindObjectOfType<MapGenerator>();

            GameSaveDataInfo saveData = new GameSaveDataInfo
            {
                saveSlot = slotIndex,
                gameVersion = Application.version,
                currentWave = _waveManager ? _waveManager.CurrentWaveLevel : 0,
                playerLife = GameManager.Instance ? GameManager.Instance.currentLives : 0,
                resources = CollectResourceData(),
                currentMapSeed = _mapGenerate ? _mapGenerate.CurrentSeed : 0,
                towers = CollectTowerData(),
                // 획득한 마법책 목록을 MagicBookManager로부터 가져와서 저장 데이터에 추가합니다.
                ownedMagicBooks = MagicBookManager.Instance.GetOwnedBooksDataForSave()
            };

            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"GameSaveManager: 데이터 수집 실패: {e.Message}");
            throw;
        }
    }

    private ResourceData CollectResourceData()
    {
        if (ResourceManager.Instance != null)
        {
            return new ResourceData(
                ResourceManager.Instance.Coin,
                ResourceManager.Instance.LightElement,
                ResourceManager.Instance.DarkElement
            );
        }

        Debug.LogWarning("GameSaveManager: ResourceManager를 찾을 수 없습니다.");
        return new ResourceData(0, 0, 0);
    }

    private List<TowerSaveInfo> CollectTowerData()
    {
        var towerSaveInfoList = new List<TowerSaveInfo>();

        if (_towerParent != null)
        {
            foreach (Transform child in _towerParent)
            {
                var towerComponent = child.GetComponent<Tower>();
                if (towerComponent != null)
                {
                    TowerData data = towerComponent.GetTowerData();
                    if (data == null) continue;

                    // 이제 TowerSaveData가 아닌 TowerSaveInfo 구조체를 사용합니다.
                    TowerSaveInfo saveData = new TowerSaveInfo
                    {
                        towerId = data.name, // TowerSaveInfo의 필드 이름에 맞게 수정
                        level = towerComponent.CurrentReinforceLevel // TowerSaveInfo의 필드 이름에 맞게 수정
                    };

                    towerSaveInfoList.Add(saveData);
                }
            }
        }

        return towerSaveInfoList;
    }

    /// <summary>
    /// 저장 데이터 유효성 검증
    /// </summary>
    private bool ValidateSaveData(GameSaveDataInfo data)
    {
        if (data == null) return false;
        if (string.IsNullOrEmpty(data.gameVersion)) return false;
        // 따라서 null 검사가 필요합니다.
        if (data.towers == null) return false;
        if (data.ownedMagicBooks == null) return false;

        return true;
    }

    /// <summary>
    /// 저장 경로 반환
    /// </summary>
    private string GetSaveDirectoryPath()
    {
        return Path.Combine(_persistentDataPath, SAVE_FOLDER);
    }

    /// <summary>
    /// 저장 파일 경로 반환
    /// </summary>
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(GetSaveDirectoryPath(), $"slot_{slotIndex}{FILE_EXTENSION}");
    }

    /// <summary>
    /// 저장 슬롯 정보 조회
    /// </summary>
    public SaveSlot GetSaveSlot(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);
        if (!File.Exists(filePath))
        {
            return new SaveSlot { isEmpty = true };
        }

        try
        {
            // 전체 데이터를 불러오긴 하지만, UI 표시용이므로 동기 방식으로 간단히 처리합니다.
            string jsonData = File.ReadAllText(filePath);
            GameSaveDataInfo loadedData = JsonConvert.DeserializeObject<GameSaveDataInfo>(jsonData);

            FileInfo fileInfo = new FileInfo(filePath);
            return new SaveSlot
            {
                isEmpty = false,
                lastModified = fileInfo.LastWriteTime,
                currentWave = loadedData.currentWave,
                currentMapSeed = loadedData.currentMapSeed
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"슬롯 {slotIndex} 정보 읽기 실패: {e.Message}");
            return new SaveSlot { isEmpty = true };
        }
    }
}
