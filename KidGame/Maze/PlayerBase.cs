using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;

namespace KidGame.Maze
{
    public class PlayerBase : MonoBehaviour, IPlayer
    {
        private Vector2Int GridPosition;
        private bool IsMoving = false;

        private ISoundManager SoundManager;

        private void Awake()
        {
            SoundManager = CompositionRoot.GetSoundManager();
        }

        public Vector2Int GetGridPosition()
        {
            return GridPosition;
        }

        public void SetGridPosition(Vector2 grid)
        {
            GridPosition = new Vector2Int((int)grid.x, (int)grid.y);
        }

        public bool Moving()
        {
            return IsMoving;
        }


        public IPromise Move(Vector2 Pos, int cellCount)
        {
            var deferred = new Deferred();

            if (cellCount <= 0)
            {
                this.transform.localPosition = Pos;
                deferred.Resolve();
            }
            else
            {
                IsMoving = true;

                SoundManager.Play(Sounds.ECommon.Footsteps);

                var duration = cellCount * 0.3f;

                transform.TweenPosition(true)
                     .SetAutoCleanup(true)
                     .SetDuration(duration)
                     .SetEndValue(Pos)
                     .OnCompleted((t) =>
                     {
                         IsMoving = false;
                         deferred.Resolve();
                     })
                     .Play();
            }

            return deferred;
        }

        public IPromise Earn(Vector2 Pos)
        {
            var deferred = new Deferred();

            IsMoving = true;

            var duration = 2f;
            var factor = 1.1f;

            SoundManager.Play(Sounds.ECommon.Chew);

            transform.TweenScale()
                 .SetAutoCleanup(true)
                 .SetDuration(duration / 8)
                 .SetStartValue(Vector2.one)
                 .SetEndValue(factor * Vector2.one)
                 .SetLoopType(TweenLoopType.Pingpong)
                 .SetLoopCount(8)
                 .OnCompleted((t) =>
                 {

                 })
                 .Play();

            transform.TweenPosition(true)
                    .SetAutoCleanup(true)
                    .SetDuration(duration)
                    .SetEndValue(Pos)
                    .OnCompleted((t) =>
                    {
                        IsMoving = false;
                        deferred.Resolve();
                    })
                    .Play();


            return deferred;
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}