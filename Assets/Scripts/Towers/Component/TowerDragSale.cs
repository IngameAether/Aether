using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerDragSale : MonoBehaviour
{
    // 타워가 드래그되고 있는지 외부에서 확인하기 위한 속성
    public bool IsDrag { get; private set; } = false;

    private bool _isDragging = false;
    private Vector3 _mouseDownPosition;
    private SaleController saleZone;
    private bool isOverSaleZone; // 판매 구역 위에 있는지 판단

    private Camera _camera;
    public Tile selectTile; // 이 타워가 배치된 타일 참조
    private BoxCollider2D _boxCollider2D;
    private int _towerSellBonusCoin = 0;

    // 드래그 시 보여줄 가짜(Ghost) 오브젝트 관련 변수
    private GameObject _dragGhost;
    private SpriteRenderer[] _allRenderers; // 내 타워의 모습을 복사하기 위함

    // 드래그로 인식될 최소 거리
    private const float DRAG_THRESHOLD = 0.1f;

    private void Awake()
    {
        saleZone = FindObjectOfType<SaleController>();

        // 내 스프라이트 렌더러를 찾아둡니다.
        _allRenderers = GetComponentsInChildren<SpriteRenderer>();
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
        // 비활성화 시 잔상이 남아있으면 삭제
        DestroyGhost();
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        // 마우스 클릭 시에는 드래그 상태가 아니라고 초기화
        IsDrag = false;

        // 마우스 클릭 위치만 저장
        _mouseDownPosition = GetMouseWorldPosition();

        // Ghost는 마우스 위치에 바로 붙일 예정

        // 터치/클릭하자마자 정보창을 띄웁니다.
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
                    CreateGhost();

                    // [핵심 변경] 모든 렌더러를 끕니다.
                    SetRenderersEnabled(false);

                    if (saleZone != null)
                    {
                        saleZone.ShowSaleUI(true);
                    }
                }
            }

            // 실제 드래그 중일 때만 타워 이동 및 판매 영역 체크
            if (IsDrag)
            {
                // 본체(transform)가 아니라 Ghost를 이동시킵니다.
                if (_dragGhost != null)
                {
                    _dragGhost.transform.position = currentMousePosition;
                }

                // 판매 구역 체크 로직
                if (saleZone != null && saleZone.SalePanelRectTransform != null)
                {
                    // 마우스 화면 좌표 기준 체크
                    Vector2 screenPos = Input.mousePosition;
                    isOverSaleZone = RectTransformUtility.RectangleContainsScreenPoint(saleZone.SalePanelRectTransform, screenPos, null);
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
        // 손을 떼면 정보창을 끕니다.
        if (TowerInfoDisplay.Instance != null)
        {
            TowerInfoDisplay.Instance.HideUI();
        }

        if (_isDragging)
        {
            if (IsDrag)
            {
                if (saleZone != null)
                {
                    saleZone.SetHighlightColor(false);
                    saleZone.ShowSaleUI(false);
                }

                if (isOverSaleZone) // 판매 확정 시
                {
                    SellTower();
                }
                else 
                {
                    // 판매 취소 시 모든 렌더러를 다시 킵니다.
                    SetRenderersEnabled(true);
                }
            }
        }

        // 드래그 종료 시 Ghost 삭제
        DestroyGhost();

        // 상태 리셋
        _isDragging = false;
        IsDrag = false;
    }

    // 모든 렌더러의 활성 상태를 설정하는 헬퍼 함수
    private void SetRenderersEnabled(bool isEnabled)
    {
        if (_allRenderers == null) return;
        foreach (var sr in _allRenderers)
        {
            if (sr != null) sr.enabled = isEnabled;
        }
    }

    // Ghost 생성 함수
    private void CreateGhost()
    {
        if (_dragGhost != null) return;

        // Ghost의 부모 오브젝트 생성
        _dragGhost = new GameObject($"{gameObject.name}_GhostParent");
        _dragGhost.transform.position = transform.position;
        _dragGhost.transform.localScale = transform.localScale;
        _dragGhost.transform.rotation = transform.rotation;

        if (_allRenderers == null) return;

        // 모든 렌더러를 순회하며 Ghost 자식으로 복제
        foreach (SpriteRenderer originalSr in _allRenderers)
        {
            if (originalSr == null) continue;

            // 각 부품에 해당하는 Ghost 자식 오브젝트 생성
            GameObject ghostPart = new GameObject($"{originalSr.gameObject.name}_GhostPart");
            ghostPart.transform.SetParent(_dragGhost.transform);

            // 원래 오브젝트와의 상대적인 위치와 회전을 맞춤
            ghostPart.transform.localPosition = originalSr.transform.localPosition;
            ghostPart.transform.localRotation = originalSr.transform.localRotation;
            ghostPart.transform.localScale = originalSr.transform.localScale;

            // SpriteRenderer 추가 및 속성 복사
            SpriteRenderer ghostSr = ghostPart.AddComponent<SpriteRenderer>();
            ghostSr.sprite = originalSr.sprite;
            ghostSr.color = new Color(originalSr.color.r, originalSr.color.g, originalSr.color.b, 0.6f); // 반투명
            ghostSr.sortingLayerID = originalSr.sortingLayerID;
            // 원래 순서를 유지하되, 전체적으로 UI처럼 위에 뜨게 하기 위해 큰 값을 더함
            ghostSr.sortingOrder = originalSr.sortingOrder + 200;
            ghostSr.flipX = originalSr.flipX;
            ghostSr.flipY = originalSr.flipY;
        }
    }

    // Ghost 삭제 함수
    private void DestroyGhost()
    {
        if (_dragGhost != null)
        {
            Destroy(_dragGhost);
            _dragGhost = null;
        }
    }

    // 판매 로직 분리 (기존 로직 동일)
    private void SellTower()
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

        var towerSelectable = GetComponent<TowerSelectable>();
        if (towerSelectable != null)
        {
            towerSelectable.SetTile(tile);
        }
    }

    private void HandleBookEffectApplied(BookEffect effect, float finalValue)
    {
        if (effect.EffectType != EBookEffectType.SellBonus) return;
        _towerSellBonusCoin = (int)finalValue;
    }
}
