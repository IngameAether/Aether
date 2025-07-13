﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Towers.Core
{

    /// <summary>
    /// 타워 정보와 사거리를 표시하는 UI 매니저
    /// </summary>
    public class TowerInfoDisplay : MonoBehaviour
    {
        [Header("Info Panel UI")] [SerializeField]
        private Canvas mainCanvas;

        [SerializeField] private GameObject infoPanelUI;
        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text towerDescriptionText;
        [SerializeField] private TMP_Text towerStatsText;
        [SerializeField] private Button flipButton;
        [SerializeField] private Button sellButton; // (나중에 추가)

        //[Header("Range Display")] [SerializeField]
        //private GameObject rangeIndicator;

        //[SerializeField] private SpriteRenderer rangeRenderer;

        [Header("Position Settings")] [SerializeField]
        private Vector2 offsetFromTower = new Vector2(10f, 0f); // 타워로부터의 오프셋 (픽셀 단위)

        [SerializeField] private Vector2 screenMargin = new Vector2(50f, 50f);

        private Tower currentSelectedTower;
        private Camera _camera;
        private RectTransform _infoPanelRect;

        private void Start()
        {
            _camera = Camera.main;
            InitializeUI();
        }

        /// <summary>
        /// 빈 공간 클릭 감지용
        /// </summary>
        private void Update()
        {
            // 마우스 왼쪽 버튼 클릭했을 때
            if (!Input.GetMouseButtonDown(0)) return;

            Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // 타일 클릭해 타워가 막 배치된 상태인 경우 UI 안 뜨게
            if (TileInteraction.isTowerJustCreated)
            {
                TileInteraction.isTowerJustCreated = false;
                return;
            }

            // 타워가 아닌 다른 곳을 클릭했으면 UI 숨기기
            if (hit.collider == null)
            {
                HideUI();
            }
            else if (hit.collider.TryGetComponent(out Tower tower))
            {
                tower.HandleTowerClicked();
            }
        }

        private void OnEnable()
        {
            Tower.OnTowerClicked += ShowTowerInfoAndRange;
        }

        private void OnDisable()
        {
            Tower.OnTowerClicked -= ShowTowerInfoAndRange;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            infoPanelUI.SetActive(false);
            //rangeIndicator.SetActive(false);

            flipButton.onClick.AddListener(FlipSelectedTower);

            _infoPanelRect = infoPanelUI.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 타워 정보 표시
        /// </summary>
        private void ShowTowerInfoAndRange(Tower tower)
        {
            currentSelectedTower = tower;

            PositionInfoPanelNearTower(tower);

            // 타워 정보 표시
            ShowTowerInfo(tower);

            // 사거리 표시
            //ShowRange(tower);
        }

        /// <summary>
        /// 정보창을 타워 밑에 위치시키는 함수
        /// </summary>
        private void PositionInfoPanelNearTower(Tower tower)
        {
            Vector3 towerWorldPos = tower.transform.position;
            Vector3 towerScreenPos = _camera.WorldToScreenPoint(towerWorldPos);
            Vector2 targetScreenPos = new Vector2(towerScreenPos.x, towerScreenPos.y) + offsetFromTower;
            Vector2 panelSize = _infoPanelRect.sizeDelta;

            float minX = screenMargin.x + panelSize.x * 0.5f;
            float maxX = Screen.width - screenMargin.x - panelSize.x * 0.5f;

            //targetScreenPos.x = Mathf.Clamp(targetScreenPos.x,
            //    screenMargin.x + panelSize.x * 0.5f,
            //    Screen.width - screenMargin.x - panelSize.x * 0.5f);

            // 화면 오른쪽으로 정보창 벗어나면 타워 왼쪽으로 위치
            if (targetScreenPos.x > maxX) targetScreenPos.x = towerScreenPos.x - offsetFromTower.x;

            //targetScreenPos.y = Mathf.Clamp(targetScreenPos.y,
            //    screenMargin.y + panelSize.y * 0.5f,
            //    Screen.height - screenMargin.y - panelSize.y * 0.5f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)mainCanvas.transform,
                targetScreenPos,
                mainCanvas.worldCamera,
                out Vector2 localPoint
            );

            _infoPanelRect.localPosition = localPoint;
        }

        /// <summary>
        /// 타워 정보 UI 업데이트
        /// </summary>
        private void ShowTowerInfo(Tower tower)
        {
            var towerSetting = tower.GetTowerSetting();

            towerNameText.text = towerSetting.name;
            towerDescriptionText.text = towerSetting.description;

            towerStatsText.text = $"Rank: {towerSetting.rank}\n" +
                                  $"Atk: {towerSetting.damage}\n" +
                                  $"Atk Delay: {towerSetting.attackDelay}sec\n" +
                                  $"Atk Range: {towerSetting.range}";

            infoPanelUI.SetActive(true);
        }

        /// <summary>
        /// 사거리 표시
        /// </summary>
        //private void ShowRange(Tower tower)
        //{
        //    rangeIndicator.transform.position = tower.GetPosition();

        //    // 사거리에 맞게 크기 조정
        //    float range = tower.GetTowerSetting().range;
        //    rangeIndicator.transform.localScale = Vector3.one * (range * 1.85f);

        //    rangeIndicator.SetActive(true);
        //}

        /// <summary>
        /// 선택된 타워 좌우반전
        /// </summary>
        private void FlipSelectedTower()
        {
            if (currentSelectedTower != null)
            {
                currentSelectedTower.FlipTower();
            }
        }

        /// <summary>
        /// UI 숨기기
        /// </summary>
        public void HideUI()
        {
            infoPanelUI.SetActive(false);
            //rangeIndicator.SetActive(false);

            currentSelectedTower = null;
        }
    }
}
