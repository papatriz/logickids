using Orbox.Async;
using UnityEngine;

namespace KidGame.Maze
{
    public interface IPlayer
    {
        IPromise Earn(Vector2 Pos);
        Vector2Int GetGridPosition();
        IPromise Move(Vector2 Pos, int cellCount);
        bool Moving();
        void SetGridPosition(Vector2 grid);
        void Destroy();
    }
}