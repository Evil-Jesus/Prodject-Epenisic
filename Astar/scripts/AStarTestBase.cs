using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Zentronic;

public class AStarTestBase : MonoBehaviour
{
    protected Vec3di pathStart=new Vec3di();
    protected Vec3di pathEnd = new Vec3di();

    /// <summary>
    /// holds the current path between path Start and Path End
    /// </summary>
    public List<Vec3di> curPath = new List<Vec3di>();
     
    public int mapWidth;
    public int mapHeight;
    public int mapDepth;

    // tilemap used to render the maze
    public List<Tilemap> tilemaps;

    // tilemap used to render the path
    public List<Tilemap> tilemaps_path;
     
    /// <summary>
    /// a maze agent that is capable of moving arount in a maze
    /// </summary>
    public AStarAgent agent;

    /// <summary>
    /// holds the mace we walk inside
    /// </summary>
    protected Grid3di srcGrid;

    /// <summary>
    /// holds the information about the path and the helper tiles
    /// </summary>
    protected Grid3di resGrid;
 
    // toggle diagonal maze movement on/off
    public bool AllowDiagonals = false;

    // helper variable for edit/cursor mode
    protected int dragModeHelper = 0;


    protected  bool pathNeedsUpdate = true;

    protected Vector3 screenPos;

    protected virtual void OnStart()
    {
        srcGrid = new Grid3di();
        srcGrid.Resize(mapWidth, mapHeight, mapDepth);
        srcGrid.BorderCode=99;

        resGrid = new Grid3di(mapWidth,mapHeight,mapDepth);

        agent.map = srcGrid;
        agent.ownPosition = pathStart;

        pathNeedsUpdate = true;
    }

    public void ApplySolutionToMap(Grid3di map , int code )
    {
        foreach( Vec3di step in curPath )
        {
            map.SetAt(step.x,step.y,step.z,code);
        }
    }

    protected virtual void OnUpdate()
    {
        // get local position within AStarTest object
        screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

	// Use this for initialization
	void Start () 
    {
        OnStart();
	}
	
	// Update is called once per frame
	void Update () 
    {
        OnUpdate();
	}
}