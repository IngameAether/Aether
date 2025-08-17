using UnityEngine;

public interface ISelectable
{
    ElementType GetElementType();
    int GetLevel();
    Tile GetTile();
    void SetSelected(bool selected);
    GameObject GetGameObject();
}
