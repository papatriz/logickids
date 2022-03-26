using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DaikonForge.Tween;
using KidGame.UI;
using MazeSkeleton;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KidGame.Maze
{
    public enum Dir
    {
        Up,
        Down,
        Left,
        Right
    }

    public class MazeDrawer : MonoBehaviour, IPointerDownHandler
    {
        public event Action<int> StepsTaken;
        public event Action AllItemsCollected;

        public Transform MazeParent;

        public int MazeWidth = 17;
        public int MazeHeight = 9;

        public int Range = 5;

        public GameObject WallPrefab;
        public GameObject CollectablePrefab;
        public GameObject RangeIndicatorPrefab;
        public GameObject TargetIndicatorPrefab;

        public GameObject PlayerPrefab;

        public float wallWidth = 128f;
        public float wallHeight = 120f;

        public Vector2 FootprintShift;

        public MazeProgressBar MazeProgress; // toDo: for test, have to be moved to view class

        private MazeControl MazeControl;

        private Vector2 Shift;

        private List<ICollectable> Collectable;
        private List<Vector2> AvailableCells;
        private List<GameObject> RangeIndicators; // toDO: maybe we will need separate class for it
        private List<GameObject> Walls;

        private IPlayer Player;
        private GameObject PlayerGO;

        private TargetIndicator TapTarget;

        private bool PlayerLocked;
        private bool MazeComplete;
        private Deferred MazeDrawn;

        private RectTransform FullScreenRect;

        private ISoundManager SoundManager;
        private static ITimers Timers;

        // TEST NEEDS:
        private int TotalSteps;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();
            FullScreenRect = transform.parent.GetComponent<RectTransform>();

            var targetGO = Instantiate(TargetIndicatorPrefab, transform.parent);
            targetGO.transform.SetSiblingIndex(2);

            TapTarget = targetGO.GetComponent<TargetIndicator>();
            TapTarget.Deactivate();

            PlayerGO = Instantiate(PlayerPrefab, transform.parent);
            PlayerGO.transform.SetSiblingIndex(3);
            Player = PlayerGO.GetComponent<IPlayer>();

            //debug
            Range = 6;
        }

        public IPromise Generate()
        {
            MazeDrawn = new Deferred();

           // if (MazeControl != null) MazeControl = null;
            MazeControl = new MazeControl(MazeWidth, MazeHeight);
            MazeControl.MazeReady += DrawMaze;

            var mazeWidthSceleton = (MazeWidth - 1) / 2;
            var mazeHeithSceleton = (MazeHeight - 1) / 2;

            var xShift = -(mazeWidthSceleton - 1) * wallWidth;
            var yShift = -(mazeHeithSceleton - 1) * wallHeight;
            var shiftCorr = 60f;
            Shift = new Vector2(xShift, yShift-shiftCorr);



            MazeControl.Generate();

            return MazeDrawn;
        }

        public void SetupPlayer()
        {
            Debug.Log("SETUP PLAYER");
            var start = MazeControl.GetStartPosition();

            var startPlayerPos = GridToScreen(start);

            Player.Move(startPlayerPos, 0);
            Player.SetGridPosition(start);
            PlayerGO.SetActive(true);

            ShowRange();

        }

        public void LockPlayer(bool state)
        {
            PlayerLocked = state;
        }

        private void ShowCollectable() // toDo: здесь будем расставлять ништяки
        {
            Collectable = new List<ICollectable>();

            var cellList = MazeControl.GetImpasses();
            int count = 0;


            foreach (Vector2 cellPos in cellList)
            {
                var pos = GridToScreen(cellPos);

                GameObject impObj = Instantiate(CollectablePrefab, MazeParent);
                impObj.transform.localPosition = pos;
                impObj.SetActive(true);
               // impObj.transform.SetAsLastSibling();
                var collectable = impObj.GetComponent<ICollectable>();
                collectable.SetGridPosition(cellPos);

                Collectable.Add(collectable);
                count++;

                if (count >= 3) break;
            }
        }


        public void DrawMaze()
        {
            TotalSteps = 0;
            MazeComplete = false;

            ShowCollectable();

            Walls = new List<GameObject>();

            var walls = MazeControl.GetWalls();

            foreach (Vector2 wallPos in walls)
            {
                var screenPos = GridToScreen(wallPos);
                CreateWall(screenPos, WallPrefab);
            }

           // ShowRange(); // FUUUUUUuuuuuuUUUUuuuuCCCccccckkkkk!
            MazeDrawn.Resolve();

        }

        public void EraseMaze()
        {
            HideRange();

            Debug.Log("ERASE MAZE");

            PlayerGO.SetActive(false);

            foreach(ICollectable c in Collectable)
            {
                c.Destroy();
            }

            foreach(GameObject go in Walls)
            {
                Destroy(go);
            }

            MazeControl.DeleteMaze();
        }

        public int GetMinimumSteps()
        {
            var result = 0;

            var start = Player.GetGridPosition();
            var itemCount = Collectable.Count;

            switch (itemCount)
            {
                case 1:

                    result = MazeControl.GetPath(start, Collectable[0].GetPosition()).Count;
                    break;

                case 2:
                    var pos1 = Collectable[0].GetPosition();
                    var pos2 = Collectable[1].GetPosition();

                    var v1 = MazeControl.GetPath(start, pos1).Count + MazeControl.GetPath(pos1, pos2).Count;
                    var v2 = MazeControl.GetPath(start, pos2).Count + MazeControl.GetPath(pos2, pos1).Count;

                    result = Mathf.Min(v1, v2);

                    break;

                case 3:
                    var tpos1 = Collectable[0].GetPosition();
                    var tpos2 = Collectable[1].GetPosition();
                    var tpos3 = Collectable[2].GetPosition();

                    var tv1 = MazeControl.GetPath(start, tpos1).Count + MazeControl.GetPath(tpos1, tpos2).Count + MazeControl.GetPath(tpos2, tpos3).Count; // 1 - 2 - 3
                    var tv2 = MazeControl.GetPath(start, tpos1).Count + MazeControl.GetPath(tpos1, tpos3).Count + MazeControl.GetPath(tpos3, tpos2).Count; // 1 - 3 - 2

                    var tv3 = MazeControl.GetPath(start, tpos2).Count + MazeControl.GetPath(tpos2, tpos1).Count + MazeControl.GetPath(tpos1, tpos3).Count; // 2 - 1 - 3
                    var tv4 = MazeControl.GetPath(start, tpos2).Count + MazeControl.GetPath(tpos2, tpos3).Count + MazeControl.GetPath(tpos3, tpos1).Count; // 2 - 3 - 1

                    var tv5 = MazeControl.GetPath(start, tpos3).Count + MazeControl.GetPath(tpos3, tpos1).Count + MazeControl.GetPath(tpos1, tpos2).Count; // 3 - 1 - 2
                    var tv6 = MazeControl.GetPath(start, tpos3).Count + MazeControl.GetPath(tpos3, tpos2).Count + MazeControl.GetPath(tpos2, tpos1).Count; // 3 - 2 - 1 

                    List<int> paths = new List<int> { tv1, tv2, tv3, tv4, tv5, tv6 };
                    result = paths.Min();

                    break;

                default:

                    Debug.Log("EROOR! Invalid items count - " + itemCount);
                    break;
            }

            return result;
        }

        public void OnPointerDown(PointerEventData eventData)
        {

            var screenPos = eventData.position;
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(FullScreenRect, screenPos, null, out canvasPos);

            var gridPos = ScreenToGrid(canvasPos);

            var canReach = AvailableCells.Contains(gridPos);

            if (!Player.Moving() && canReach && !PlayerLocked)
            {
                HideRange();
                LockPlayer(true);

                var tapPos = GridToScreen(gridPos) + FootprintShift;
                TapTarget.Activate(tapPos);

                MovePlayer(gridPos)
                    .Done(TapTarget.Deactivate)
                    .Done(() => LockPlayer(false));
            }
            else
                if (!canReach && !PlayerLocked)
            {
                LockPlayer(true);
                SoundManager.Play(Sounds.ECommon.UhUh);
                Timers.Wait(0.3f)
                    .Then(IndicateWrongTap)
                    .Done(() => LockPlayer(false));
            }
        }

        private IPromise IndicateWrongTap() // Flash accessible range indicators, play oops sound 
        {
            var deferred = new Deferred();

           

            foreach(GameObject r in RangeIndicators)
            {
                var control = r.GetComponent<RangeIndicator>();

                if (r != null) deferred = (Deferred)control.Flash();
            }

            return deferred;
        }

        private IPromise MovePlayer(Vector2 gridPos)
        {
            var deferred = new Deferred();

            var path = MazeControl.GetPath(Player.GetGridPosition(), gridPos);
            path.Reverse();
            var previous = (Vector2)Player.GetGridPosition();
            var step = 0;

            foreach(Vector2 stepPos in path)
            {
                var collectIndex = Collectable.FindIndex(item => item.GetPosition() == stepPos);
                var isCollectable = collectIndex >= 0;

                if (isCollectable)
                {
                    Vector2 dirVector = stepPos - previous;
                    deferred = (step > 0) ? (Deferred)deferred.Then(() => CollectItem(collectIndex, dirVector)) : (Deferred)CollectItem(collectIndex, dirVector);
                    if (MazeComplete) break;
                }
                else
                {
                    var newScreenPos = GridToScreen(stepPos);
                    deferred = (step > 0) ? (Deferred)deferred.Then(() => Player.Move(newScreenPos, 1)) : (Deferred)Player.Move(newScreenPos, 1);
                }

                previous = stepPos;
                step++;

            }

            if (!MazeComplete) deferred = (Deferred)deferred.Done(ShowRange);

            //TEST
            TotalSteps += step;
            MazeProgress.SetNewValue(TotalSteps);
            StepsTaken(step);

            Player.SetGridPosition(gridPos);

            return deferred;
        }


        private void ShowRange()
        {
            if (MazeComplete) return;

            AvailableCells = MazeControl.GetAccessibleCells(Player.GetGridPosition(), Range);
            RangeIndicators = new List<GameObject>();

            foreach (Vector2 pos in AvailableCells)
            {
                GameObject tile = Instantiate(RangeIndicatorPrefab, MazeParent);
                tile.transform.localPosition = GridToScreen(pos)+FootprintShift;
                RangeIndicators.Add(tile);
            }
        }

        private void HideRange()
        {
            foreach (GameObject tile in RangeIndicators)
            {
                Destroy(tile);
            }
        }


        private IPromise CollectItem(int index, Vector2 direction) //toDo: Move animation to Collectable class
        {
            var targetPlayerPos = GridToScreen(Collectable[index].GetPosition());
            Player.SetGridPosition(Collectable[index].GetPosition());

            return Deferred.All(Collectable[index].Earn(direction), Player.Earn(targetPlayerPos))
                        .Done(() => HandleEarningCollectable(index));
        }


        private void HandleEarningCollectable(int index)
        {
            Collectable.RemoveAt(index);

            if (Collectable.Count == 0)
            {
                Debug.Log("All earned");
                MazeComplete = true;
                AllItemsCollected();
            }
        }

        private void CreateWall(Vector2 pos, GameObject prefab)
        {
            GameObject wall = Instantiate(prefab, MazeParent);
            wall.transform.localPosition = pos;
            wall.transform.SetSiblingIndex(1);
            Walls.Add(wall);

        }

        private Vector2 GridToScreen(Vector2 gridPosition)
        {
            var posX = gridPosition.x * wallWidth + Shift.x;
            var posY = gridPosition.y * wallHeight + Shift.y;
            var result = new Vector2(posX, posY);

            return result;
        }

        private Vector2 ScreenToGrid(Vector2 screenPos)
        {
            var shiftedX = screenPos.x - Shift.x;


            var gridX = Mathf.Floor((screenPos.x - Shift.x + wallWidth / 2) / wallWidth );
            var gridY = Mathf.Floor((screenPos.y - Shift.y + wallHeight / 2) / wallHeight );

            var result = new Vector2(gridX, gridY);

            return result;
        }

        private Vector2 DirToVector(Dir dir)
        {
            var vector = new Vector2(1, 0);

            switch (dir)
            {
                case Dir.Down:
                    vector = new Vector2(0, -1);
                    break;
                case Dir.Up:
                    vector = new Vector2(0, 1);
                    break;
                case Dir.Left:
                    vector = new Vector2(-1, 0);
                    break;
            }

            return vector;
        }
    }
}