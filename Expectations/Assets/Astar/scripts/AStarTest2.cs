

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Zentronic;
using UnityEngine.SceneManagement;

public class AStarTest2 : AStarTestBase 
{
    public Button btnExit;
    public Button btnRecreate;

    public Maze maze;

    public int weightLeft=100;
    public int weightRight=100;
    public int weightTop=100;
    public int weightBottom=100;

    public bool generateMaze = false;

    public enum Algo
    {
        DEEP_FIRST,
        BREATHE_FIRST
    }

    public Algo algorithm = Algo.DEEP_FIRST;

    void InitializeMaze()
    {
        maze = new Maze(mapWidth / 3, mapHeight / 3);

        if (algorithm == Algo.DEEP_FIRST)
        {
            maze.GenerateRandomMazeDeepFirst(weightLeft,weightRight,weightTop,weightBottom);
        }
        else if (algorithm == Algo.BREATHE_FIRST)
        {
            maze.GenerateRandomMazeBreatheFirst(weightLeft,weightRight,weightTop,weightBottom);
        }

        maze.ApplyToMap(srcGrid,1);

        pathStart = new Vec3di(maze.begin.Position.x*3,maze.begin.Position.y*3,0);
        pathEnd = new Vec3di(maze.end.Position.x*3,maze.end.Position.y*3,0);

        agent.ownPosition = pathStart;
    }

    public GameObject agentRep;

    override protected void OnStart()
    {
        base.OnStart();

        pathStart = new Vec3di( 1,14,0);
        pathEnd   = new Vec3di(62, 1,0);

        agentRep.gameObject.SetActive(true);

        InitializeMaze();

        btnExit.OnButtonUp += delegate(Button sender)
        {
             SceneManager.LoadScene(0);
        };
    
        btnRecreate.OnButtonUp += delegate(Button sender)
        {
            generateMaze=true;
        };
    }
   
    override protected void OnUpdate()
    {
        if (generateMaze == true)
        {
            InitializeMaze();
            generateMaze = false;
            pathNeedsUpdate = true;
        }

        base.OnUpdate();
          
        // calculate tile position
        Vec3di tilePosMouse = tilemaps_path[0].LocalPosToTilePos(tilemaps_path[0].transform.InverseTransformPoint(screenPos));

        if(pathNeedsUpdate)
        {
            agent.FindPath( pathEnd, curPath,AllowDiagonals);

            tilemaps[0].ApplyMap(srcGrid,0);
             
            if (dragModeHelper == 0)
            {
                agent.Follow(curPath);
            }

            pathNeedsUpdate=false;
        }
 
        resGrid.Clear();
        ApplySolutionToMap(resGrid,9);
 
        resGrid.SetAt(pathStart.x, pathStart.y, pathStart.z, 10);
        resGrid.SetAt(pathEnd.x,   pathEnd.y,   pathEnd.z,   10);

        resGrid.SetAt(tilePosMouse.x,  tilePosMouse.y, 0, 11);
         
        tilemaps_path[0].ApplyMap(resGrid, 0);
       
        agentRep.transform.localPosition = tilemaps_path[0].TilePosToLocalPos(agent.ownPosition.x, agent.ownPosition.y, agentRep.transform.localPosition.z);
        agentRep.transform.eulerAngles = new Vector3(0, 0, agent.curAimXYAnimated);
      
        if (lastTilePos.Compare(tilePosMouse) == false)
        {
            if (srcGrid.GetAt(tilePosMouse) != srcGrid.BorderCode)
            {
                agent.Stop();

                pathEnd = tilePosMouse;
                pathStart = agent.ownPosition;

                dragModeHelper=0;
                pathNeedsUpdate=true;

                lastTilePos = tilePosMouse;
            }
        }
    }

    Vec3di lastTilePos = new Vec3di();

     
}