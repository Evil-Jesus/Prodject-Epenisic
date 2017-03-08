

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Zentronic;
using UnityEngine.SceneManagement;

public class AStarTest1 : AStarTestBase 
{
    public Button btnExit;
     

    void InitializeMaze()
    {
        
        string sMaze =  "################################################################"+
                        "#    #          #       #      ##    #  #       #       #      #"+
                        "# #  #  ###aaaa #   # # #  ### ##    # #        #       # ######"+
                        "# #  #    #a    #   # #a#  # # ##    ##     #  ##   #   #      #"+
                        "# #  #    #a aaa#   #   #  # # aa    a     #  #     #   ###### #"+
                        "# #  #    #a a a#   #a# #  # # ##    #    #  #  #   #   #      #"+
                        "# #  #    #a aaa#   # # #  # # ##    #   #  #  ##   #   # ######"+
                        "# #  #    #a        #   #  a # ##    ####  #  # #   #   #      #"+
                        "# #  ###  #######   # #a#  # # a     #    #  #  #   #   ###### #"+
                        "# #    #      aaa   # # #  # a ##    #   #  #   #   #   #      #"+
                        "# #    #  ###  ###  #      # # ##    #  #  #   ##   #   # ######"+
                        "# #         aa      ######## a ##    # #  #   # #   #   #      #"+
                        "# ######    #####   #        #       #   #   #  #   #     ######"+
                        "# #    #    #   #   #  ###a### ##    # ##   #####   #####      #"+
                        "#      #    #       #          ##    #                  #      #"+
                        "#    #          #       #      ##    #          #       #      #"+
                        "# #  #  ###aaaa #   # # #  ### ##    #          #       # ######"+
                        "# #  #    #a    #   # #a#  # # ##    aa        ##   #   #      #"+
                        "# #  #    #a aaa#   #   #  # # aa    ####     #     #   ###### #"+
                        "# #  #    #a a a#   #a# #  # # ##       #    #  #   #   #      #"+
                        "# #  #    #a aaa#   # # #  # # ##    #  #   #  ##   #   # ######"+
                        "# #  #    #a        #   #  a # ##    #  #a##  # #   #   #      #"+
                        "# #  ###  #######   # #a#  # # a     #       #  #   #   ###### #"+
                        "################################################################";
                          
        srcGrid.ParseLayerFromString(sMaze,tilemaps[0].ValueLookup,0);
    
    //    pathStart = srcGrid.GenerateRandomMaze(mapWidth / 3, mapHeight / 3, 1);
    }

    public GameObject agentRep;

    override protected void OnStart()
    {
        base.OnStart();

        pathStart = new Vec3di( 1,14,0);
        pathEnd   = new Vec3di(62, 1,0);

        agent.ownPosition = pathStart;
        agentRep.gameObject.SetActive(true);

        InitializeMaze();

        btnExit.OnButtonUp += delegate(Button sender)
        {
            SceneManager.LoadScene(0);
        };
    }
   
    override protected void OnUpdate()
    {
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