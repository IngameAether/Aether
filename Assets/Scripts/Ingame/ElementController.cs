using System.Collections;
using System.Collections.Generic;
using TMPro;
using Towers.Core;
using UnityEngine;

public class ElementController : MonoBehaviour
{
    private bool isDrag = false;
    private Vector3 offset;
    private SaleController saleZone;
    public TileInteraction tileInteraction;
    public ElementType type;
    bool isOver;
    private bool isClick;
    public Tile selectTile;

    void Start()
    {
        saleZone = FindObjectOfType<SaleController>();
    }

    public void Initialize(TileInteraction tileInteraction)
    {
        this.tileInteraction = tileInteraction;
    }

    void OnMouseDown()
    {
        isDrag = true;
        // ���콺�� ������Ʈ ������ ���� �����ؼ� �ڿ������� �巡���ϱ� ����
        Vector3 mousePosition = GetMouseWorldPosition();
        offset = transform.position - mousePosition;
        
        if (isClick) return;
        TowerCombiner.Instance.SelectElement(this);
        Debug.LogWarning("원소 클릭함");
        isClick = true;
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
                SaleController.coin += 10;
                TowerCombiner.Instance.ClearSelectedElements();
            }
        }
        saleZone?.SetHighlightColor(false); // if saleZone != null
        isDrag = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        // ���콺�� position ��������
        Vector3 mousePosition = Input.mousePosition;
        // ��Ȯ�� ���� ��ǥ�� ��ȯ�ϱ� ����
        mousePosition.z = -Camera.main.transform.position.z; ;
        // Main Camera ��ǥ�踦 �����Ͽ� ���콺�� ���� ��ǥ ���
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }
}
