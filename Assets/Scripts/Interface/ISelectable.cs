using UnityEngine;

public interface ISelectable
{
    ElementType GetElementType();
    int GetLevel();
    Tile GetTile();
    string GetName();
    void SetSelected(bool selected);
    GameObject GetGameObject();
}
