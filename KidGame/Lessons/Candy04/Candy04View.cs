using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.Candy04
{
    public class Candy04View : MonoBehaviour, ICandy04View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action MistakeHappened;

        public Character CharSmall;
        public Character CharBig;

        public Transform CandyParentTransform;
        public GameObject CandyPrefab;

        public IconProgressBar ProgressBar;

        public Transform[] CandyPlaceHolders;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform FireworkRoot;
        public Transform WinDialogRoot;

        private WinDialog WinDialog;

        private Vector2[] CandyPlaces;

        private List<Candy> Candies = new List<Candy>();

        private int FilledCharactersCount;
        private Dictionary<Collider2D, Enum> Targets;

        private static ITimers Timers;
        private static ISoundManager SoundManager;
        private static IResourceManager ResourceManager;
        private static IGame Game;

        private UIParticle Firework;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();
            ResourceManager = CompositionRoot.GetResourceManager();

            var CandyCount = CandyPlaceHolders.Length;
            CandyPlaces = new Vector2[CandyCount];

            for (int i = 0; i < CandyCount; i++)
            {
                CandyPlaces[i] = CandyPlaceHolders[i].position;
                CandyPlaceHolders[i].gameObject.SetActive(false);
            }

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());
            CharSmall.Filled += HandlePlateFilled;
            CharBig.Filled += HandlePlateFilled;
        }

        private void HandlePlateFilled()
        {
            FilledCharactersCount++; //toDo: подумать/попробовать закрывать бокс после наполнения

            if (FilledCharactersCount >= 2)
            {
                FilledCharactersCount = 0;
                TaskComplete();
            }


        }

        

        public IPromise ShowAllObjects(List<ECandyFormAndSize> hand) // toDo: not ready yet
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = CandyPlaces.ToList();

            Candies.Clear();
            FilledCharactersCount = 0;


            for (int i = 0; i < CandyPlaces.Length; i++)
            {
                var candyGO = Instantiate(CandyPrefab, CandyParentTransform);
                candyGO.SetActive(false);

                var candy = candyGO.GetComponent<Candy>();
                candy.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                var formSize = hand[i];

                var rndCandy = GetRandomCandyByGivenForm(formSize);

                candy.Setup(rndCandy, formSize);

                candy.SetMultiTarget(Targets);
                candy.SetStartPosition(place);

                candy.SetPlaced(false);
                candy.TargetReached += HandleCandyInPlace;
                candy.MistakeMade += MistakeHappened;

                Candies.Add(candy);

            }

            foreach (Candy Candy in Candies)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(Candy.Appear);
            }

            return deferred;
        }

        private ECandies GetRandomCandyByGivenForm(ECandyFormAndSize type)
        {
            int form = (int)type % 5;
            var texture = Random.Range(0, 4);
            var resultInt = form * 4 + texture;

            return (ECandies)resultInt;
        }


        private void HandleCandyInPlace(int i)
        {
            LockAllObjects();
            Candies[i].SetPlaced(true);

            var character = Candies[i].GetMatchedCollider().GetComponentInParent<Character>();

            character.PutObjectInPlace(Candies[i])
                .Done(UnlockUnplacedObjects);
        }

        public IPromise SetupAndShowCharacters(ECandyFormAndSize small, ECandyFormAndSize big)
        {
            Targets = new Dictionary<Collider2D, Enum>()
            {
                { CharSmall.GetTargetCollider(), small },
                { CharBig.GetTargetCollider(), big },

            };

            LockAllObjects();

            CharSmall.Appear(small);

            return Timers.Wait(0.3f)
                    .Then(() => CharBig.Appear(big))
                    .Done(UnlockUnplacedObjects);
        }

        public IPromise RemoveCharacters()
        {
            CharBig.DisAppear();

            return Timers.Wait(0.01f)
                    .Then(CharSmall.DisAppear);
        }



        public void UnlockUnplacedObjects()
        {
            foreach (Candy Candy in Candies)
            {
                if (!Candy.IsPlaced()) Candy.Unlock();
            }
        }

        public void LockAllObjects()
        {
            foreach (Candy Candy in Candies)
            {
                Candy.Lock();
            }
        }

        public IPromise RemoveAllObjects()
        {
            var deferred = new Deferred();

            foreach (Candy Candy in Candies)
            {
                if (!Candy.IsPlaced())
                {
                    deferred = (Deferred)Candy.DisAppear();
                }

            }

            return deferred;
        }

        public void DestroyAllObjects()
        {
            foreach (Candy Candy in Candies)
            {
                Destroy(Candy.gameObject, 0.2f);
            }
        }

        public IPromise ShowTutorial()
        {
            BackButton.gameObject.SetActive(false);

            var mainTarget = CharSmall.GetTargetCollider().transform;

            var handTarget = Candies[0].transform;

            return Tutorial.Show(null, handTarget, mainTarget, new Vector3(120f, 60f, 0), new Vector3(-60f, 0f, 0))
                .Done(() => BackButton.gameObject.SetActive(true));
        }

        #region COMMON METHODS (WIN ROUTINE, PROGRESS BAR, HIDE/SHOW ETC)

        //======================== MOSTLY COMMON METHODS =====================================

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

        private void StartFirework()
        {
            var values = Enum.GetValues(typeof(EFireworks));

            int index = Random.Range(0, values.Length);
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

        //======================== END OF COMMON METHODS BLOCK =========================

        #endregion
    }
}