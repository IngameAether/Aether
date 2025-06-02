using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementController : MonoBehaviour
{
    private bool isDrag = false;
    private Vector3 offset;
    private SaleController saleZone;
    bool isOver;
    public Tile selectTile;

    void Start()
    {
        saleZone = FindObjectOfType<SaleController>();
    }

    void OnMouseDown()
    {
        isDrag = true;
        // 마우스와 오브젝트 사이의 간격 유지해서 자연스럽게 드래그하기 위함
        Vector3 mousePosition = GetMouseWorldPosition();
        offset = transform.position - mousePosition;
    }

    void OnMouseDrag()
    {
        if (isDrag)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition + offset;

            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            isOver = RectTransformUtility.RectangleContainsScreenPoint(saleZone.RectTransform, screenPos, null);

            saleZone.SetHighlightColor(isOver);
        }
    }

    void OnMouseUp()
    {
        if (isDrag)
        {
            if (isOver)
            {
                if (selectTile != null)
                {
                    selectTile.isElementBuild = true;
                    selectTile.element = null;
                }
                Destroy(gameObject);
                saleZone.coin += 10;
                saleZone.coinTxt.text = saleZone.coin.ToString();
            }
        }
        saleZone?.SetHighlightColor(false);
        isDrag = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        // 마우스의 position 가져오기
        Vector3 mousePosition = Input.mousePosition;
        // 정확한 월드 좌표로 변환하기 위해
        mousePosition.z = -Camera.main.transform.position.z; ;
        // Main Camera 좌표계를 적용하여 마우스의 월드 좌표 얻기
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }
}
