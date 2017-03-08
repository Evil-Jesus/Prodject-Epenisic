using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zentronic;

public class Entity : AStarAgent
{

    public List<Vec3di> curPath = new List<Vec3di>();
    public Vec3di pathEnd = new Vec3di();
    public bool allowDiagonals = true;
    public string entityType = "noType";

    // Use this for initialization
    void Start()
    {
        map = GameObject.Find("World").GetComponent<World>().navGrid;
        ownPosition = new Vec3di(5, 5, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(ownPosition.x, ownPosition.y, 0);
        if (Input.GetKeyDown(KeyCode.Space)) {
            print(map.GetAt(4, 5, 99));
            FindPath(pathEnd, curPath, true);
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            Follow(curPath);
        }
    }
}
