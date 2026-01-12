using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerDragSale : MonoBehaviour
{
    // 타워가 드래그되고 있는지 외부에서 확인하기 위한 속성
    public bool IsDrag { get; private set; } = false;

    private bool _isDragging = false;
    private Vector3 offset;
    private Vector3 initialPosition; // 드래그 시작 시 초기 위치 저장
    private Vector3 _mouseDownPosition;
    private SaleController saleZone;
    private bool isOverSaleZone; // 판매 구역 위에 있는지 판단
    
    private Camera _camera;
    public Tile selectTile; // 이 타워가 배치된 타일 참조
    private BoxCollider2D _boxCollider2D;
    private int _towerSellBonusCoin = 0;

    // 드래그로 인식될 최소 거리
    private const float DRAG_THRESHOLD = 0.1f;

    private void Awake()
    {
        saleZone = FindObjectOfType<SaleController>();
    }

    private void Start()
    {
        if (selectTile != null)
        {
            _boxCollider2D = selectTile.GetComponent<BoxCollider2D>();

            // TowerSelectable에 타일 정보 전달
            var towerSelectable = GetComponent<TowerSelectable>();
            if (towerSelectable != null)
            {
                towerSelectable.SetTile(selectTile);
            }
        }
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        // MagicBookManager가 존재할 때만 이벤트를 구독합니다.
        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
        }
    }

    private void OnDisable()
    {
        // MagicBookManager가 존재할 때만 이벤트를 해제합니다.
        if (MagicBookManager.Instance != null)
        {
            MagicBookManager.Instance.OnBookEffectApplied -= HandleBookEffectApplied;
        }
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        // 마우스 클릭 시에는 드래그 상태가 아니라고 초기화
        IsDrag = false;
        initialPosition = transform.position; // 현재 위치를 초기 위치로 저장
        _mouseDownPosition = GetMouseWorldPosition();
        offset = transform.position - _mouseDownPosition;

        // 터치/클릭하자마자 정보창을 띄웁니다.
        // 현재 타워 컴포넌트를 가져와서 넘겨줍니다.
        Tower myTower = GetComponent<Tower>();
        if (TowerInfoDisplay.Instance != null && myTower != null)
        {
            TowerInfoDisplay.Instance.ShowTowerInfo(myTower);
        }
    }

    private void OnMouseDrag()
    {
        if (_isDragging)
        {
            Vector3 currentMousePosition = GetMouseWorldPosition();

            // 마우스가 일정 거리 이상 움직였을 때만 실제 드래그로 판정
            if (!IsDrag)
            {
                float distance = Vector3.Distance(_mouseDownPosition, currentMousePosition);
                // DRAG_THRESHOLD 값을 사용해 드래그 임계값을 조절할 수 있습니다.
                if (distance > DRAG_THRESHOLD)
                {
                    IsDrag = true;

                    // 실제 드래그가 시작될 때만 판매 UI 표시
                    if (saleZone != null)
                    {
                        saleZone.ShowSaleUI(true);
                    }
                }
            }

            // 실제 드래그 중일 때만 타워 이동 및 판매 영역 체크
            if (IsDrag)
            {
                transform.position = currentMousePosition + offset;

                if (saleZone != null && saleZone.SalePanelRectTransform != null)
                {
                    Vector2 screenPos = _camera.WorldToScreenPoint(transform.position);
                    isOverSaleZone =
                        RectTransformUtility.RectangleContainsScreenPoint(saleZone.SalePanelRectTransform, screenPos, null);
                    saleZone.SetHighlightColor(isOverSaleZone);
                }
                else
                {
                    isOverSaleZone = false;
                    saleZone?.SetHighlightColor(false);
                }
            }
        }
    }

    private void OnMouseUp()
    {
        // _isDragging 상태를 먼저 확인
        if (_isDragging)
        {
            // 드래그가 실제로 발생했는지(IsDrag == true) 확인
            if (IsDrag)
            {
                if (saleZone != null)
                {
                    saleZone.SetHighlightColor(false);
                    saleZone.ShowSaleUI(false);
                }

                if (isOverSaleZone) // 판매 확정 시
                {
                    int sellPrice = 0;
                    
                    var tower = GetComponent<Tower>();
                    if (tower != null)
                    {
                        int level = tower.Rank;
                        sellPrice = GameDataDatabase.GetInt($"Lv{level}_tower_sell", 10);
                    }
                    else
                    {
                        var element = GetComponent<ElementController>();
                        if (element != null)
                        {
                            sellPrice = GameDataDatabase.GetInt("element_sell", 0);
                        }
                    }

                    if (selectTile != null)
                    {
                        _boxCollider2D.enabled = true;
                        selectTile.tower = null;
                        selectTile.element = null;
                        selectTile.isElementBuild = true;
                    }
                    Debug.Log($"{gameObject.name} 판매됨. 가격: {sellPrice}");
                    ResourceManager.Instance.AddCoin(sellPrice + _towerSellBonusCoin);
                    Destroy(gameObject);
                }
                else // 판매 취소 시
                {
                    transform.position = initialPosition;
                }
            }
            // 드래그가 아닌 단순 클릭인 경우, 이 부분에서는 아무것도 하지 않음.
            // (TowerSelectable 스크립트가 UI를 관리하도록 설계되어 있어야 함)
        }

        // 손을 떼면 정보창을 끕니다.
        if (TowerInfoDisplay.Instance != null)
        {
            TowerInfoDisplay.Instance.HideUI();
        }

        // 상태 리셋
        _isDragging = false;
        IsDrag = false;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -_camera.transform.position.z;
        mousePosition = _camera.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }

    public void SetAssignedTile(Tile tile)
    {
        selectTile = tile;

        // TowerSelectable에도 타일 정보 전달
        var towerSelectable = GetComponent<TowerSelectable>();
        if (towerSelectable != null)
        {
            towerSelectable.SetTile(tile);
        }
    }

    // 새로운 신호 형식 (BookEffect, float)을 받도록 파라미터를 변경합니다.
    private void HandleBookEffectApplied(BookEffect effect, float finalValue)
    {
        // 효과 타입이 '판매 보너스'가 아니면 아무것도 하지 않고 함수를 종료합니다.
        if (effect.EffectType != EBookEffectType.SellBonus) return;

        // finalValue는 float이지만, 코인 값은 정수여야 하므로 int로 변환해줍니다.
        _towerSellBonusCoin = (int)finalValue;
    }
}
