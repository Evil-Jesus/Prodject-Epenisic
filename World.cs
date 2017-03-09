using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zentronic;
using TileEditor;

public class World : MonoBehaviour
{

    public Grid3di navGrid = new Grid3di(32, 32, 1);
    public PathTile[,] tiles = new PathTile[32, 32];

    // Use this for initialization
    void Start()
    {
        genNav();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void genNav()
    {
        PathTile[] paths = GetComponentsInChildren<PathTile>();
        foreach (PathTile curPath in paths) {
            tiles[curPath.x, curPath.y] = curPath;
            if (curPath.code != 0) {
                navGrid.SetAt(curPath.x, curPath.y, curPath.code);

            }
            print(curPath.code);
            print(navGrid.GetAt(curPath.x, curPath.y, 0));
        }
    }
}
