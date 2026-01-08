using UnityEngine;

/// <summary>
/// 타워를 선택 가능하게 만드는 컴포넌트
/// </summary>
public class TowerSelectable : MonoBehaviour, ISelectable
{
    private Tower tower;
    private bool isSelected = false;
    private Color originalColor;
    private Tile cachedTile;

    void Start()
    {
        tower = GetComponent<Tower>();

        // 타일 참조 캐시
        var towerDragSale = GetComponent<TowerDragSale>();
        if (towerDragSale != null && towerDragSale.selectTile != null)
        {
            cachedTile = towerDragSale.selectTile;
        }
    }

    /// <summary>
    /// InputManager에서 호출하는 클릭 처리 (OnMouseDown 대체)
    /// </summary>
    public void OnClick()
    {
        if (Time.timeScale == 0f) return;

        var dragSale = GetComponent<TowerDragSale>();
        if (dragSale != null && dragSale.IsDrag)
        {
            return;
        }

        // TowerCombiner에 선택 알림
        TowerCombiner.Instance.SelectItem(this);
    }

    #region ISelectable

    public ElementType GetElementType()
    {
        return tower?.towerData?.ElementType ?? ElementType.None;
    }

    public int GetLevel()
    {
        return tower?.Rank ?? 1;
    }

    public Tile GetTile()
    {
        if (cachedTile != null)
            return cachedTile;

        // 캐시가 없으면 TowerDragSale에서 찾기
        var towerDragSale = GetComponent<TowerDragSale>();
        if (towerDragSale != null && towerDragSale.selectTile != null)
        {
            cachedTile = towerDragSale.selectTile;
            return cachedTile;
        }

        return null;
    }

    public string GetName()
    {
        return name;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // 타일 하이라이트 (ElementController와 동일한 방식)
        var parentTile = GetTile();
        if (parentTile != null)
        {
            parentTile.ApplyHighlight(selected);
        }
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void SetTile(Tile tile)
    {
        cachedTile = tile;
    }

    #endregion
}
