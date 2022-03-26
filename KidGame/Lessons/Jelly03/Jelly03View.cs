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

namespace KidGame.Lessons.Jelly03
{
    public class Jelly03View : MonoBehaviour, IJelly03View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action MistakeHappened;

        public Egg Egg01;
        public Egg Egg02;

        public Transform JellyParentTransform;
        public GameObject JellyPrefab;

        public IconProgressBar ProgressBar;

        public Transform[] JellyPlaceHolders;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform FireworkRoot;
        public Transform WinDialogRoot;

        private WinDialog WinDialog;

        private Vector2[] JellyPlaces;

        private List<Jelly> Jellys = new List<Jelly>();

        private static ITimers Timers;
        private static ISoundManager SoundManager;
        private static IResourceManager ResourceManager;
        private static IGame Game;

        private UIParticle Firework;

        private Dictionary<Collider2D, Enum> Targets;
        private int FilledEggCount;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            SoundManager = CompositionRoot.GetSoundManager();
            ResourceManager = CompositionRoot.GetResourceManager();

            var jellyCount = JellyPlaceHolders.Length;
            JellyPlaces = new Vector2[jellyCount];

            for (int i = 0; i < jellyCount; i++)
            {
                JellyPlaces[i] = JellyPlaceHolders[i].position;
                JellyPlaceHolders[i].gameObject.SetActive(false);
            }

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());
            Egg01.Filled += HandleEggFilled;
            Egg02.Filled += HandleEggFilled;
        }

        public IPromise ShowAllObjects()
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = JellyPlaces.ToList();

            Jellys.Clear();
            FilledEggCount = 0;

            for (int i = 0; i < JellyPlaces.Length; i++)
            {
                var jellyGO = Instantiate(JellyPrefab, JellyParentTransform);
                jellyGO.SetActive(false);

                var jelly = jellyGO.GetComponent<Jelly>();
                jelly.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                jelly.SetMultiTarget(Targets);
                jelly.SetStartPosition(place);
                jelly.SetType((EColors)i);

                jelly.SetPlaced(false);
                jelly.TargetReached += HandleJellyInPlace;
                jelly.MistakeMade += MistakeHappened;


                Jellys.Add(jelly);

            }

            foreach (Jelly jelly in Jellys)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(jelly.Appear);
            }

            return deferred;
        }

        private void HandleEggFilled(Egg egg)
        {
            FilledEggCount++; 

            egg.Close()
                .Done(() =>
                {
                    if (FilledEggCount >= 2)
                    {
                        FilledEggCount = 0;
                        TaskComplete();
                    }
                });
        }

        private void HandleJellyInPlace(int i)
        {
            LockAllObjects();
            Jellys[i].SetPlaced(true);

            var egg = Jellys[i].GetMatchedCollider().GetComponentInParent<Egg>();

            egg.PutObjectInPlace(Jellys[i])
                .Done(UnlockUnplacedObjects);
        }

        public IPromise SetupAndShowEggs(List<EColors> hand)
        {
            Targets = new Dictionary<Collider2D, Enum>()
            {
                { Egg01.GetTargetCollider(), hand[0] },
                { Egg02.GetTargetCollider(), hand[1] },

            };

            Egg01.Setup(hand[0]);
            Egg02.Setup(hand[1]);

            Egg02.Appear();

            return Timers.Wait(0.3f)
                    .Then(Egg01.Appear);
        }

        public IPromise CloseEggs()
        {
            Egg02.Close();

            return Egg01.Close();
        }

        public IPromise RemoveEggs()
        {
            Egg02.DisAppear();

            return Timers.Wait(0.01f)
                    .Then(Egg01.DisAppear);
        }



        public void LockAllObjects()
        {
            foreach (Jelly jelly in Jellys)
            {
                jelly.Lock();
            }
        }

        public IPromise RemoveAllObjects()
        {
            var deferred = new Deferred();

            foreach (Jelly jelly in Jellys)
            {
                if (!jelly.IsPlaced())
                {
                    deferred = (Deferred)jelly.DisAppear();
                }

            }

            return deferred;
        }

        public void DestroyAllObjects()
        {
            foreach (Jelly jelly in Jellys)
            {
                Destroy(jelly.gameObject, 0.2f);
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

            var mainTarget = Egg01.GetTargetCollider().transform;

            var handTarget = Jellys.Where(j => j.GetMatchType()==Egg01.GetCurrentType()).First().transform;

            return Tutorial.Show(null, handTarget, mainTarget, new Vector3(0f, 80f, 0), new Vector3(0f, 0f, 0))
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

        public void UnlockUnplacedObjects()
        {
            foreach (Jelly jelly in Jellys)
            {
                if (!jelly.IsPlaced()) jelly.Unlock();
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