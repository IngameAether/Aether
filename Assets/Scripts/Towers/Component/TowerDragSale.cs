using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerDragSale : MonoBehaviour
{
    private bool isDrag = false;
    private Vector3 offset;
    private Vector3 initialPosition; // 드래그 시 초기 위치 저장
    private SaleController saleZone;
    private bool isOverSaleZone; // 판매 구역 위에 있는지 판단

    public Tile selectTile; // 이 타워가 배치된 타일 참조
    private BoxCollider2D _boxCollider2D;

    private void Start()
    {
        _boxCollider2D = selectTile.GetComponent<BoxCollider2D>();
    }
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

    private void OnMouseDown()
    {
        isDrag = true;
        initialPosition = transform.position; // 현재 위치를 초기 위치로 저장

        Vector3 mousePosition = GetMouseWorldPosition();
        offset = transform.position - mousePosition;

        if (saleZone != null)
        {
            saleZone.ShowSaleUI(true);
        }

    }

    private void OnMouseDrag()
    {
        if (isDrag)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition + offset;

            if (saleZone != null && saleZone.SalePanelRectTransform != null)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
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

    private void OnMouseUp()
    {
        if (isDrag)
        {
            if(saleZone != null)
            {
                saleZone.SetHighlightColor(false); // 드래그 끝나면 하이라이트 해제
                saleZone.ShowSaleUI(false); // 판매창 및 sale 글씨 비활성화
            }

            if (isOverSaleZone) // 판매 확정시
            {
                // 타일에서 타워정보 제거
                if (selectTile != null)
                {
                    _boxCollider2D.enabled = true;
                    selectTile.tower = null;
                    selectTile.element = null;
                    selectTile.isElementBuild = true;
                }
                Debug.Log($"{gameObject.name} 판매됨");
                SaleController.coin += 20;
                Destroy(gameObject);
            }
            else // 판매 취소 시
            {
                transform.position = initialPosition; // 초기 위치로 이동
            }
        }
        isDrag = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }

    //
    public void SetAssignedTile(Tile tile)
    {
        selectTile = tile;
    }
}
