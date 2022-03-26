using Orbox.Async;
using UnityEngine;

namespace KidGame.Maze
{
    public interface ICollectable
    {
        IPromise Earn(Vector2 direction);
        Vector2 GetPosition();
        void SetGridPosition(Vector2 grid);
        void Destroy();
    }
}