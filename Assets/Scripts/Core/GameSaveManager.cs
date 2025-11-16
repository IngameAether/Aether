using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            StartCoroutine(SaveGame(0));
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
    /// 게임 저장 (코루틴)
    /// </summary>
    public IEnumerator SaveGame(int slotIndex, System.Action<bool> onComplete = null)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"GameSaveManager: Invalid save slot: {slotIndex}");
            onComplete?.Invoke(false);
            yield break;
        }

        bool success = false;
        try
        {
            GameSaveDataInfo saveData = CollectCurrentGameData(slotIndex);
            CurrentGameData = saveData;

            string filePath = GetSaveFilePath(slotIndex);

            // JSON 직렬화
            string jsonData = JsonConvert.SerializeObject(saveData, Formatting.Indented);

            // 파일 저장 (동기 방식 - Unity에서 더 안정적)
            File.WriteAllText(filePath, jsonData);

            Debug.Log($"GameSaveManager: Game saved successfully to slot {slotIndex}");
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"GameSaveManager: Save failed: {e.Message}");
            success = false;
        }

        onComplete?.Invoke(success);
    }

    /// <summary>
    /// 게임 로드 (코루틴)
    /// </summary>
    public IEnumerator LoadGame(int slotIndex, System.Action<GameSaveDataInfo> onComplete = null)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            onComplete?.Invoke(null);
            yield break;
        }

        string filePath = GetSaveFilePath(slotIndex);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"GameSaveManager: No save file found at slot {slotIndex}");
            onComplete?.Invoke(null);
            yield break;
        }

        GameSaveDataInfo loadedData = null;
        try
        {
            // 파일 읽기 (동기 방식)
            string jsonData = File.ReadAllText(filePath);

            // JSON 역직렬화
            loadedData = JsonConvert.DeserializeObject<GameSaveDataInfo>(jsonData);

            if (ValidateSaveData(loadedData))
            {
                CurrentGameData = loadedData;
                Debug.Log($"GameSaveManager: Game loaded successfully from slot {slotIndex}");
            }
            else
            {
                Debug.LogError("GameSaveManager: Save data validation failed");
                loadedData = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GameSaveManager: Load failed: {e.Message}");
            loadedData = null;
        }

        onComplete?.Invoke(loadedData);
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
                elements = CollectElementData(),
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

                    // 타워가 위치한 타일 정보 가져오기
                    var towerDragSale = child.GetComponent<TowerDragSale>();
                    Tile tile = null;
                    if (towerDragSale != null)
                    {
                        tile = towerDragSale.selectTile;
                    }

                    // 타일이 없으면 저장하지 않음 (위치 정보가 없는 타워)
                    if (tile == null)
                    {
                        Debug.LogWarning($"타워 {data.name}의 타일 정보를 찾을 수 없어 저장하지 않습니다.");
                        continue;
                    }

                    TowerSaveInfo saveData = new TowerSaveInfo
                    {
                        towerId = data.name,
                        level = towerComponent.CurrentReinforceLevel,
                        lightReinforceCount = towerComponent.LightReinforceCount,
                        darkReinforceCount = towerComponent.DarkReinforceCount,
                        tileX = tile.x,
                        tileY = tile.y
                    };

                    towerSaveInfoList.Add(saveData);
                }
            }
        }

        return towerSaveInfoList;
    }

    private List<ElementSaveInfo> CollectElementData()
    {
        var elementSaveInfoList = new List<ElementSaveInfo>();

        // 씬에 있는 모든 ElementController 찾기
        ElementController[] allElements = FindObjectsOfType<ElementController>();

        foreach (var element in allElements)
        {
            if (element.parentTile != null)
            {
                ElementSaveInfo saveData = new ElementSaveInfo
                {
                    elementType = (int)element.type,
                    tileX = element.parentTile.x,
                    tileY = element.parentTile.y
                };

                elementSaveInfoList.Add(saveData);
            }
            else
            {
                Debug.LogWarning($"원소 {element.type}의 타일 정보를 찾을 수 없어 저장하지 않습니다.");
            }
        }

        return elementSaveInfoList;
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
        if (data.elements == null) return false;
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
    /// 저장 슬롯의 전체 데이터 조회
    /// </summary>
    public GameSaveDataInfo GetSaveDataInfo(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string jsonData = File.ReadAllText(filePath);
            GameSaveDataInfo loadedData = JsonConvert.DeserializeObject<GameSaveDataInfo>(jsonData);
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"슬롯 {slotIndex} 데이터 읽기 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 저장 슬롯 정보 조회 (UI 표시용 요약 정보)
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
                currentMapSeed = loadedData.currentMapSeed,
                aetherAmount = loadedData.resources.aether
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"슬롯 {slotIndex} 정보 읽기 실패: {e.Message}");
            return new SaveSlot { isEmpty = true };
        }
    }

    /// <summary>
    /// 저장된 원소들을 맵에 복원
    /// </summary>
    private void RestoreElements(List<ElementSaveInfo> elements)
    {
        if (elements == null || elements.Count == 0)
        {
            Debug.Log("복원할 원소가 없습니다.");
            return;
        }

        // 모든 타일을 찾아서 좌표로 매핑
        Tile[] allTiles = FindObjectsOfType<Tile>();
        var tileMap = new System.Collections.Generic.Dictionary<(int x, int y), Tile>();
        foreach (var tile in allTiles)
        {
            tileMap[(tile.x, tile.y)] = tile;
        }

        // staticElementPrefabs 배열에서 ElementType에 맞는 프리팹 찾기
        if (TileInteraction.staticElementPrefabs == null || TileInteraction.staticElementPrefabs.Length == 0)
        {
            Debug.LogError("TileInteraction.staticElementPrefabs가 설정되지 않았습니다.");
            return;
        }

        foreach (var elementInfo in elements)
        {
            // 타일 찾기
            if (!tileMap.TryGetValue((elementInfo.tileX, elementInfo.tileY), out Tile tile))
            {
                Debug.LogWarning($"타일 ({elementInfo.tileX}, {elementInfo.tileY})을 찾을 수 없습니다.");
                continue;
            }

            // ElementType에 맞는 프리팹 찾기
            ElementType targetType = (ElementType)elementInfo.elementType;
            GameObject elementPrefab = null;

            foreach (var prefab in TileInteraction.staticElementPrefabs)
            {
                var ec = prefab.GetComponent<ElementController>();
                if (ec != null && ec.type == targetType)
                {
                    elementPrefab = prefab;
                    break;
                }
            }

            if (elementPrefab == null)
            {
                Debug.LogWarning($"ElementType {targetType}에 맞는 프리팹을 찾을 수 없습니다.");
                continue;
            }

            // 원소 생성
            GameObject elementObj = Instantiate(elementPrefab, tile.transform.position, Quaternion.identity);
            ElementController elementController = elementObj.GetComponent<ElementController>();
            if (elementController != null)
            {
                elementController.Initialize(tile, targetType);
            }

            tile.element = elementObj;
            tile.isElementBuild = false;
        }
    }

    /// <summary>
    /// 로드된 게임 상태를 복원 (맵 생성 후 호출)
    /// </summary>
    public void RestoreGameState()
    {
        if (CurrentGameData == null)
        {
            Debug.Log("복원할 게임 데이터가 없습니다.");
            return;
        }

        // 자원 복원
        if (ResourceManager.Instance != null && CurrentGameData.resources != null)
        {
            ResourceManager.Instance.ResetAllResources();
            ResourceManager.Instance.AddCoin(CurrentGameData.resources.aether);
            ResourceManager.Instance.AddElement(ReinforceType.Light, CurrentGameData.resources.lightElement);
            ResourceManager.Instance.AddElement(ReinforceType.Dark, CurrentGameData.resources.darkElement);
        }

        // 생명력 복원
        if (GameManager.Instance != null)
        {
            int currentLives = GameManager.Instance.currentLives;
            int savedLives = CurrentGameData.playerLife;
            int lifeDiff = savedLives - currentLives;

            if (lifeDiff > 0)
            {
                GameManager.Instance.AddLife(lifeDiff);
            }
            else if (lifeDiff < 0)
            {
                for (int i = 0; i < -lifeDiff; i++)
                {
                    GameManager.Instance.LoseLife();
                }
            }
            Debug.Log($"생명력 복원: {savedLives}");
        }

        // 마법책 복원
        if (MagicBookManager.Instance != null && CurrentGameData.ownedMagicBooks != null)
        {
            MagicBookManager.Instance.RestoreOwnedBooks(CurrentGameData.ownedMagicBooks);
        }

        // 원소 먼저 복원
        RestoreElements(CurrentGameData.elements);

        // 타워 복원
        RestoreTowers(CurrentGameData.towers);

        Debug.Log("게임 상태 복원 완료");
    }

    /// <summary>
    /// 저장된 타워들을 맵에 복원
    /// </summary>
    private void RestoreTowers(List<TowerSaveInfo> towers)
    {
        if (towers == null || towers.Count == 0)
        {
            Debug.Log("복원할 타워가 없습니다.");
            return;
        }

        // 모든 타일을 찾아서 좌표로 매핑
        Tile[] allTiles = FindObjectsOfType<Tile>();
        var tileMap = new Dictionary<(int x, int y), Tile>();
        foreach (var tile in allTiles)
        {
            tileMap[(tile.x, tile.y)] = tile;
        }

        // TowerCombiner 찾기
        TowerCombiner towerCombiner = TowerCombiner.Instance;
        if (towerCombiner == null)
        {
            towerCombiner = FindObjectOfType<TowerCombiner>();
            if (towerCombiner == null)
            {
                Debug.LogError("TowerCombiner를 찾을 수 없습니다. 타워 복원 실패.");
                return;
            }
        }

        if (!_towerParent)
        {
            _towerParent = towerCombiner.towerParent;
        }

        foreach (var towerInfo in towers)
        {
            // 타일 찾기
            if (!tileMap.TryGetValue((towerInfo.tileX, towerInfo.tileY), out Tile tile))
            {
                Debug.LogWarning($"타일 ({towerInfo.tileX}, {towerInfo.tileY})을 찾을 수 없습니다.");
                continue;
            }

            // TowerData 찾기
            TowerData towerData = towerCombiner.GetTowerDataById(towerInfo.towerId);
            if (towerData == null)
            {
                Debug.LogWarning($"TowerData {towerInfo.towerId}를 찾을 수 없습니다.");
                continue;
            }

            // 타워 프리팹 가져오기 (ElementType으로)
            GameObject towerPrefab = towerCombiner.GetTowerPrefabByElementType(towerData.ElementType);
            if (towerPrefab == null)
            {
                Debug.LogWarning($"ElementType {towerData.ElementType}에 해당하는 프리팹을 찾을 수 없습니다.");
                continue;
            }

            // 타일의 원소가 있다면 제거
            if (tile.element != null)
            {
                Destroy(tile.element);
                tile.element = null;
            }

            // 타워 생성
            var tileInteraction = tile.GetComponent<TileInteraction>();
            GameObject towerObj = null;
            if (tileInteraction != null)
            {
                towerObj = tileInteraction.PlacedTower(towerPrefab, tile);
            }
            else
            {
                towerObj = Instantiate(towerPrefab, tile.transform.position, Quaternion.identity);
                tile.tower = towerObj;

                var towerDragSale = towerObj.GetComponent<TowerDragSale>();
                if (towerDragSale != null)
                {
                    towerDragSale.selectTile = tile;
                }

                var towerSelectable = towerObj.GetComponent<TowerSelectable>();
                if (towerSelectable != null)
                {
                    towerSelectable.SetTile(tile);
                }
            }

            if (_towerParent != null)
            {
                towerObj.transform.SetParent(_towerParent);
            }

            // 타워 초기화 (Setup 메서드 사용)
            var towerComponent = towerObj.GetComponent<Tower>();
            if (towerComponent != null)
            {
                towerComponent.Setup(towerData);

                // 강화 레벨 복원
                if (towerInfo.level > 0 || towerInfo.lightReinforceCount > 0 || towerInfo.darkReinforceCount > 0)
                {
                    towerComponent.RestoreReinforcement(
                        towerInfo.level,
                        towerInfo.lightReinforceCount,
                        towerInfo.darkReinforceCount
                    );
                }
            }

            tile.isElementBuild = false;
        }
    }
}
