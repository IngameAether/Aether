using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerDragSale : MonoBehaviour
{
    private bool isDrag = false;
    private Vector3 offset;
    private Vector3 initialPosition; // �巡�� �� �ʱ� ��ġ ����
    private SaleController saleZone;
    private bool isOverSaleZone; // �Ǹ� ���� ���� �ִ��� �Ǵ�

    public Tile selectTile; // �� Ÿ���� ��ġ�� Ÿ�� ����

    private void Awake()
    {
        Debug.Log("TowerDragSale Awake �Լ� ȣ���! SaleController ã�� �õ�.");
        saleZone = FindObjectOfType<SaleController>();
        if (saleZone == null)
        {
            Debug.LogError("SaleController�� ���Ե� ������Ʈ Ȱ��ȭ �ʿ�");
        }
        else
        {
            Debug.Log("SaleController�� ���������� ã�ҽ��ϴ�!"); // �� �αװ� �ߴ��� Ȯ��
        }
    }

    private void OnMouseDown()
    {
        isDrag = true;
        initialPosition = transform.position; // ���� ��ġ�� �ʱ� ��ġ�� ����

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

            if (saleZone != null && saleZone.RectTransform != null)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                isOverSaleZone = RectTransformUtility.RectangleContainsScreenPoint(saleZone.RectTransform, screenPos, null);
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
            if (isOverSaleZone) // �Ǹ� Ȯ����
            {
                // Ÿ�Ͽ��� Ÿ������ ����
                if (selectTile != null)
                {
                    selectTile.tower = null;
                    selectTile.isElementBuild = false; // Ÿ�� ������� ǥ��
                }
                Destroy(gameObject);
                SaleController.coin += 20;
            }
            else // �Ǹ� ��� ��
            {
                transform.position = initialPosition; // �ʱ� ��ġ�� �̵�
            }
        }

        // �巡�� ���߸� �Ǹ�â ����
        if (saleZone != null)
        {
            saleZone.SetHighlightColor(false);
            saleZone.ShowSaleUI(false);
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