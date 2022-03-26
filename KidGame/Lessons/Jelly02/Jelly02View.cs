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

namespace KidGame.Lessons.Jelly02
{
    public class Jelly02View : MonoBehaviour, IJelly02View
    {
        public event Action TaskComplete;
        public event Action BackButtonPressed;
        public event Action MistakeHappened;

        public Mouse Mouse;

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
        }

        private void StartFirework()
        {
            var values = Enum.GetValues(typeof(EFireworks));

            int index = UnityEngine.Random.Range(0, values.Length);
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

            var mainTarget = Mouse.GetTargetCollider().transform; 

            var handTarget = Jellys[0].transform;

            return Tutorial.Show(null, handTarget, mainTarget, new Vector3(130f, -70f, 0), new Vector3(-60f, 0f, 0))
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

        public IPromise ShowAllJellys(List<EJellyForm> hand)
        {
            var deferred = new Deferred();
            float wait = 0f;

            var targetCollider = Mouse.GetTargetCollider();
            var targetType = Mouse.GetCurrentType();

            List<Vector2> PlacesHeap = JellyPlaces.ToList();

            Jellys.Clear();

            for (int i = 0; i < hand.Count; i++)
            {
                var jellyGO = Instantiate(JellyPrefab, JellyParentTransform);
                jellyGO.SetActive(false);

                var jelly = jellyGO.GetComponent<Jelly>();
                jelly.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                jelly.SetType(hand[i]);

                jelly.SetTarget(targetCollider, targetType);
                jelly.SetStartPosition(place);

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

        private void HandleJellyInPlace(int i)
        {
            LockAllJellys();
            Jellys[i].SetPlaced(true);

            Mouse.PutObjectInPlace(Jellys[i])
                .Done(TaskComplete);
        }

        public IPromise SetupAndShowMouse(EJellyForm jellyType)
        {
            LockAllJellys();

            return Mouse.Appear(jellyType)
                .Done(UnlockUnplacedJellys);
        }

        public IPromise RemoveMouse()
        {
            return Mouse.DisAppear();
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

        public void UnlockUnplacedJellys()
        {
            foreach (Jelly jelly in Jellys)
            {
                if (!jelly.IsPlaced()) jelly.Unlock();
            }
        }

        public void LockAllJellys()
        {
            foreach (Jelly jelly in Jellys)
            {
                jelly.Lock();
            }
        }

        public IPromise RemoveAllJellys()
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

        public void DestroyAllJellys()
        {
            foreach (Jelly jelly in Jellys)
            {
                Destroy(jelly.gameObject);
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