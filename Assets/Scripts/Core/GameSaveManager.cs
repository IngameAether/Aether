using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveSlotInfo
{
    public bool isEmpty;
    public DateTime lastModified;
}

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    public GameSaveData CurrentGameData { get; private set; }

    private const int MAX_SAVE_SLOTS = 3;
    private const string SAVE_FOLDER = "GameSaves";
    private const string FILE_EXTENSION = ".gamesave";
    private string _persistentDataPath;

    private Transform _towerParent;
    private MapGenerator _mapGenerate;
    private WaveManager _waveManager;

    public bool save = false;

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
            GameSaveData saveData = CollectCurrentGameData(slotIndex);
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
    public async Task<GameSaveData> LoadGameAsync(int slotIndex)
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
            GameSaveData loadedData = await Task.Run(() => JsonConvert.DeserializeObject<GameSaveData>(jsonData));

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
    private GameSaveData CollectCurrentGameData(int slotIndex)
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

            GameSaveData saveData = new GameSaveData
            {
                saveSlot = slotIndex,
                gameVersion = Application.version, // 게임 버전 추가
                currentWave = _waveManager ? _waveManager.CurrentWaveLevel : 0,
                playerLife = GameManager.Instance ? GameManager.Instance.currentLives : 0,
                resources = CollectResourceData(),
                currentMapSeed = _mapGenerate ? _mapGenerate.CurrentSeed : 0,
                towers = CollectTowerData(), // 타워 데이터 수집 추가
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

    private List<TowerSetting> CollectTowerData()
    {
        var towers = new List<TowerSetting>();

        if (_towerParent != null)
        {
            // 타워 자식 오브젝트들에서 데이터 수집
            foreach (Transform child in _towerParent)
            {
                var towerComponent = child.GetComponent<Tower>(); // Tower 컴포넌트가 있다고 가정
                if (towerComponent != null)
                {
                    towers.Add(towerComponent.GetTowerSetting());
                }
            }
        }

        return towers;
    }

    /// <summary>
    /// 저장 데이터 유효성 검증
    /// </summary>
    private bool ValidateSaveData(GameSaveData data)
    {
        if (data == null) return false;
        if (string.IsNullOrEmpty(data.gameVersion)) return false;
        if (data.resources == null) return false;
        if (data.towers == null) return false;

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
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(GetSaveDirectoryPath(), $"slot_{slotIndex}{FILE_EXTENSION}");
    }

    /// <summary>
    /// 저장 슬롯 정보 조회
    /// </summary>
    public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);
        if (!File.Exists(filePath)) return new SaveSlotInfo { isEmpty = true };

        try
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return new SaveSlotInfo
            {
                isEmpty = false,
                lastModified = fileInfo.LastWriteTime,
            };
        }
        catch
        {
            return new SaveSlotInfo { isEmpty = true };
        }
    }
}
