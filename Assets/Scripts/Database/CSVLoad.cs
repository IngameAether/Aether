using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSVLoad : MonoBehaviour
{
    // 다른 스크립트에서 접근할 수 있도록 public static 변수로 선언
    public static List<Dictionary<string, object>> enemyData;
    public static List<Dictionary<string, object>> towerData;
    public static List<Dictionary<string, object>> waveData; 
    public static List<Dictionary<string, object>> gameData;

    void Awake()
    {
        // 실제 파일 이름("파일명")을 정확히 적어서 로드
        enemyData = CSVReader.Read("Enemy_Data");
        towerData = CSVReader.Read("Tower_Data");
        waveData = CSVReader.Read("Wave_Data");
        gameData = CSVReader.Read("Game_Data");
    }
}
