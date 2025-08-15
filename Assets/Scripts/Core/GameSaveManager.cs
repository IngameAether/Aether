using System;
using System.Collections.Generic;
using System.IO;
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

    private const int MAX_SAVE_SLOTS = 3;
    private const string SAVE_FOLDER = "GameSaves";
    private const string FILE_EXTENSION = ".gamesave";

    private GameSaveData _currentGameData;

    private SpawnManager _spawnManager;

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

    private void InitializeSaveSystem()
    {
        string savePath = GetSaveDirectoryPath();
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        _spawnManager = FindObjectOfType<SpawnManager>();
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
            string jsonData = await Task.Run(() => JsonConvert.SerializeObject(saveData, Formatting.Indented));

            string filePath = GetSaveFilePath(slotIndex);
            await File.WriteAllTextAsync(filePath, jsonData);

            _currentGameData = saveData;
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
            Debug.LogError($"GameSaveManager: Invalid save slot: {slotIndex}");
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
                _currentGameData = loadedData;
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
        // 일단 없는 데이터는 그냥 Empty로 설정
        GameSaveData saveData = new GameSaveData
        {
            saveSlot = slotIndex,
            currentWave = _spawnManager.currentWaveLevel,
            playerLife = GameManager.Instance.currentLives,
            resources = new ResourceData(),
            currentMapId = SceneManager.GetActiveScene().name,
            towers = new List<TowerSaveData>()
        };

        return saveData;
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
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
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
