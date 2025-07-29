using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopUpManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static PopUpManager Instance { get; private set; }

    private void Awake()
    {
        // 인스턴스 중복 생성 방지
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 씬 전환 시 파괴되지 않도록
        DontDestroyOnLoad(gameObject);
        Debug.Log("PopUpManager 인스턴스가 성공적으로 초기화됨. 씬 전환 시 유지됨");
        // 팝업들을 관리한 초기화 로직을 이곳에 추가할 수 있음
        InitializePopUpSystem();
    }

    private void InitializePopUpSystem()
    {
        Debug.Log("PopUpManager의 초기 시스템 설정이 진행됨");
    }

    public void OpenPopUp(string popUpType)
    {
        Debug.Log($"팝업 '{popUpType}' 열기 요청을 받았습니다. (아직 기능 미구현)");
    }

    public void CloseCurrentPopUp()
    {
        Debug.Log("현재 팝업 닫기 요청을 받았습니다. (아직 기능 미구현)");
    }
}
