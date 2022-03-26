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

namespace KidGame.Lessons.Jelly05
{
    public class Jelly05View : MonoBehaviour, IJelly05View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action MistakeHappened;

        public Plate Plate1;
        public Plate Plate2;

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

        private List<Jelly> Jellies = new List<Jelly>();
        private Dictionary<Collider2D, Enum> Targets;
        private int FilledPlatesCount;

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

            var JellyCount = JellyPlaceHolders.Length;
            JellyPlaces = new Vector2[JellyCount];

            for (int i = 0; i < JellyCount; i++)
            {
                JellyPlaces[i] = JellyPlaceHolders[i].position;
                JellyPlaceHolders[i].gameObject.SetActive(false);
            }

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());
            Plate1.Filled += HandlePlateFilled;
            Plate2.Filled += HandlePlateFilled;
        }

        private void HandlePlateFilled(Plate plate)
        {
            FilledPlatesCount++;

            plate.Toss()
                .Done(() =>
                {
                    if (FilledPlatesCount >= 2)
                    {
                        FilledPlatesCount = 0;
                        TaskComplete();
                    }
                });
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

            var mainTarget = Plate1.GetTargetCollider().transform;

            var handTarget = Jellies.Where(j => j.GetJType() == Plate1.GetCurrentQuest()).First().transform;

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

        public IPromise ShowAllObjects(List<EJellyType> hand)
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = JellyPlaces.ToList();

            Jellies.Clear();

            for (int i = 0; i < JellyPlaces.Length; i++)
            {
                var JellyGO = Instantiate(JellyPrefab, JellyParentTransform);
                JellyGO.SetActive(false);

                var Jelly = JellyGO.GetComponent<Jelly>();
                Jelly.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                Jelly.Setup(hand[i]);
                Jelly.SetStartPosition(place);

                Jelly.SetMultiTarget(Targets);
                Jelly.SetStartPosition(place);

                Jelly.SetPlaced(false);
                Jelly.TargetReached += HandleJellyInPlace;
                Jelly.MistakeMade += MistakeHappened;


                Jellies.Add(Jelly);

            }

            foreach (Jelly Jelly in Jellies)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(Jelly.Appear);
            }

            return deferred;
        }


        private void HandleJellyInPlace(int i)
        {
            LockAllObjects();
            Jellies[i].SetPlaced(true);

            var plate = Jellies[i].GetMatchedCollider().GetComponentInParent<Plate>();

            plate.PutObjectInPlace(Jellies[i])
                .Done(UnlockUnplacedObjects);
        }

        public IPromise SetupAndShowPlates(EJellyType plate1, EJellyType plate2)
        {
            Targets = new Dictionary<Collider2D, Enum>()
            {
                { Plate1.GetTargetCollider(), plate1 },
                { Plate2.GetTargetCollider(), plate2 },

            };

            var quest1GO = Instantiate(JellyPrefab, JellyParentTransform);
            quest1GO.SetActive(false);

            var jelly1 = quest1GO.GetComponent<Jelly>();
            jelly1.Setup(plate1);

            jelly1.SetPlaced(true);
            jelly1.Lock();

            Plate1.SetCurrentQuest(jelly1);

            var quest2GO = Instantiate(JellyPrefab, JellyParentTransform);
            quest2GO.SetActive(false);

            var jelly2 = quest2GO.GetComponent<Jelly>();
            jelly2.Setup(plate2);

            jelly2.SetPlaced(true);
            jelly2.Lock();

            Plate2.SetCurrentQuest(jelly2);

            Plate1.Appear();

            return Timers.Wait(0.3f)
                    .Then(Plate2.Appear);
        }

        public IPromise RemovePlates()
        {
            Plate1.DisAppear();

            return Timers.Wait(0.01f)
                    .Then(Plate2.DisAppear);
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
            foreach (Jelly Jelly in Jellies)
            {
                if (!Jelly.IsPlaced()) Jelly.Unlock();
            }
        }

        public void LockAllObjects()
        {
            foreach (Jelly Jelly in Jellies)
            {
                Jelly.Lock();
            }
        }

        public IPromise RemoveAllObjects()
        {
            var deferred = new Deferred();

            foreach (Jelly Jelly in Jellies)
            {
                if (!Jelly.IsPlaced())
                {
                    deferred = (Deferred)Jelly.DisAppear();
                }

            }

            return deferred;
        }

        public void DestroyAllObjects()
        {
            foreach (Jelly Jelly in Jellies)
            {
                Destroy(Jelly.gameObject, 0.2f);
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