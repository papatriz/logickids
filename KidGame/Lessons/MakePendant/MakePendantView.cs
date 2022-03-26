using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using DaikonForge.Tween;
using KidGame.Effects;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.MakePendant
{
    public class MakePendantView : MonoBehaviour, IMakePendantView
    {
        public event Action BackButtonPressed;
        public event Action MistakeHappened;
        public event Action PendantsFilled;

        public Pendant Pendant01;
        public Pendant Pendant02;

        public Transform GemsParentTransform;
        public GameObject GemPrefab;

        public IconProgressBar ProgressBar;

        public Transform[] GemPlaceHolders;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform FireworkRoot;
        public Transform WinDialogRoot;

        private WinDialog WinDialog;

        private Vector2[] GemPlaces;

        private List<Gem> Gems = new List<Gem>();
        private Dictionary<Collider2D, Enum> Frames;
        private Collider2D FrameBig01, FrameSmall01, FrameBig02, FrameSmall02;

        private static ITimers Timers;
        private ISoundManager SoundManager;
        private static IResourceManager ResourceManager;
        private static IGame Game;

        private UIParticle Firework;


        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();
            ResourceManager = CompositionRoot.GetResourceManager();

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            var gemsCount = GemPlaceHolders.Length;
            GemPlaces = new Vector2[gemsCount];

            for (int i = 0; i < gemsCount; i++)
            {
                GemPlaces[i] = GemPlaceHolders[i].position;
                GemPlaceHolders[i].gameObject.SetActive(false);
            }

            Pendant01.GetTargetColliders(out FrameBig01, out FrameSmall01);
            Pendant02.GetTargetColliders(out FrameBig02, out FrameSmall02);

            BackButton.onClick.AddListener(() => BackButtonPressed());
        }

        public IPromise ShowAllGems(List<EGemType> hand)
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = GemPlaces.ToList();

            Gems.Clear();

            for (int i = 0; i < hand.Count; i++)
            {
                var gemGO = Instantiate(GemPrefab, GemsParentTransform);
                gemGO.SetActive(false);

                var gem = gemGO.GetComponent<Gem>();
                gem.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                gem.SetType(hand[i]);

                gem.SetMultiTarget(Frames);
                gem.SetStartPosition(place);

                gem.SetPlaced(false);
                gem.TargetReached += HandleGemInFrame;
                gem.MistakeMade += MistakeHappened;


                Gems.Add(gem);

            }

            foreach (Gem gem in Gems)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(gem.Appear);
            }

            return deferred;
        }

        private void StartFirework()
        {
            var values = Enum.GetValues(typeof(EFireworks));

            int index = UnityEngine.Random.Range(0, values.Length);

            //index = 0; // FOR TESTS ONLY!! REMOVE AFTER

            var firework = (EFireworks)values.GetValue(index);

            Firework = ResourceManager.CreatePrefabInstance<EFireworks, UIParticle>(firework);
            Firework.transform.SetParent(FireworkRoot, false);
            Firework.gameObject.SetActive(true);

            Firework.Play();
        }

        private void StopFirework()
        {
            if (Firework != null)
            {
                Firework.Stop();
                Firework.gameObject.SetActive(false);
                Destroy(Firework.gameObject);
            }
        }

        public IPromise ShowWinDialog(int starCount)
        {

            Shading.Shade(true, 1, 0.5f);
            StartFirework();

            WinDialog.ResetToInitial();
            SoundManager.Play(Sounds.ECommon.TaDam);

            return WinDialog.Show()
                        .Then(() => WinDialog.SetStars(starCount));
        }

        public IPromise WaitForWinDialogClose()
        {
            return WinDialog.WaitForClose();
        }

        public IPromise HideWinDialog()
        {
            return WinDialog.Hide()
                .Always(StopFirework);
        }

        public void CloseAndAnimateChest()
        {
            WinDialog.CloseAndAnimateChest();
        }

        public IPromise PrepareForTransit()
        {
            WinDialog.PrepateForTransit();
            StopFirework();

            return Shading.Shade(true, 0.5f)
                .Done(() => WinDialog.gameObject.SetActive(false));
        }

        public IPromise ShowTutorial()
        {
            // TEMPORARY
            BackButton.gameObject.SetActive(false);
            //
            var arrowTargetsList = new List<Transform>();

            var mainTarget = FrameBig01.transform;
            arrowTargetsList.Add(mainTarget);

            var handTarget = Gems[0].transform;

            return Tutorial.Show(arrowTargetsList, handTarget, mainTarget, new Vector3(17f,0,0))
                .Done(() => BackButton.gameObject.SetActive(true)); // TEMPORARY
        }

        public IPromise RemoveAllGems()
        {
            var deferred = new Deferred();

            foreach (Gem gem in Gems)
            {
                if (!gem.IsPlaced())
                {
                    deferred = (Deferred)gem.DisAppear();
                }
            }

            return deferred;
        }

        public void DestroyAllGems()
        {
            foreach (Gem gem in Gems)
            {
                Destroy(gem.gameObject);
            }
        }

        public IPromise SetupAndShowPendants(EGemType bigFrame1, EGemType smallFrame1, EGemType bigFrame2, EGemType smallFrame2)
        {
            
            Pendant01.Setup(bigFrame1, smallFrame1);
            Pendant02.Setup(bigFrame2, smallFrame2);

            Pendant01.GetTargetColliders(out FrameBig01, out FrameSmall01);
            Pendant02.GetTargetColliders(out FrameBig02, out FrameSmall02);

            Debug.Log("Setup: " + FrameBig01 + " : " + FrameSmall01 + " : " + FrameBig02 + " : " + FrameSmall02);

            Frames = new Dictionary<Collider2D, Enum>()
            {
                {FrameBig01, bigFrame1 },
                {FrameSmall01, smallFrame1 },
                {FrameBig02, bigFrame2 },
                {FrameSmall02, smallFrame2 },
            };


            Pendant01.Appear();

            return Timers.Wait(0.2f)
                .Then(Pendant02.Appear);
        }

        public IPromise RemovePendants()
        {
            return Pendant02.DisAppear()
                        //.Then(() => Timers.Wait(0.2f))
                        .Then(Pendant01.DisAppear);
        }

        private void HandleGemInFrame(int i)
        {
            LockAllGems();
            Gems[i].SetPlaced(true);

            PutGemIntoFrame(i)
                .Done(UpdateTargets)
                .Done(UnlockUnplacedGems);
        }

        private void UpdateTargets()
        {
            foreach(Gem gem in Gems)
            {
                gem.SetMultiTarget(Frames);
            }
        }

        private IPromise PutGemIntoFrame(int i)
        {
            var deferred = new Deferred();
            var duration = 0.3f;

            var start = Gems[i].transform.position;
            var matchedCollider = Gems[i].GetMatchedCollider();

            var matchedTransform = matchedCollider.transform;
            var newParent = matchedTransform.parent;

            SoundManager.Play(Sounds.ECommon.PutIceInGlass);

            Gems[i].transform.SetParent(newParent, false);
            Gems[i].transform.SetAsFirstSibling();

            var finish = matchedTransform.position;
            var rotation = matchedTransform.eulerAngles;

            Gems[i].transform.TweenRotation(true)
                .SetAutoCleanup(true)
                //.SetStartValue(new Vector3(0, 0, -20f))
                .SetEndValue(rotation)
                .SetDuration(duration)
               // .SetEasing(TweenEasingFunctions.EaseInOutSine)
                .Play();

            Gems[i].transform.TweenPosition(false)
               .SetAutoCleanup(true)
               .SetDuration(duration)
               .SetStartValue(start)
               .SetEndValue(finish)
               .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {
                   Gems[i].transform.eulerAngles = matchedTransform.eulerAngles;
                   Gems[i].transform.position = matchedTransform.position;

                   Frames.Remove(matchedCollider);

                   if (Frames.Count < 1) PendantsFilled();

                   deferred.Resolve();
               })
            .Play();

            return deferred;
        }

        public IPromise TranslateToNextCheckpoint()
        {
            return ProgressBar.TranslateToNext();
        }

        public void ResetProgress()
        {
            ProgressBar.Reset();
        }

        public IPromise CheckpointReached()
        {
            return ProgressBar.ActivateNext();
        }

        public void UnlockUnplacedGems()
        {
            foreach (Gem gem in Gems)
            {
                if (!gem.IsPlaced()) gem.Unlock();
            }
        }

        public void LockAllGems()
        {
            foreach (Gem gem in Gems)
            {
                gem.Lock();
            }
        }

        public IPromise Hide()
        {
            return Shading.Shade(true, 0.5f)
                .Done(() => gameObject.SetActive(false));
        }

        public IPromise Show()
        {
            gameObject.SetActive(true);
            Shading.ShadeInstant();

            return Shading.Shade(false, 1.5f);
        }

        public void ShowInstant()
        {
            gameObject.SetActive(true);
            Shading.UnshadeInstant();
        }

        public void HideInstant()
        {
            gameObject.SetActive(false);
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
        }


    }
}