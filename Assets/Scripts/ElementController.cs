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

    void Start()
    {
        saleZone = FindObjectOfType<SaleController>();
    }

    void OnMouseDown()
    {
        isDrag = true;
        // ���콺�� ������Ʈ ������ ���� �����ؼ� �ڿ������� �巡���ϱ� ����
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
            isOver = RectTransformUtility.RectangleContainsScreenPoint(saleZone.RectTransform, screenPos, Camera.main);

            saleZone.SetHighlightColor(isOver);
        }
    }

    void OnMouseUp()
    {
        if (isDrag)
        {
            if (!isOver)
            {
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
        // ���콺�� position ��������
        Vector3 mousePosition = Input.mousePosition;
        // ��Ȯ�� ���� ��ǥ�� ��ȯ�ϱ� ����
        mousePosition.z = -Camera.main.transform.position.z; ;
        // Main Camera ��ǥ�踦 �����Ͽ� ���콺�� ���� ��ǥ ���
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }
}
