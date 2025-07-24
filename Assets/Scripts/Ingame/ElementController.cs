using System.Collections;
using System.Collections.Generic;
using TMPro;
using Towers.Core;
using UnityEngine;

public class ElementController : MonoBehaviour
{
    private bool isDrag = false;
    private Vector3 offset;
    private Vector3 initialPosition;
    public TileInteraction tileInteraction;
    public ElementType type;
    public bool isClick = false;
    public Tile selectTile;

    void Start() { }

    public void Initialize(TileInteraction tileInteraction, ElementType elementType)
    {
        this.type = elementType;
        this.tileInteraction = tileInteraction;
    }

    void OnMouseDown()
    {
        isDrag = true;
        initialPosition = transform.position;

        Vector3 mousePosition = GetMouseWorldPosition();
        offset = transform.position - mousePosition;

        if (isClick)
        {
            Debug.LogError("isClick True인데???????");
            return;
        }
        isClick = true;
        Debug.LogWarning("원소 클릭함");
        TowerCombiner.Instance.SelectElement(this);
    }

    void OnMouseDrag()
    {
        if (isDrag)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            transform.position = mousePosition + offset;
        }
    }

    void OnMouseUp()
    {
        if (isDrag)
        {
            transform.position = initialPosition;
        }
        isDrag = false;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z; ;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }

    public void DeselectElement()
    {
        isClick = false;
    }
}