using UnityEngine;

public class ElementController : MonoBehaviour, ISelectable
{
    public Tile parentTile;
    public ElementType type;
    public bool isClick = false;

    void Start() { }

    public void Initialize(Tile parentTile, ElementType elementType)
    {
        this.type = elementType;
        this.parentTile = parentTile;
    }

    void OnMouseDown()
    {

        Debug.LogWarning("원소 클릭함: TowerCombiner로 전달");
        TowerCombiner.Instance.SelectItem(this);
    }

    // TowerCombiner가 원소를 선택/ 선택 해제 할 때 호출하는 메소드
    public void SetSelected(bool selected)
    {
        isClick = selected;
        if(parentTile != null)
        {
            parentTile.ApplyHighlight(selected);
        }
    }
    public void DeselectElement()
    {
        SetSelected(false); // isClick을 false로 설정하고 하이라이트 해제
    }

    #region ISelectable

    public ElementType GetElementType()
    {
        return type;
    }

    public int GetLevel()
    {
        return 0; // 원소는 레벨 0
    }

    public Tile GetTile()
    {
        return parentTile;
    }
    public string GetName()
    {
        return name;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #endregion
}
