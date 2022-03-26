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

namespace KidGame.Lessons.Fruit03
{
    public class Fruit03View : MonoBehaviour, IFruit03View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action MistakeHappened; 

        public Box Box01;
        public Box Box02;

        public Transform FruitParentTransform;
        public GameObject FruitPrefab;

        public IconProgressBar ProgressBar;

        public Transform[] FruitPlaceHolders;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform FireworkRoot;
        public Transform WinDialogRoot;

        private WinDialog WinDialog;

        private Vector2[] FruitPlaces;

        private List<Fruit> Fruits = new List<Fruit>();

        private static ITimers Timers;
        private static ISoundManager SoundManager;
        private static IResourceManager ResourceManager;
        private static IGame Game;

        private UIParticle Firework;

        private Dictionary<Collider2D, Enum> Targets;
        private int FilledBoxCount;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();
            ResourceManager = CompositionRoot.GetResourceManager();

            var fruitCount = FruitPlaceHolders.Length;
            FruitPlaces = new Vector2[fruitCount];

            for (int i = 0; i < fruitCount; i++)
            {
                FruitPlaces[i] = FruitPlaceHolders[i].position;
                FruitPlaceHolders[i].gameObject.SetActive(false);
            }

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());
            Box01.Filled += HandleBoxFilled;
            Box02.Filled += HandleBoxFilled;
        }

        public IPromise ShowAllFruits()
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = FruitPlaces.ToList();

            Fruits.Clear();
            FilledBoxCount = 0;

            var spriteIndex = Random.Range(0, 4);
            var spriteChangeCounter = 0;

            for (int i = 0; i < FruitPlaces.Length; i++)
            {
                var fruitGO = Instantiate(FruitPrefab, FruitParentTransform);
                fruitGO.SetActive(false);

                var fruit = fruitGO.GetComponent<Fruit>();
                fruit.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                var size = (i % 2 == 0) ? EFruitType.Big : EFruitType.Small;
                fruit.SetType(size);
                fruit.SetSprite(spriteIndex);

                spriteChangeCounter++;
                if (spriteChangeCounter > 1)
                {
                    spriteChangeCounter = 0;
                    spriteIndex = Random.Range(0, 4);
                }

                fruit.SetMultiTarget(Targets);
                fruit.SetStartPosition(place);

                fruit.SetPlaced(false);
                fruit.TargetReached += HandleFruitInPlace;
                fruit.MistakeMade += MistakeHappened;


                Fruits.Add(fruit);

            }

            foreach (Fruit fruit in Fruits)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(fruit.Appear);
            }

            return deferred;
        }

        private void HandleBoxFilled(EFruitType type)
        {
            FilledBoxCount++; //toDo: подумать/попробовать закрывать бокс после наполнения

            var boxToClose = (type == EFruitType.Big) ? Box02 : Box01;
            boxToClose.CloseBox();

            if (FilledBoxCount >= 2)
            {
                FilledBoxCount = 0;
                TaskComplete();
            }

        }

        private void HandleFruitInPlace(int i)
        {
            LockAllFruits();
            Fruits[i].SetPlaced(true);

            var box = Fruits[i].GetMatchedCollider().GetComponentInParent<Box>();

            box.PutFruitInPlace(Fruits[i])
                .Done(UnlockUnplacedFruits);
        }

        public IPromise SetupAndShowBoxes()
        {
            Targets = new Dictionary<Collider2D, Enum>()
            {
                { Box01.GetTargetCollider(), Box01.GetCurrentType() },
                { Box02.GetTargetCollider(), Box02.GetCurrentType() },

            };

            Box02.Appear();

            return Timers.Wait(0.3f)
                    .Then(Box01.Appear);
        }

        public IPromise CloseBoxes()
        {
            Box02.CloseBox();

            return Box01.CloseBox();
        }

        public IPromise RemoveBoxes()
        {
            Box02.DisAppear();

            return Timers.Wait(0.01f)
                    .Then(Box01.DisAppear);
        }



        public void LockAllFruits()
        {
            foreach (Fruit fruit in Fruits)
            {
                fruit.Lock();
            }
        }

        public IPromise RemoveAllFruits()
        {
            var deferred = new Deferred();

            foreach (Fruit fruit in Fruits)
            {
                if (!fruit.IsPlaced())
                {
                    deferred = (Deferred)fruit.DisAppear();
                }

            }

            return deferred;
        }

        public void DestroyAllFruits()
        {
            foreach (Fruit fruit in Fruits)
            {
                Destroy(fruit.gameObject, 0.2f);
            }
        }


        // MOSTLY COMMON METHODS

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

            var mainTarget = Box02.GetTargetCollider().transform;

            var handTarget = Fruits[0].transform;

            return Tutorial.Show(null, handTarget, mainTarget, new Vector3(30f, 150f, 0), new Vector3(-60f, 0f, 0))
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

        public void UnlockUnplacedFruits()
        {
            foreach (Fruit fruit in Fruits)
            {
                if (!fruit.IsPlaced()) fruit.Unlock();
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