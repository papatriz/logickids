using System;
using System.Collections.Generic;
using MazeSkeleton;
using UnityEngine;


namespace KidGame.Maze
{
    public class MazeControl 
    {
        public event Action MazeReady;

        private MazeSkeleton2D Maze;

        private int[,] MazeArray;
        private MazePathfinder Pathfinder;

        private int Width;
        private int Height;

        private Vector2 mazePosition = Vector2.zero;

        private float tunnelWigth = 1f;
        private int wallPerCellSide = 3;
        private int mazeSeed = 1000;
        private bool useSeed = false;
        private int roomCount = 2;
        private int roomWidth = 2;
        private int roomHeight = 2;
        private bool genRooms = false;

        private RoomParams rPar = new RoomParams();

        public MazeControl(int mazeWidth, int mazeHeight)
        {
            Width = mazeWidth;
            Height = mazeHeight;

            int mazeWidthSkeleton = (int)(mazeWidth - 1) / 2;
            int mazeHeightSkeleton = (int)(mazeHeight - 1) / 2;

            Maze = new MazeSkeleton2D(mazeWidthSkeleton, mazeHeightSkeleton, tunnelWigth, wallPerCellSide, mazePosition);
            AppendMazeParams();

            Maze.GenerationIsFault = OnMazeGenerationFault;
            Maze.MazeGenerated = OnMazeGenerated;
        }

        public void Generate()
        {
            Maze.GenerateMaze();
        }

        public Vector2 GetStartPosition()
        {
            return Maze.GetStartCell().Position;
        }

        public List<Vector2> GetImpasses()
        {
            var result = new List<Vector2>();
            var impasses = Maze.GetImpassess();
            var startPos = GetStartPosition();

            foreach(MazeCell imp in impasses)
            {
                if (imp.Position != startPos) result.Add(imp.Position);
            }

            if (result.Count < 2)
            {
                var other = Maze.GetExitCell();
                result.Add(other.Position);
            }

            return result;
        }

        public List<Vector2> GetWalls()
        {
            return Maze.GetWallPositions();
        }

        public List<Vector2> GetPath(Vector2 start, Vector2 finish)
        {
            var result = new List<Vector2>();

            Pathfinder.SetStart(start);
            Pathfinder.SetTarget(finish);

            var path = Pathfinder.Find();

            foreach(Vector2Int step in path)
            {
                Vector2 pos = new Vector2(step.x - 1, step.y - 1);
                result.Add(pos);
            }

            return result;
        }

        public List<Vector2> GetAccessibleCells(Vector2 from, int range)
        {
            var result = new List<Vector2>();

            Pathfinder.SetStart(from);

            Vector2 vector, currentPos;

            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    vector = new Vector2(i, j);

                    if (vector != Vector2.zero)
                    {
                        currentPos = from + vector;


                        if (currentPos.x >= 0 && currentPos.y >= 0 && currentPos.x < MazeArray.GetLength(0) - 1 && currentPos.y < MazeArray.GetLength(1) - 1)
                        {
                            int arrX = (int)currentPos.x + 1;
                            int arrY = (int)currentPos.y + 1;

                            if (MazeArray[arrX, arrY] == -1) // cell is empty
                            {
                                Pathfinder.SetTarget(currentPos);

                                var path = Pathfinder.Find();

                                if (path.Count > 0 && path.Count <= range) result.Add(currentPos);

                            }
                        }
                    }
                }
            }

            return result;
        }

        private void AppendMazeParams()
        {
            rPar.roomCount = roomCount;
            rPar.roomHeight = roomHeight;
            rPar.roomWidth = roomWidth;

            Maze.SetRoomParams(genRooms, rPar);
            Maze.SetUseLongWay(false);
            Maze.SetMazeSeed(useSeed, mazeSeed);
        }

        private void OnMazeGenerated()
        {
            MazeToArray();
            Pathfinder = new MazePathfinder(MazeArray);

            MazeReady();
        }
        private void OnMazeGenerationFault()
        {
            DeleteMaze();
            GenerateMaze();
        }

        public void DeleteMaze()
        {
            Maze.DeleteMaze();
            MazeArray = null;
            Pathfinder = null;
        }

        private void GenerateMaze()
        {
            Maze.GenerateMaze();
        }

        private void MazeToArray()
        {

            MazeArray = new int[Width, Height];

            for (int i = 0; i < MazeArray.GetLength(0); i++)
                for (int j = 0; j < MazeArray.GetLength(1); j++)
                {
                    var pos = new Vector2(i - 1, j - 1);
                    if (Maze.IsThereWall(pos))
                    {
                        MazeArray[i, j] = -2;
                    }
                    else
                    {
                        MazeArray[i, j] = -1;
                    }
                }
        }
    }
}