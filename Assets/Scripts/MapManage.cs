using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManage : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapRenderer mapRenderer;

    int[,] mapTiles;

    void Start()
    {
        mapTiles = mapGenerator.generateMap();
        mapRenderer.RenderMap(mapTiles);
    }

    public void ResetMap()
    {
        mapRenderer.ClearMap();
        mapTiles = mapGenerator.generateMap();
        mapRenderer.RenderMap(mapTiles);
    }
}
