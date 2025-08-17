using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerDragSale : MonoBehaviour
{
    public bool IsDrag { get; private set; } = false;

    private bool _isDragging = false;
    private Vector3 offset;
    private Vector3 initialPosition; // 드래그 시 초기 위치 저장
    private Vector3 _mouseDownPosition;
    private SaleController saleZone;
    private bool isOverSaleZone; // 판매 구역 위에 있는지 판단

    private Camera _camera;
    public Tile selectTile; // 이 타워가 배치된 타일 참조
    private BoxCollider2D _boxCollider2D;
    private int _towerSellBonusCoin = 0;

    private void Awake()
    {
        Debug.Log("TowerDragSale Awake 함수 호출됨! SaleController 찾기 시도.");
        saleZone = FindObjectOfType<SaleController>();
        if (saleZone == null)
        {
            Debug.LogError("SaleController가 포함된 오브젝트 활성화 필요");
        }
        else
        {
            Debug.Log("SaleController를 성공적으로 찾았습니다!"); // 이 로그가 뜨는지 확인
        }
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
        MagicBookManager.OnBookEffectApplied += HandleBookEffectApplied;
    }

    private void OnDisable()
    {
        MagicBookManager.OnBookEffectApplied -= HandleBookEffectApplied;
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        IsDrag = false;
        initialPosition = transform.position; // 현재 위치를 초기 위치로 저장
        _mouseDownPosition = GetMouseWorldPosition();
        offset = transform.position - _mouseDownPosition;

        if (saleZone != null)
        {
            saleZone.ShowSaleUI(true);
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
                if (distance > 0.1)
                {
                    IsDrag = true;

                    // 드래그 시작시에만 판매 UI 표시
                    if (saleZone != null)
                    {
                        saleZone.ShowSaleUI(true);
                    }
                }
            }

            // 실제 드래그 중일 때만 타워 이동 및 판매 영역 체크
            if (_isDragging)
            {
                transform.position = currentMousePosition + offset;

                if (saleZone != null && saleZone.SalePanelRectTransform != null)
                {
                    Vector2 screenPos = _camera.WorldToScreenPoint(transform.position);
                    isOverSaleZone =
                        RectTransformUtility.RectangleContainsScreenPoint(saleZone.SalePanelRectTransform, screenPos,
                            null);
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
        if (_isDragging)
        {
            // 실제 드래그했을 때만 판매 로직 처리
            if (IsDrag)
            {
                if(saleZone != null)
                {
                    saleZone.SetHighlightColor(false);
                    saleZone.ShowSaleUI(false);
                }

                if (isOverSaleZone) // 판매 확정시
                {
                    if (selectTile != null)
                    {
                        _boxCollider2D.enabled = true;
                        selectTile.tower = null;
                        selectTile.element = null;
                        selectTile.isElementBuild = true;
                    }
                    Debug.Log($"{gameObject.name} 판매됨");
                    ResourceManager.Instance.AddCoin(20 + _towerSellBonusCoin);
                    Destroy(gameObject);
                }
                else // 판매 취소 시
                {
                    transform.position = initialPosition;
                }
            }
            // 단순 클릭인 경우 아무것도 하지 않음 (TowerSelectable에서 처리)
        }

        // 상태 리셋
        _isDragging = false;
        IsDrag = false;
    }

    Vector3 GetMouseWorldPosition()
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

    private void HandleBookEffectApplied(EBookEffectType bookEffectType, int value)
    {
        if (bookEffectType != EBookEffectType.SellBonus) return;
        _towerSellBonusCoin = value;
    }
}
