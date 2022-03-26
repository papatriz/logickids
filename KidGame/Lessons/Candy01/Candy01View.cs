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

namespace KidGame.Lessons.Candy01
{
    public class Candy01View : MonoBehaviour, ICandy01View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action MistakeHappened;

        public Plate Plate;

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
            Plate.Filled += HandlePlateFilled;
        }

        private void HandlePlateFilled()
        {
            TaskComplete();
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

        public IPromise ShowTutorial()
        {
            BackButton.gameObject.SetActive(false);

            var mainTarget = Plate.GetTargetCollider().transform;

            var handTarget = Candies[0].transform;

            return Tutorial.Show(null, handTarget, mainTarget, new Vector3(0f, 80f, 0), new Vector3(-60f, 0f, 0))
                .Done(() => BackButton.gameObject.SetActive(true));
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

        public IPromise ShowAllObjects(List<ECandies> hand)
        {
            var deferred = new Deferred();
            float wait = 0f;

            var targetCollider = Plate.GetTargetCollider();

            List<Vector2> PlacesHeap = CandyPlaces.ToList();

            Candies.Clear();

            var properType = GetCandyTexture(hand[0]);

            for (int i = 0; i < CandyPlaces.Length; i++)
            {
                var CandyGO = Instantiate(CandyPrefab, CandyParentTransform);
                CandyGO.SetActive(false);

                var Candy = CandyGO.GetComponent<Candy>();
                Candy.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                Candy.Setup(hand[i]);
                Candy.SetType(GetCandyTexture(hand[i]));
                Candy.SetForm(GetCandyForm(hand[i]));

                Candy.SetTarget(targetCollider, properType);
                Candy.SetStartPosition(place);

                Candy.SetPlaced(false);
                Candy.TargetReached += HandleCandyInPlace;
                Candy.MistakeMade += MistakeHappened;


                Candies.Add(Candy);

            }

            foreach (Candy Candy in Candies)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(Candy.Appear);
            }

            return deferred;
        }

        private ECandyTexture GetCandyTexture(ECandies candy)
        {
            var resInt = (int)candy % 4;

          //  Debug.Log("Candy: " + candy + " Texture: " + (ECandyTexture)resInt);

            return (ECandyTexture)resInt;
        }

        private ECandyForm GetCandyForm(ECandies candy)
        {
            var candyInt = (int)candy;

            var resInt = Mathf.FloorToInt((float)candyInt / 4f);

            return (ECandyForm)resInt;
        }

        private void HandleCandyInPlace(int i)
        {
            LockAllObjects();
            Candies[i].SetPlaced(true);

            Plate.PutObjectInPlace(Candies[i])
                .Done(UnlockUnplacedObjects);
        }

        public IPromise SetupAndShowPlate(ECandies candyType)
        {
            var candyGO = Instantiate(CandyPrefab, CandyParentTransform);
            candyGO.SetActive(false);

            var candy = candyGO.GetComponent<Candy>();
            candy.Setup(candyType);
            candy.SetType(GetCandyTexture(candyType));
            candy.SetForm(GetCandyForm(candyType));
            candy.SetPlaced(true);
            candy.Lock();

            Plate.SetCurrentQuest(candy);

            return Plate.Appear();
        }

        public IPromise RemovePlate()
        {
            return Plate.DisAppear();
        }

        public IPromise TossPlate()
        {
            return Plate.Toss();
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