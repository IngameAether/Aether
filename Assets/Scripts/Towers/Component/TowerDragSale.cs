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
        Debug.Log("TowerDragSale Awake 함수 호출됨! SaleController 찾기 시도.");
        saleZone = FindObjectOfType<SaleController>();
        if (saleZone == null)
        {
            Debug.LogError("SaleController가 포함된 오브젝트를 씬에 추가하거나 활성화해야 합니다.");
        }
        else
        {
            Debug.Log("SaleController를 성공적으로 찾았습니다!");
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
        MagicBookManager.Instance.OnBookEffectApplied += HandleBookEffectApplied;
    }

    private void OnDisable()
    {
        MagicBookManager.Instance.OnBookEffectApplied -= HandleBookEffectApplied;
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        // 마우스 클릭 시에는 드래그 상태가 아니라고 초기화
        IsDrag = false;
        initialPosition = transform.position; // 현재 위치를 초기 위치로 저장
        _mouseDownPosition = GetMouseWorldPosition();
        offset = transform.position - _mouseDownPosition;

        // OnMouseDown에서는 UI를 띄우지 않고 드래그 시작만 준비
        // saleZone.ShowSaleUI(true); // 이 부분을 제거했습니다.
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
            // 드래그가 아닌 단순 클릭인 경우, 이 부분에서는 아무것도 하지 않음.
            // (TowerSelectable 스크립트가 UI를 관리하도록 설계되어 있어야 함)
            else
            {
                // 단순히 타워를 클릭했을 때의 로직 (예: 타워 정보 UI)이
                // 이 스크립트의 OnMouseUp이 아닌 다른 스크립트에서 처리되도록 해야 합니다.
                // 만약 이 스크립트에서 처리해야 한다면 여기에 추가 로직을 넣습니다.
                // 예를 들어, GetComponent<TowerSelectable>().ShowInfoUI(); 등을 호출할 수 있습니다.
            }
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

    private void HandleBookEffectApplied(EBookEffectType bookEffectType, int value)
    {
        if (bookEffectType != EBookEffectType.SellBonus) return;
        _towerSellBonusCoin = value;
    }
}
