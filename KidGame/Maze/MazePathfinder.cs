using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KidGame.Maze
{
    public class MazePathfinder
    {
        private int[,] MazeWork;
        private int[,] MazeOrigin;

        private int Width;
        private int Height;

        private int EndX;
        private int EndY;

        private Vector2Int Target;
        private Vector2Int Start;

        public MazePathfinder(int[,] maze)
        {
            //MazeWork = null;
            //MazeOrigin = null;
            Debug.Log("Pathfinder ctor");
            Debug.Log("maze hash=" + maze.GetHashCode());

            MazeOrigin = maze;
            Debug.Log("MazeOrigin hash=" + MazeOrigin.GetHashCode());

            Width = maze.GetLength(0);
            Height = maze.GetLength(1);
        }

        public List<Vector2Int> Find()
        {
            SendWave();

            var path = GetPath();

            return path;
        }

        public void SetTarget(Vector2 pos)
        {
            int arrayX = (int)pos.x + 1;
            int arrayY = (int)pos.y + 1;
            Target = new Vector2Int(arrayX, arrayY);
        }

        public void SetStart(Vector2 pos)
        {
            int arrayX = (int)pos.x + 1;
            int arrayY = (int)pos.y + 1;
            Start = new Vector2Int(arrayX, arrayY);
        }

        private List<Vector2Int> GetPath()
        {
            var path = new List<Vector2Int>();

            bool reachStart = false;
            int curX = EndX;
            int curY = EndY;

            path.Add(new Vector2Int(EndX, EndY)); // Add destination cell. May remove if dont needed

            while (!reachStart)
            {
                var steps = MazeWork[curX, curY];

                if (MazeWork[curX + 1, curY] == steps-1)
                {
                    curX = curX + 1;
                    path.Add(new Vector2Int(curX, curY));
                }
                else
                if (MazeWork[curX - 1, curY] == steps - 1)
                {
                    curX = curX - 1;
                    path.Add(new Vector2Int(curX, curY));
                }
                else
                if (MazeWork[curX, curY + 1] == steps - 1)
                {
                    curY = curY + 1;
                    path.Add(new Vector2Int(curX, curY));
                }
                else
                if (MazeWork[curX, curY - 1] == steps - 1)
                {
                    curY = curY - 1;
                    path.Add(new Vector2Int(curX, curY));
                }
                else
                {
                    Debug.Log("Backtrace FAILED");
                    break;
                }

                reachStart = MazeWork[curX, curY] == 0;
            }

            if (reachStart) path.RemoveAt(path.Count - 1); // dont need start cell

            return path;
        }

        private void SendWave()
        {
            bool foundEnd = false;
            int it = 0;

            MazeWork = new int[Width, Height];
            Array.Copy(MazeOrigin, MazeWork, MazeOrigin.Length);

            MazeWork[Target.x, Target.y] = -3;
            MazeWork[Start.x, Start.y] = 0;

            while (!foundEnd)
            {
                bool foundEmpty = false;

                for (int x = 0; x < Width && !foundEnd; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        if (MazeWork[x, y] == it)
                        {
                            if (x < Width - 1)
                            {
                                int east = MazeWork[x + 1, y];
                                if (east == -3)
                                {
                                    foundEnd = true;

                                    EndX = x + 1;
                                    EndY = y;
                                    MazeWork[x + 1, y] = it + 1;

                                    break;
                                }
                                else if (east == -1)
                                {
                                    MazeWork[x + 1, y] = it + 1;
                                    foundEmpty = true;
                                }
                            }

                            if (x > 0)
                            {
                                int west = MazeWork[x - 1, y];
                                if (west == -3)
                                {
                                    foundEnd = true;

                                    EndX = x - 1;
                                    EndY = y;
                                    MazeWork[x - 1, y] = it + 1;

                                    break;
                                }
                                else if (west == -1)
                                {
                                    MazeWork[x - 1, y] = it + 1;
                                    foundEmpty = true;
                                }
                            }

                            if (y < Height - 1)
                            {
                                int south = MazeWork[x, y + 1];
                                if (south == -3)
                                {
                                    foundEnd = true;

                                    EndX = x;
                                    EndY = y + 1;
                                    MazeWork[x, y + 1] = it + 1;

                                    break;
                                }
                                else if (south == -1)
                                {
                                    MazeWork[x, y + 1] = it + 1;
                                    foundEmpty = true;
                                }
                            }

                            if (y > 0)
                            {
                                int north = MazeWork[x, y - 1];
                                if (north == -3)
                                {
                                    foundEnd = true;

                                    EndX = x;
                                    EndY = y - 1;
                                    MazeWork[x, y - 1] = it + 1;

                                    break;
                                }
                                else if (north == -1)
                                {
                                    MazeWork[x, y - 1] = it + 1;
                                    foundEmpty = true;
                                }
                            }
                        }
                    }
                }

                if (!foundEnd && !foundEmpty)
                {
                    Debug.Log("No solution");
                    break;
                }

                it++;
            }


        }
    }
}