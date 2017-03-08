using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zentronic;
using UnityEngine.SceneManagement;

public class AStarTest : AStarTestBase 
{
    public Button btnExit;

    void InitializeMaze()
    {
        string s_maze_layer_0 = "################################"+
                                "# #  #          #       #      #"+
                                "# #  #          #       #      #"+
                                "# #  #  ###aaaa #   # # #  ### #"+
                                "# #  #    #a    #   # #a#  # # #"+
                                "# #  #    #a aaa#   #   #  # # a"+
                                "# #  #    #a a a#   #a# #  # # #"+
                                "# #  #    #a aaa#   # # #  # # #"+
                                "# #  #    #a    #   #   #  a # #"+
                                "# #  #    #a    #   #   #  a # #"+
                                "# #  ###  #######   # #a#  # # a"+
                                "# #    #      aa#   # # #  # a #"+
                                "# #    #  ###  ###  #      # # #"+
                                "# #         aa  #   ######## a #"+
                                "# #         aa  #            a #"+
                                "################################";

        string s_maze_layer_1 = "################################"+
                                "#    #########     ###     #####"+
                                "################################"+
                                "################################"+
                                "############### ################"+
                                "############### ################"+
                                "############### ################"+
                                "############### ################"+
                                "################################"+
                                "###############    ##     ######"+
                                "################################"+
                                "################################"+
                                "################################"+
                                "################################"+
                                "################################"+
                                "################################";

        srcGrid.ParseLayerFromString(s_maze_layer_0,tilemaps[0].ValueLookup,0);
        srcGrid.ParseLayerFromString(s_maze_layer_1,tilemaps[1].ValueLookup,1);
    }

    override protected void OnStart()
    {
        base.OnStart();

        pathStart = new Vec3di( 1,14,0);
        pathEnd   = new Vec3di(30, 1,0);

        InitializeMaze();

        btnExit.OnButtonUp += delegate(Button sender)
        {
            SceneManager.LoadScene(0);
        };
        
      //  agentRep.gameObject.SetActive(true);

 	}
    public GameObject agentRep;

    override protected void OnUpdate()
    {
        base.OnUpdate();
         
        if(pathNeedsUpdate)
        {
            agent.ownPosition = pathStart;
            agent.FindPath( pathEnd, curPath,AllowDiagonals);

            for (int i = 0; i < tilemaps.Count; i++)
            {
                // apply first floor
                tilemaps[i].ApplyMap(srcGrid, i);
            }

            if (dragModeHelper == 0)
            {
                agent.Follow(curPath);
            }

            pathNeedsUpdate=false;
        }

        // calculate tile position
        Vector3 localPos0 = tilemaps_path[0].transform.InverseTransformPoint(screenPos);
        Vec3di tilePos0 = tilemaps_path[0].LocalPosToTilePos(localPos0);
        tilePos0.z=0;

        Vector3 localPos1 = tilemaps_path[1].transform.InverseTransformPoint(screenPos);
        Vec3di tilePos1 = tilemaps_path[1].LocalPosToTilePos(localPos1);
        tilePos1.z=1;

        resGrid.Clear();
 
        ApplySolutionToMap(resGrid,9);
 
        resGrid.SetAt(pathStart.x, pathStart.y, 0, 10);
        resGrid.SetAt(pathEnd.x,   pathEnd.y,   0, 10);

        resGrid.SetAt(pathStart.x, pathStart.y, 1, 10);
        resGrid.SetAt(pathEnd.x,   pathEnd.y,   1,   10);
 
        resGrid.SetAt(tilePos0.x,  tilePos0.y, 0, 11);
        resGrid.SetAt(tilePos1.x,  tilePos1.y, 1, 11);

        tilemaps_path[0].ApplyMap(resGrid, 0);
        tilemaps_path[1].ApplyMap(resGrid, 1);

       // agentRep.transform.localPosition = tilemaps_path[0].TilePosToLocalPos(agent.ownPosition.x, agent.ownPosition.y, agentRep.transform.localPosition.z);
       // agentRep.transform.eulerAngles = new Vector3(0, 0, agent.curAimXYAnimated);
         
        HandleCursor(tilePos0, tilePos1);
	}
      
    void HandleCursor(Vec3di tilePos0, Vec3di tilePos1)
    {
        if( Input.GetMouseButtonUp(0) == true)
        {
            if(dragModeHelper==0)
            {
                if( pathStart.CompareXY(tilePos0))
                {
                    // enter drag mode - start tile
                    dragModeHelper=1;
                }
                else if(  pathEnd.CompareXY(tilePos0))
                {
                    // enter drag mode - end tile
                    dragModeHelper=2;
                }
                else if( pathStart.CompareXY(tilePos1)) 
                {
                    // enter drag mode - start tile
                    dragModeHelper=3;
                }
                else if(  pathEnd.CompareXY(tilePos1))
                {
                    // enter drag mode - end tile
                    dragModeHelper=4;
                }
            }
            else
            {
                dragModeHelper=0;
            }
        }
        else
        {
            if(dragModeHelper==1)
            {
                pathStart = tilePos0;
                pathNeedsUpdate=true;
            }
            else if(dragModeHelper==2)
            {
                pathEnd = tilePos0;
                pathNeedsUpdate=true;
            }
            else if(dragModeHelper==3)
            {
                pathStart = tilePos1;
                pathStart.z=1;

                pathNeedsUpdate=true;
            }
            else if(dragModeHelper==4)
            {
                pathEnd = tilePos1;
                pathEnd.z=1;

                pathNeedsUpdate=true;
            }
        }
    }
}