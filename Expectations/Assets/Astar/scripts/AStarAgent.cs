using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Zentronic
{
    /// <summary>
    /// agent class. Can be used to find a path between start end end point in a given map
    /// </summary>
    public class AStarAgent : MonoBehaviour
    {
        public Vec3df ownPosition;

        public Grid3di map;
         
        /// <summary>
        /// Finds a path from own position to given position in maze.
        /// </summary>
        /// <returns>The path.</returns>
        /// <param name="map">Map.</param>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <param name="solution">Solution.</param>
        /// <param name="allowDiagonals">If set to <c>true</c> allow diagonals.</param>
        public AStarSearch.SearchState FindPath( Vec3di end, List<Vec3di> solution, bool allowDiagonals)
        {
            solution.Clear();

            AStarSearch astarsearch = new AStarSearch( map, ownPosition, end,allowDiagonals);

            AStarSearch.SearchState SearchState;

            do
            {
                SearchState = astarsearch.SearchStep();
            }
            while( SearchState == AStarSearch.SearchState.SEARCHING );

            if( SearchState == AStarSearch.SearchState.SUCCEEDED )
            {
                GridPosition mapPos = astarsearch.GetSolutionStart();
                  
                while(mapPos!=null)
                {
                    solution.Add( mapPos.Position );

                    mapPos = astarsearch.GetSolutionNext();
                };

                // Once you're done with the solution you can free the nodes up
                astarsearch.Cleanup();
            }
            else if( SearchState == AStarSearch.SearchState.FAILED )
            {
                //Zentronic.Debug.LogError("search terminated. Goal state not found!");
            }

            return SearchState;
        }

        /// <summary>
        /// Follow the specified solution and speed.
        /// speed is given in tiles per second
        /// a speed of 1 means that one tile is travelled per second!!
        /// </summary>
        /// <param name="solution">Solution.</param>
        /// <param name="speed">Speed.</param>
        public void Follow( List<Vec3di> solution )
        {
            if (curWalkJob != null)
            {
                Stop();
            }

            if (curWalkJob == null && solution.Count > 1)
            {
                List<Vec3df> path = new List<Vec3df>();
                foreach (Vec3di v in solution)
                {
                    path.Add(new Vec3df(v.x, v.y,v.z));
                }

                float speedPerTile = AgentMoveSpeed/(float)solution.Count;
                curWalkJob = StartCoroutine(coFollowPath(path,speedPerTile));
            }
        }

        public void Stop()
        {
            if (curWalkJob != null)
            {
                StopCoroutine(curWalkJob);
                curWalkJob = null;
            }
        }

        Coroutine curWalkJob = null;

        // current position in path walk
        Vec3df curWalkPos = null;

        // next position in path walk
        Vec3df nextWalkPos = null;

        // pcntg of interpolation between curWalkPos and nextWalkPos
        float walkDelta = 0;

        // the aim while walking in degree.
        // Can be used as sprite rotation
        public float curAimXY = 0;
     
        // animated aim. Turns to curAimXY with aimRotateSpeed
        public float curAimXYAnimated = 0;

        // default rotate 180 degree per second
        public float aimXYAnimationSpeed = 180.0f;
         
        public float AgentMoveSpeed = 1;
           
        IEnumerator coFollowPath(List<Vec3df> path, float speedPerTile )
        {
            Vector3 curAim = Vector3.zero;

            // unit vector that points in x direction
            Vector3 unit_vector_x = new Vector3(1, 0, 0);
             
            // solution.Count=4
            // 3 steps required
            //          from to
            // step 1:   0 .. 1
            // step 2:   1 .. 2
            // step 3:   2 .. 3

            // PcntPerStep=0.33f
            // e.g 4 points     pcntg           idx0  idx1
            // point 0:         0.00 -> 0.33    0     1  
            // point 1:         0.33 -> 0.66    1     2
            // point 2:         0.66 -> 1.00    2     3 
  
            if (path.Count > 1)
            {
                float pcntg = 0;

                // pcntg that is necessary per walk step from point to pint
                float PcntPerStep = 1.0f / (float)(path.Count-1);

                // check if previous job was interrupted and resume it to avoid jerky movement
                if (curWalkPos != null && nextWalkPos != null && path.Count > 2 )
                {
                    // if job was interrupted we can match the last 2 positions in the new solution
                    if (path[0].Compare(curWalkPos) && 
                        path[1].Compare(nextWalkPos) )
                    {
                        // if the first 2 positions in new path match this means we have been travelling the same direction before and we can skip the delta amount
                        pcntg = walkDelta * PcntPerStep;
                    }
                }

                Vec3df lastPosition = new Vec3df(ownPosition);
                
                while (pcntg < 1.0f)
                {
                    pcntg =  Mathf.Clamp01(pcntg+Time.deltaTime * speedPerTile);

                    // 0.24 .. 0 , 0.25 .. 1, 0.5 .. 2, 0.75 .. 3 0.99 .. 3
                    float idx0f = (float)(path.Count-1) * pcntg;

                    // truncate the float
                    int idx0 = (int)(idx0f);

                    int idx1 = idx0+1;

                    // will be 0.33 with above example
                    // lower percentage border
                    float pidx0 = ((float)idx0) * PcntPerStep;

                    // delta will now go from 0 .. 1
                    walkDelta  = (pcntg - pidx0) / PcntPerStep;

                    if (idx1 < path.Count )
                    {
                        curWalkPos = path[idx0];
                        nextWalkPos = path[idx1];
                      
                        ownPosition.x = path[idx0].x * (1.0f - walkDelta) + path[idx1].x * (walkDelta);
                        ownPosition.y = path[idx0].y * (1.0f - walkDelta) + path[idx1].y * (walkDelta);
                        ownPosition.z = path[idx0].z * (1.0f - walkDelta) + path[idx1].z * (walkDelta);

                        curAim.x = ownPosition.x - lastPosition.x;
                        curAim.y = ownPosition.y - lastPosition.y;
                        curAim.z = 0;

                        if (curAim.magnitude > 0)
                        {
                            curAimXY = Angle(curAim, unit_vector_x);
  
                            // correct the rotation directions by using negative angles that are closer to the destination
                            if (curAimXY - curAimXYAnimated > 180.0f)
                            {
                                // happens if aimXYanimated is negativ!! e.g:  curAimXY: 180 aimXYanimated:  -90
                                curAimXY -= 360.0f;
                            }
                             
                            if (curAimXYAnimated - curAimXY > 180.0f)
                            {
                                // happens if   curAimXY is negative!! e.g   curAimXY: -90 aimXYanimated: -180
                                curAimXYAnimated -= 360.0f;
                            }
  
                            curAimXY         = Wrap360(curAimXY);
                            curAimXYAnimated = Wrap360(curAimXYAnimated);
                        }

                        if (curAimXYAnimated < curAimXY)
                        {
                            curAimXYAnimated += aimXYAnimationSpeed * Time.deltaTime;

                            if (curAimXYAnimated > curAimXY)
                                curAimXYAnimated = curAimXY;
                        }
                        else if (curAimXYAnimated > curAimXY)
                        {
                            curAimXYAnimated -= aimXYAnimationSpeed * Time.deltaTime;

                            if (curAimXYAnimated < curAimXY)
                                curAimXYAnimated = curAimXY;
                        }

                        lastPosition.x = ownPosition.x;
                        lastPosition.y = ownPosition.y;
                        lastPosition.z = ownPosition.z;
                    }

                    // wait for next frame!
                    yield return null;
                }

                ownPosition = path[path.Count - 1];
                yield return null;

                // path is complete, now finish rotation if required
                if (curAimXYAnimated < curAimXY)
                {
                    while (curAimXYAnimated < curAimXY)
                    {
                        curAimXYAnimated += aimXYAnimationSpeed * Time.deltaTime;

                        if (curAimXYAnimated > curAimXY)
                            curAimXYAnimated = curAimXY;

                        yield return null;
                    }
                }
                else if (curAimXYAnimated > curAimXY)
                {
                    while (curAimXYAnimated > curAimXY)
                    {
                        curAimXYAnimated -= aimXYAnimationSpeed * Time.deltaTime;

                        if (curAimXYAnimated < curAimXY)
                            curAimXYAnimated = curAimXY;

                        yield return null;
                    }
                }
  
                curWalkPos=null;
                nextWalkPos = null;
                curWalkJob = null;
            }
        }
 
        float Angle(Vector3 v1, Vector3 v2)
        {
            Vector3 cross = Vector3.Cross(v1, v2);

            float angle = Vector3.Angle(v2, v1);

            if (cross.z < 0)
            {
                angle = angle*-1.0f; // 90 becomes 270
            }
            return angle;
        }

        float Wrap360(float angle )
        {
            if (angle < -360)
            {
                angle += 360;
            }

            if (angle > 360)
            {
                angle -= 360;
            }

            return angle;
        }

        /// <summary>
        /// helper class that holds one position within the given map
        /// </summary>
        public class GridPosition
        {
            // grid in use
            Grid3di grid;

            // position in map
            Vec3di pos;

            public Vec3di Position
            {
                get
                {
                    return pos;
                }
            }

            public GridPosition(Grid3di Map)
            {
                grid = Map;
                pos = new Vec3di();
            }

            public GridPosition(Grid3di Map, int px, int py, int pz )
            {
                grid=Map;
                pos = new Vec3di(px,py,pz);
            }

            public GridPosition(Grid3di Map, Vec3di p )
            {
                grid=Map;
                pos = p;
            }

            /// <summary>
            /// compare if 2 positions are same
            /// </summary>
            /// <param name="nodeGoal">Node goal.</param>
            public bool Compare( GridPosition nodeGoal )
            {
                // same state in a maze search is simply when (x,y) are the same
                if( (pos.x == nodeGoal.pos.x) && (pos.y == nodeGoal.pos.y)&& (pos.z == nodeGoal.pos.z) )
                {
                    return true;
                }

                return false;
            }

            public float EstimatedDistanceToNode( GridPosition nodeGoal )
            {
                // ManhattanDistance is hardcoded could be paramatrized in Astar
                return ManhattanDistance(nodeGoal);
            }

            public float ChebyshevDistance( GridPosition nodeGoal )
            {
                float xd = Mathf.Abs( (float)pos.x - (float)nodeGoal.pos.x);
                float yd = Mathf.Abs( (float)pos.y - (float)nodeGoal.pos.y);
                return Mathf.Max(xd,yd);
            }

            public float EuclidianDistance(  GridPosition nodeGoal )
            {
                float xd = (float)pos.x - (float)nodeGoal.pos.x;
                float yd = (float)pos.y - (float)nodeGoal.pos.y;
                return Mathf.Sqrt( xd*xd + yd*yd);
            }

            public float ManhattanDistance( GridPosition nodeGoal )
            {
                float xd = Mathf.Abs( (float)pos.x - (float)nodeGoal.pos.x);
                float yd = Mathf.Abs( (float)pos.y - (float)nodeGoal.pos.y);
                float zd = Mathf.Abs( (float)pos.z - (float)nodeGoal.pos.z);

                return xd + yd + zd;
            }

            public void PrintNodeInfo()
            {
                Debug.Log( "Node position : ("+pos.x+","+pos.y+","+pos.z+")");
            }

            // This generates the successors to the given Node. It uses a helper function called
            // AddSuccessor to give the successors to the AStar class. The A* specific initialisation
            // is done for each node internally, so here you just set the state information that
            // is specific to the application
            public void FindSuccessors( AStarSearch curSearch, GridPosition parent_node, bool allowDiagonal )
            {
                Vec3di pos_parent = new Vec3di(-1,-1,-1);

                if( parent_node != null )
                {
                    pos_parent = parent_node.pos;
                }
              
                Vec3di PosUpLeft = new Vec3di(pos.x-1, pos.y+1 ,pos.z);
                Vec3di PosUpRight = new Vec3di(pos.x+1,pos.y+1 ,pos.z);
                Vec3di PosDownLeft = new Vec3di(pos.x-1, pos.y-1 ,pos.z);
                Vec3di PosDownRight = new Vec3di(pos.x+1,pos.y-1 ,pos.z);

                Vec3di PosUp     = new Vec3di(pos.x  ,pos.y+1 ,pos.z);
                Vec3di PosLeft   = new Vec3di(pos.x-1,pos.y   ,pos.z);

                Vec3di PosRight = new Vec3di(pos.x+1,pos.y   ,pos.z);
                Vec3di PosDown  = new Vec3di(pos.x  ,pos.y-1 ,pos.z);

                Vec3di PosAbove = new Vec3di(pos.x  ,pos.y ,pos.z+1);
                Vec3di PosBelow = new Vec3di(pos.x  ,pos.y ,pos.z-1);

                if (allowDiagonal)
                {
                    if ((grid.GetAt(PosUpLeft) != grid.BorderCode) && PosUpLeft.Compare(pos_parent) == false)
                    {
                        // we have pos UpLeft, now check if at lest Up or left are maze
                        if (grid.GetAt(PosLeft) != grid.BorderCode || grid.GetAt(PosUp) != grid.BorderCode)
                        {
                            curSearch.AddSuccessor(new GridPosition(grid, PosUpLeft));
                        }
                    }

                    if ((grid.GetAt(PosUpRight) != grid.BorderCode) && PosUpRight.Compare(pos_parent) == false)
                    {
                        // we have pos UpLeft, now check if at lest Up or left are maze
                        if (grid.GetAt(PosRight) != grid.BorderCode || grid.GetAt(PosUp) != grid.BorderCode)
                        {
                            curSearch.AddSuccessor(new GridPosition(grid, PosUpRight));
                        }
                    }

                    if ((grid.GetAt(PosDownLeft) != grid.BorderCode) && PosDownLeft.Compare(pos_parent) == false)
                    {
                        // we have pos UpLeft, now check if at lest Up or left are maze
                        if (grid.GetAt(PosLeft) != grid.BorderCode || grid.GetAt(PosDown) != grid.BorderCode)
                        {
                            curSearch.AddSuccessor(new GridPosition(grid, PosDownLeft));
                        }
                    }

                    if ((grid.GetAt(PosDownRight) != grid.BorderCode) && PosDownRight.Compare(pos_parent) == false)
                    {
                        // we have pos UpLeft, now check if at lest Up or left are maze
                        if (grid.GetAt(PosRight) != grid.BorderCode || grid.GetAt(PosDown) != grid.BorderCode)
                        {
                            curSearch.AddSuccessor(new GridPosition(grid, PosDownRight));
                        }
                    }
                }
 
                // push each possible move except allowing the search to go backwards
                if( (grid.GetAt( PosLeft  ) !=  grid.BorderCode) && PosLeft.Compare(pos_parent) == false  )
                {
                    curSearch.AddSuccessor( new GridPosition(grid, PosLeft) );
                }

                if( (grid.GetAt( PosDown ) !=  grid.BorderCode) && PosDown.Compare(pos_parent) == false  )
                {
                    curSearch.AddSuccessor( new GridPosition(grid, PosDown ) );
                }

                if( (grid.GetAt( PosRight  ) !=  grid.BorderCode) && PosRight.Compare(pos_parent) == false )
                {
                    curSearch.AddSuccessor( new GridPosition( grid,PosRight ) );
                }

                if( (grid.GetAt( PosUp  ) !=  grid.BorderCode)  && PosUp.Compare(pos_parent) == false )
                {
                    curSearch.AddSuccessor( new GridPosition(grid, PosUp ) );
                }
               
                if( (grid.GetAt( PosAbove  ) !=  grid.BorderCode)  && PosAbove.Compare(pos_parent) == false )
                {
                    curSearch.AddSuccessor( new GridPosition(grid, PosAbove ) );
                }

                if( (grid.GetAt( PosBelow  ) !=  grid.BorderCode)  && PosBelow.Compare(pos_parent) == false )
                {
                    curSearch.AddSuccessor( new GridPosition(grid, PosBelow ) );
                }
            }

            //  get the cost of the map at position
            public float GetCost()
            {
                return (float) grid.GetAt( pos.x, pos.y, pos.z, grid.BorderCode );
            }
        };

        /// <summary>
        /// A star search.
        /// </summary>
        public class AStarSearch
        {
            public enum SearchState
            {
                NOT_INITIALISED,
                SEARCHING,
                SUCCEEDED,
                FAILED,
            };
 
            public AStarSearch(Grid3di map, Vec3di  start,  Vec3di goal, bool AllowDiagonal) 
            {
                searchState=SearchState.NOT_INITIALISED;
                curSolutionNode=null;
                cancelRequested=false;
                SetStartAndGoalStates(map,start,goal);
                allowDiagonal = AllowDiagonal;
            }

            // A State represents a possible state in the search
            // The user provided state type is included inside this type
            public class SearchNode  
            {
                // node is always connected to prev node unless it is the start node
                public SearchNode prevNode; 

                // node is always connected to next node unless it is the next node
                public SearchNode nextNode; 

                // position in grid
                public GridPosition position;

                // f(x)=g(x)+h(x)
                // cost of this node + it's predecessors
                public float g; 

                // heuristic estimate of distance to goal
                public float h; 

                // sum of cumulative cost of predecessors and self and heuristic
                public float f; 

                public SearchNode() 
                {      
                    prevNode=null;
                    nextNode=null;
                    g=0;
                    f=0;
                    h=0;
                }

                public SearchNode(GridPosition State) 
                {      
                    prevNode=null;
                    nextNode=null;
                    g=0;
                    f=0;
                    h=0;
                    position=State;
                }
            };
 
            // allow diagonal moves on grid
            bool allowDiagonal=false;
 
            // call at any time to cancel the search and free up all the memory
            void CancelSearch()
            {
                cancelRequested = true;
            }

            // Set Start and goal states
            void SetStartAndGoalStates( Grid3di map, Vec3di  start,  Vec3di goal )
            {
                cancelRequested = false;

                startNode = new SearchNode(new GridPosition(map,start.x, start.y, start.z));
                endNode = new SearchNode(new GridPosition(map, goal.x, goal.y, goal.z));

                searchState = SearchState.SEARCHING;

                // Initialise the AStar specific parts of the Start Node
                // The user only needs fill out the state information
                startNode.g = 0; 
                startNode.h = startNode.position.EstimatedDistanceToNode( endNode.position );
                startNode.f = startNode.g + startNode.h;
                startNode.prevNode = null;

                openNodes.Clear();
                // Push the start node on the Open list
                openNodes.Add( startNode ); // heap now unsorted

                // Initialise counter for search steps
                searchSteps = 0;
            }

            // Advances search one step 
            public SearchState SearchStep()
            {
                // Next I want it to be safe to do a searchstep once the search has succeeded...
                if( (searchState == SearchState.SUCCEEDED) || (searchState == SearchState.FAILED) )
                {
                    return searchState; 
                }

                // Failure is defined as emptying the open list as there is nothing left to 
                // search...
                // New: Allow user abort
                if( openNodes.Count==0 || cancelRequested )
                {
                    FreeUnusedNodes();
                    searchState = SearchState.FAILED;
                    return searchState;
                }

                // Incremement step count
                searchSteps ++;

                // removing the front will not change the sort order
                SearchNode node = openNodes[0];
                openNodes.RemoveAt(0);

                // Check for the goal, once we pop that we're done
                if( node.position.Compare( endNode.position ) )
                {
                    // The user is going to use the Goal Node he passed in 
                    // so copy the parent pointer of n 
                    endNode.prevNode = node.prevNode;

                    // A special case is that the goal was passed in as the start state
                    // so handle that here
                    if(  node.position.Compare( startNode.position ) == false )
                    {
                        // set the child pointers in each node (except Goal which has no child)
                        SearchNode nextNode = endNode;
                        SearchNode prevNode = endNode.prevNode;

                        do 
                        {
                            prevNode.nextNode = nextNode;
                            nextNode = prevNode;
                            prevNode = prevNode.prevNode;
                        } 
                        while( nextNode != startNode ); // Start is always the first node by definition
                    }

                    // delete nodes that aren't needed for the solution
                    FreeUnusedNodes();

                    searchState = SearchState.SUCCEEDED;

                    return searchState;
                }
                else // not goal
                {
                    successors.Clear(); // empty vector of successor nodes to n

                    node.position.FindSuccessors( this, node.prevNode !=null ? node.prevNode.position : null , allowDiagonal); 

                    // Now handle each successor to the current node ...
                    foreach( SearchNode successor in successors )
                    {
                        //  The g value for this successor ...
                        float new_g = node.g + node.position.GetCost();

                        // Now we need to find whether the node is on the open or closed lists
                        // If it is but the node that is already on them is better (lower g)
                        // then we can forget about this successor

                        // First linear search of open list to find node
                        int openlist_result=0;

                        foreach( SearchNode open in openNodes  )
                        {
                            if( open.position.Compare( successor.position ) )
                            {
                                break;                  
                            }

                            openlist_result++;
                        }

                        if( openlist_result != openNodes.Count )
                        {
                            // we found this state on open
                            if( openNodes[openlist_result].g <= new_g )
                            {
                                // the one on Open is cheaper than this one
                                continue;
                            }
                        }

                        int closedlist_result=0;
                        foreach( SearchNode closed in closedNodes  )
                        {
                            if( closed.position.Compare( successor.position ) )
                            {
                                break;                  
                            }

                            closedlist_result++;
                        }

                        if( closedlist_result != closedNodes.Count )
                        {
                            // we found this state on closed
                            if( closedNodes[closedlist_result].g <= new_g )
                            {
                                // the one on Closed is cheaper than this one
                                continue;
                            }
                        }

                        // This node is the best node so far 
                        // so lets keep it and set up its AStar specific data ...
                        successor.prevNode = node;
                        successor.g = new_g;
                        successor.h = successor.position.EstimatedDistanceToNode( endNode.position );
                        successor.f = successor.g + successor.h;

                        // Remove successor from closed if it was on it
                        if( closedlist_result != closedNodes.Count )
                        {
                            // remove it from Closed
                            closedNodes.RemoveAt( closedlist_result );
                        }

                        // Update old version of this node
                        if( openlist_result != openNodes.Count )
                        {      
                            openNodes.RemoveAt( openlist_result );
                        }

                        InsertSorted(openNodes,successor);
                    }

                    // push n onto Closed, as we have expanded it now
                    closedNodes.Add(node);

                } // end else (not goal so expand)

                return searchState; // Succeeded bool is false at this point. 
            }

            /// <summary>
            /// Inserts a node into the list, sorted.
            /// </summary>
            /// <param name="list">List.</param>
            /// <param name="node">Node.</param>
            void InsertSorted( List<SearchNode> list, SearchNode node  )
            {
                for (int i=0;i<list.Count;i++)
                {
                    if( list[i].f > node.f )
                    {
                        list.Insert(i,node);
                        return;
                    }
                }

                list.Add(node);
            }

            // User calls this to add a successor to a list of successors
            // when expanding the search frontier
            public void AddSuccessor( GridPosition State )
            {
                successors.Add( new SearchNode(State) );
            }

            // Free the solution nodes
            // remove all the links to make it easier for the GC to cleanup
            public void Cleanup()
            {
                SearchNode n = startNode;

                if( startNode.nextNode != null )
                {
                    do
                    {
                        SearchNode del = n;
                        n = n.nextNode;
                        del = null;
                    } 
                    while( n != endNode );
                }
            }

            // Get start node of solution
            public GridPosition GetSolutionStart()
            {
                curSolutionNode = startNode;

                if( startNode != null)
                {
                    return startNode.position;
                }
                else
                {
                    return null;
                }
            }

            // Get next node
            public GridPosition GetSolutionNext()
            {
                if( curSolutionNode != null )
                {
                    if( curSolutionNode.nextNode != null)
                    {
                        SearchNode child = curSolutionNode.nextNode;

                        curSolutionNode = curSolutionNode.nextNode;

                        return child.position;
                    }
                }

                return null;
            }

            // Get end node
            GridPosition GetSolutionEnd()
            {
                curSolutionNode = endNode;

                if( endNode != null)
                {
                    return endNode.position;
                }
                else
                {
                    return null;
                }
            }

            // Step solution iterator backwards
            GridPosition GetSolutionPrev()
            {
                if( curSolutionNode != null)
                {
                    if( curSolutionNode.prevNode != null)
                    {
                        SearchNode parent = curSolutionNode.prevNode;

                        curSolutionNode = curSolutionNode.prevNode;

                        return parent.position;
                    }
                }

                return null;
            }

            // Get the number of steps
            int GetStepCount() 
            { 
                return searchSteps; 
            }

            // This call is made by the search class when the search ends. A lot of nodes may be
            // created that are still present when the search ends. They will be deleted by this 
            // routine once the search ends
            void FreeUnusedNodes()
            {
                openNodes.Clear();
                closedNodes.Clear();
            }

            // list with open nodes
            List< SearchNode> openNodes=new List<SearchNode>();

            // list with closed nodes
            List< SearchNode> closedNodes=new List<SearchNode>(); 

            // list with successor nodes
            List< SearchNode> successors=new List<SearchNode>();

            // State
            SearchState searchState;

            // Counts steps
            int searchSteps;

            // Start and goal state pointers
            SearchNode startNode;
            SearchNode endNode;

            // pointer to current solution. Is used as iterator 
            // when solution is queried out
            SearchNode curSolutionNode;

            bool cancelRequested;
        };

        static public void TestFunction()
        {
            String Map  ="##############################"+
                        "#               #        #   #"+
                        "#    #  ###     #   #    #   #"+
                        "#    #    #aaaaa#   #    #   #"+
                        "#    #    #aaaaa#   #    #   #"+
                        "#    #    #aaaaaa   #    #   #"+
                        "#    ###  #######   #    #   #"+
                        "#      #            #        #"+
                        "#      #            #        #"+
                        "##############################";

            Grid3di map = new Grid3di();
            map.Resize(30,10);
            map.BorderCode=99;

            Dictionary<char,int> valueLookup = new Dictionary<char, int>();
            valueLookup['#']=map.BorderCode;
            valueLookup[' ']=0;
            valueLookup['a']=1;

            map.ParseLayerFromString(Map,valueLookup,0);

            Dictionary<int,char> codeLookup = new Dictionary<int, char>();
            codeLookup[0]='=';
            codeLookup[map.BorderCode]='@';
            codeLookup[1]='a';
            codeLookup[9]='*';  // symbol for path

            map.ShowDebugInfo(codeLookup);

            List<Vec3di> solution = new List<Vec3di>();

            AStarAgent pf = new AStarAgent();

            pf.ownPosition = new Vec3di(2, 9, 0);
            pf.map = map;

            if( pf.FindPath( new Vec3di(28,2,0),solution, false ) == AStarSearch.SearchState.SUCCEEDED)
            {
                Grid3di copy = new Grid3di(map);

                int i=1;
                foreach( Vec3di step in solution )
                {
                    //Zentronic.Debug.Log("step " + i + ":" +  step.x + "/" + step.y);
                    i++;
                    copy.SetAt(step.x,step.y,step.z,9);
                }

                copy.ShowDebugInfo(codeLookup);
            }
            else
            {
                Debug.LogError("unable to find path!");
            }   
        }
    }
}