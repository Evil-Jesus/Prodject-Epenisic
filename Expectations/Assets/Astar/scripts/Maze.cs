using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Zentronic
{
    /// <summary>
    /// helper class for maze createion. Is used by maze class
    /// to generate random mazes
    /// Holds four booleans to indicate wheter left,right,up and down walls
    /// are intact or broken
    /// </summary>
    public class Maze
    {
        
        public class Cell
        {
            public Cell(Vec3di position)
            {
                this.position = position;

                this.left = true;
                this.right = true;
                this.upper = true;
                this.lower = true;
            }
              
            public bool left;
            public bool right;
            public bool upper;
            public bool lower;

            public bool IsIntact()
            {
                return 
                    right == true && 
                    left == true  && 
                    upper == true  && 
                    lower == true;
            }

            Vec3di position;

            public Vec3di Position
            {
                get { return this.position; }
            }
        }

      

        public Maze(int W,int H )
        {
            w = W;
            h = H;

            arr = new Cell[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    arr[x + w * y] = new Cell(new Vec3di(x, y, 0));
                }
            }
        }

        public void ApplyToMap( Grid3di map , int emptyCode)
        {
            map.Resize(w * 3, h * 3,1);

            for (int Y = 0; Y < h; Y++)
            {
                for (int X = 0; X < w; X++)
                {
                    Maze.Cell cell = GetAt(X,Y);

                    int x0 = X * 3;
                    int y0 = Y * 3;

                    // fill block
                    for (int y = y0; y < y0 + 3; y++)
                    {
                        for (int x = x0; x < x0 + 3; x++)
                        {
                            map.SetAt(x,y,map.BorderCode);
                        }
                    }

                    // empty field in the center
                    map.SetAt(x0 + 1, y0 + 1, emptyCode);

                    if (cell.right == false)
                    {
                        map.SetAt(x0 + 2, y0 + 1, emptyCode);
                    }

                    if (cell.left==false)
                    {
                        map.SetAt(x0 + 0, y0 + 1, emptyCode);
                    }

                    if (cell.upper == false)
                    {
                        map.SetAt(x0 + 1, y0 + 0, emptyCode);
                    }

                    if (cell.lower == false)
                    {
                        map.SetAt(x0 + 1, y0 + 2, emptyCode);
                    }

                }
            }
        }

        /// <summary>
        /// generates a maze using deep first search like described on
        /// https://en.wikipedia.org/wiki/Maze_generation_algorithm
        /// </summary>
        /// <param name="w1">W1.</param>
        /// <param name="w2">W2.</param>
        /// <param name="w3">W3.</param>
        /// <param name="w4">W4.</param>
        public void GenerateRandomMazeDeepFirst(int w1=100, int w2=100, int w3=100, int w4=100)
        {
            float total_weight = w1+w2+w3+w4;
            float p1 = (float)w1 / total_weight;
            float p2 = p1+(float)w2 / total_weight;
            float p3 = p2+(float)w3 / total_weight;
            float p4 = p3+(float)w4 / total_weight;

            Stack<Cell> stack = new Stack<Cell>();
 
            // start with random cell
            Cell curLocation = GetAt(GetRandomPosition());

            // push random location
            stack.Push(curLocation);
            
            while (curLocation != null)
            {
                // find intact neighbours
                Cell randNeighbour  = FindRandomIntactNeighbour(curLocation.Position,p1,p2,p3,p4);

                if (randNeighbour!= null)
                {
                    stack.Push( curLocation);

                    breakWall(curLocation,randNeighbour);
                    
                    curLocation = randNeighbour;
                }
                else
                {
                    curLocation = stack.Count > 0 ? stack.Pop() : null;
                }
            }
 
            makeMazeBeginEnd();
        }

        public Vec3di GetRandomPosition()
        {
            return new Vec3di(UnityEngine.Random.Range(0, w), UnityEngine.Random.Range(0, h), 0);
        }

        public void GenerateRandomMazeBreatheFirst(int w1=100, int w2=100, int w3=100, int w4=100)
        {
            float total_weight = w1+w2+w3+w4;
            float p1 = (float)w1 / total_weight;
            float p2 = p1+(float)w2 / total_weight;
            float p3 = p2+(float)w3 / total_weight;
            float p4 = p3+(float)w4 / total_weight;

            Queue<Cell> stack = new Queue<Cell>();

            // start with random cell
            Cell curLocation = GetAt(UnityEngine.Random.Range(0,w),UnityEngine.Random.Range(0,h));

            // push random location
            stack.Enqueue(curLocation);

            while (curLocation != null)
            {
                // find intact neighbours
                Cell randNeighbour  = FindRandomIntactNeighbour(curLocation.Position,p1,p2,p3,p4);

                if (randNeighbour!= null)
                {
                    stack.Enqueue( curLocation);

                    breakWall(curLocation,randNeighbour);

                    curLocation = randNeighbour;
                }
                else
                {
                    curLocation = stack.Count > 0 ? stack.Dequeue() : null;
                }
            }

            makeMazeBeginEnd();
        }
         
        public Cell begin;
        public Cell end;

        public Cell GetAt(int x, int y)
        {
            return arr[x+y*w];
        }

        public Cell GetAt(Vec3di p)
        {
            return arr[p.x+p.y*w];
        }

        private void makeMazeBeginEnd()
        {
            begin = GetAt(new Vec3di(0, UnityEngine.Random.Range(0,h),0));
            begin.left = false;

            end = GetAt(new Vec3di(w-1, UnityEngine.Random.Range(0,h),0));
            end.right = false;
        }

        private void breakWall( Cell current, Cell neighbour)
        {
            if (current.Position.x == neighbour.Position.x)
            {
                if (current.Position.y > neighbour.Position.y)
                {
                    current.upper = false;
                    neighbour.lower = false;
                }
                else
                {
                    // the neighbour is above
                    current.lower = false;
                    neighbour.upper = false;
                   
                }
            }
            else // x is not equal
            {
                if (current.Position.x > neighbour.Position.x)
                {
                    // the neighbour is left
                    current.left = false;
                    neighbour.right = false;
                }
                else
                {
                    // the neighbour is right
                    current.right = false;
                    neighbour.left = false;
                }
            }
        }

        private List<Cell> FindIntactNeighbours( Vec3di pos )
        {
            List<Cell> neighbours = new List<Cell>();

            // test left neighbour
            if (pos.x > 0)
            {
                Cell leftNeighbour = GetAt(pos.x-1, pos.y);
                if (leftNeighbour.IsIntact())     neighbours.Add(leftNeighbour);
            }
 
            // test right neighbour
            if (pos.x < (w-1))
            {
                Cell rightNeighbour = GetAt(pos.x+1, pos.y);
                if (rightNeighbour.IsIntact())    neighbours.Add(rightNeighbour);
            }

            // above neighbour
            if (pos.y > 0)
            {
                Cell upperNeighbour = GetAt(pos.x , pos.y-1);
                if (upperNeighbour.IsIntact())    neighbours.Add(upperNeighbour);
            }

            // below neighbour
            if (pos.y < (h-1))
            {
                Cell lowerNeighbour = GetAt(pos.x , pos.y+1);
                if (lowerNeighbour.IsIntact())    neighbours.Add(lowerNeighbour);
            }

            return neighbours;
        }
 
        private Cell FindRandomIntactNeighbour(Vec3di pos, float p1,float p2, float p3, float p4 )
        {
            Cell leftNeighbour = null;
            Cell rightNeighbour = null;
            Cell upperNeighbour = null;
            Cell lowerNeighbour = null;

            List<Cell> neighbours = new List<Cell>();

            // test left neighbour
            if (pos.x > 0)
            {
                leftNeighbour = GetAt(pos.x-1, pos.y);

                if (!leftNeighbour.IsIntact())
                {
                    leftNeighbour = null;
                }
            }

            // test right neighbour
            if (pos.x < (w-1))
            {
                rightNeighbour = GetAt(pos.x+1, pos.y);

                if (!rightNeighbour.IsIntact())
                {
                    rightNeighbour = null;
                }
            }

            // above neighbour
            if (pos.y > 0)
            {
                upperNeighbour = GetAt(pos.x , pos.y-1);

                if (!upperNeighbour.IsIntact())
                {
                    upperNeighbour = null;
                }
            }

            // below neighbour
            if (pos.y < (h-1))
            {
                lowerNeighbour = GetAt(pos.x , pos.y+1);

                if (!lowerNeighbour.IsIntact())
                {
                    lowerNeighbour = null;
                }
            }

            // draw random between 0 and 1
            float r = UnityEngine.Random.insideUnitCircle.magnitude;

            if (r < p1 && leftNeighbour != null)
            {
                return leftNeighbour;
            }
            else if (r < p2 && rightNeighbour != null)
            {
                return rightNeighbour;
            }
            else if (r < p3 && upperNeighbour != null)
            {
                return upperNeighbour;
            }
            else if(r < p4 && lowerNeighbour != null)
            {
                return lowerNeighbour;
            }

            // we could not deliver a prefered direction, so just return a random neighbour

            if (leftNeighbour != null)
                neighbours.Add(leftNeighbour);
            
            if (rightNeighbour != null)
                neighbours.Add(rightNeighbour);

            if (upperNeighbour != null)
                neighbours.Add(upperNeighbour);

            if (lowerNeighbour != null)
                neighbours.Add(lowerNeighbour);
             
            if (neighbours.Count > 0)
            {
                return neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
            }

            return null;
        }

        int w;
        int h;

        Cell[] arr;
    }

}
