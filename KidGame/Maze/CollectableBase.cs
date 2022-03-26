using DaikonForge.Tween;
using Orbox.Async;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.Maze
{
    public class CollectableBase : MonoBehaviour, ICollectable
    {
        private Vector2 GridPosition;
        private Image Image;

        private void Awake()
        {
            Image = GetComponent<Image>();
        }



        public Vector2 GetPosition()
        {
            return GridPosition;
        }

        public void SetGridPosition(Vector2 grid)
        {
            GridPosition = grid;
        }

        public IPromise Earn(Vector2 direction)
        {
            var deferred = new Deferred();

            Image.fillMethod = Image.FillMethod.Horizontal;
            Image.fillOrigin = (int)Image.OriginHorizontal.Right;

            switch (direction)
            {
                case Vector2 v when v.Equals(Vector2.down):
                    Image.fillMethod = Image.FillMethod.Vertical;
                    Image.fillOrigin = (int)Image.OriginVertical.Bottom;
                    break;
                case Vector2 v when v.Equals(Vector2.up):
                    Image.fillMethod = Image.FillMethod.Vertical;
                    Image.fillOrigin = (int)Image.OriginVertical.Top;
                    break;
                case Vector2 v when v.Equals(Vector2.left):
                    Image.fillMethod = Image.FillMethod.Horizontal;
                    Image.fillOrigin = (int)Image.OriginHorizontal.Left;
                    break;
            }

            var duration = 2f;

            var tween = new Tween<float>();

            tween.SetStartValue(1f)
                .SetEndValue(0)
                .SetDuration(duration)
                .OnExecute(current =>
                {
                    Image.fillAmount = current;
                })
                .OnCompleted(t =>
                {
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