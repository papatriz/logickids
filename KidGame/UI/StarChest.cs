using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KidGame.UI
{
    public class StarChest : MonoBehaviour
    {
        public GameObject ChestOpenTop;
        public GameObject ChestOpenFG;
        public GameObject ChestClosed;

        public Transform StarRoot;
        public GameObject StarPrefab;

        //public Sprite ActiveStarImage;
        //public Sprite PlaceholderImage;

        public Transform[] StarRowInitPos;

        private IGame Game;
        private static ITimers Timers;
        private static ISoundManager SoundManager;

        private Vector3[] InsidePos = new Vector3[21];
        private float[] RowScale = new float[] { 1f, 0.86f, 0.79f };
        private float[] RowShift = new float[] { 53f, 50f, 47f };

        private Image[] StarImages = new Image[21];
        private float PlaceHolderAlpha = 0;//0.2f;

        private Tween<Vector3> RotateBounce;
        private Tween<Vector3> ScaleBounce;

        private Vector3 IniPosition;

        private float SpeedFactor;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();

            SwitchState(true);
            GenerateInsidePos();

            IniPosition = transform.position;
        }

        private void GenerateInsidePos()
        {
            for(int i=0; i<InsidePos.Length; i++)
            {
                var row = Mathf.FloorToInt((float)i / 7f);
                var col = i % 7;
                var yShift = (i % 2 > 0) ? 3f : 0;

                var x = StarRowInitPos[row].localPosition.x + RowShift[row] * col;
                var y = StarRowInitPos[row].localPosition.y;// + yShift;

                Vector3 pos = new Vector3(x,y,0);

                // FOR DEBUG OR POPULATE PLACEHOLDERS
                var starGO = Instantiate(StarPrefab, StarRoot);
                starGO.transform.localPosition = pos;
                starGO.transform.localScale = new Vector3(RowScale[row], RowScale[row], 1);
                starGO.transform.SetAsFirstSibling();
                var image = starGO.GetComponent<Image>();

                var color = image.color;
                var placeholderColor = new Color(color.r, color.g, color.b, PlaceHolderAlpha);
                image.color = placeholderColor;
                starGO.SetActive(true);
                //
                StarImages[i] = image;
                InsidePos[i] = pos;
            }
        }

        public void ResetPosition()
        {
            transform.position = IniPosition;
        }

        private void HideAllStars()
        {
            for (int i = 0; i < StarImages.Length; i++)
            {
                var color = StarImages[i].color;
                var hidddenColor = new Color(color.r, color.g, color.b, 0f);
                StarImages[i].color = hidddenColor;
                Debug.Log("Star image " + i + " turned OFF");
            }
        }

        public void ShowPreviouslyEarnedStars()
        {
            HideAllStars();

            for (int i = 0; i < Game.StarsInChest; i++)
            {
                var color = StarImages[i].color;
                var activeColor = new Color(color.r, color.g, color.b, 1f);
                StarImages[i].color = activeColor;
            }
        }

        public void ShowAllStars()
        {
            for (int i = 0; i < StarImages.Length; i++)
            {
                var color = StarImages[i].color;
                var activeColor = new Color(color.r, color.g, color.b, 1f);
                StarImages[i].color = activeColor;
                StarImages[i].transform.localPosition = InsidePos[i];
                StarImages[i].gameObject.SetActive(true);
                Debug.Log("Star image " + i + " turned ON");
            }
        }

        public IPromise MoveDown()
        {
            var deferred = new Deferred();
            var duration = 0.7f;

            var start = transform.localPosition;
            var finish = start + new Vector3(0, -450f, 0);

            Timers.Wait(0.35f)
                .Done(() => SoundManager.Play(Sounds.ECommon.ChestSlam));

            transform.TweenPosition(true)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {
                   StopAnimation();
                   SwitchState(true);
                   ShowAllStars();
                   

                   deferred.Resolve();
               })
            .Play();

            return deferred;
        }

        public IPromise RemoveAfterErruption()
        {
            var deferred = new Deferred();
            var duration = 0.5f;

            var start = transform.localPosition;
            var finish = start + new Vector3(0, -300f, 0);

            transform.TweenPosition(true)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.EaseInCubic)
               .OnCompleted((t) =>
               {
                   gameObject.SetActive(false);
                   deferred.Resolve();
               })
            .Play();

            return deferred;
        }

        public IPromise MoveStarsToItem(Transform target)
        {
            var deferred = new Deferred();

            SpeedFactor = 1f / LongestStarPath(target.position);

            Debug.Log("Earn stars");
            SoundManager.Play(Sounds.ECommon.EarnCoin);

            for (int i = 0; i < StarImages.Length; i++)
            {
                var j = i;
                deferred = (Deferred)EarnStar(j, target.position);
            }

            return deferred;
        }

        private float LongestStarPath(Vector3 target)
        {
            var longest = 0f;

            for (int i = 0; i < StarImages.Length; i++)
            {
                var dist = Vector3.Distance(StarImages[i].transform.position, target);
                if (dist > longest) longest = dist;
            }

            return longest;
        }

        private IPromise EarnStar(int i, Vector3 targetPos)
        {
            var deferred = new Deferred();

            var dist = Vector3.Distance(StarImages[i].transform.position, targetPos);
            var duration = dist * SpeedFactor;

            StarImages[i].transform.TweenPosition()
                .SetAutoCleanup(true)
                .SetDuration(duration)
                .SetEndValue(targetPos)
                .SetEasing(TweenEasingFunctions.EaseInSine)
                 .OnCompleted((t) =>
                 {
                     StarImages[i].gameObject.SetActive(false);
                     deferred.Resolve();
                 })
                 .Play();

            return deferred;
        }

        public IPromise ErruptStars()
        {
            var deferred = new Deferred();

            Debug.Log("Errupt stars");
            SoundManager.Play(Sounds.ECommon.WhooshShort);

            for (int i = 0; i < StarImages.Length; i++)
            {
                var j = i;
                deferred = (Deferred)ErruptStar(j);
            }

            return deferred;
        }

        private IPromise ErruptStar(int i)
        {
            var deferred = new Deferred();
            var duration = 1f;

            var start = InsidePos[i];
            var R = Random.Range(750f, 900f);//900f;
            var yShift = 450f;
            R += yShift;

            var angle = 328f + i * 3f;
            if (angle > 360) angle -= 360;
            var angleRad = Mathf.Deg2Rad * angle;

            var y1 = R * Mathf.Cos(angleRad);
            var x1 = R * Mathf.Sin(angleRad);

            var finish = new Vector3(x1, y1 - yShift, 0);

           // Debug.Log("Errupt star #" + i + " end:" + finish);

            StarImages[i].transform.TweenPosition(true)
                .SetAutoCleanup(true)
                .SetDuration(duration)
                .SetStartValue(start)
                .SetEndValue(finish)
                .SetEasing(TweenEasingFunctions.EaseInOutSine)
                 .OnCompleted((t) =>
                 {

                     deferred.Resolve();
                 })
                 .Play();

            return deferred;
        }

        public Vector3 GetStarPosition(int n)
        {
            var ind = Game.StarsInChest + n;

            var indMod = ind % InsidePos.Length;

            return InsidePos[indMod];
        }

        public Vector3 GetStarScale(int n)
        {
            var i = (Game.StarsInChest + n) % InsidePos.Length;
            var row = Mathf.FloorToInt((float)i / 7f); 

            Debug.Log("I="+i+" Row="+row);

            return new Vector3(RowScale[row], RowScale[row], 1);
        }

        public Transform GetStarRoot()
        {
            return StarRoot;
        }

        public void AnimateClosed()
        {
            var startRot = new Vector3(0, 0, 2);
            var endRot = new Vector3(0, 0, -2);
            var startScale = Vector3.one;
            var endScale = 1.08f * Vector3.one;


            RotateBounce = ChestClosed.transform.TweenRotation(true)
                            .SetAutoCleanup(true)
                            .SetDuration(0.3f)
                            .SetLoopType(TweenLoopType.Pingpong)
                            .SetLoopCount(0)
                            .SetEasing(TweenEasingFunctions.Bounce)
                            .SetStartValue(startRot)
                            .SetEndValue(endRot);

            ScaleBounce = ChestClosed.transform.TweenScale()
                            .SetAutoCleanup(true)
                            .SetDuration(0.2f)
                            .SetLoopType(TweenLoopType.Pingpong)
                            .SetLoopCount(0)
                            .SetEasing(TweenEasingFunctions.Bounce)
                            .SetStartValue(startScale)
                            .SetEndValue(endScale);

            RotateBounce.Play();
            ScaleBounce.Play();

         //   SoundManager.Play(Sounds.ECommon.Electro); // toDo: подобрать нормальный звук, после этого 

        }

        public void StopSound()
        {
          //  SoundManager.Stop(Sounds.ECommon.Electro);
        }

        public void StopAnimation()
        {
            if (RotateBounce != null)
            {
                RotateBounce.Stop();
                ScaleBounce.Stop();

            //    SoundManager.Stop(Sounds.ECommon.Electro);
            }

            ChestClosed.transform.eulerAngles = Vector3.zero;
            ChestClosed.transform.localScale = Vector3.one;

        }

        public void SwitchState(bool isOpen)
        {
           // if (!isOpen) SoundManager.Play(Sounds.ECommon.ChestSlam);
            ChestClosed.SetActive(!isOpen);
            ChestOpenTop.SetActive(isOpen);
            ChestOpenFG.SetActive(isOpen);
        }
    }
}