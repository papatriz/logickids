using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using KidGame.Effects;
using KidGame.UI;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.SortColoredGems
{
    public class SortColoredGemsView : MonoBehaviour, ISortColoredGemsView
    {
        public event Action BoxIsFilled;
        public event Action BackButtonPressed;
        public event Action MistakeHappened;

        public Box Box;

        public Transform GemsParentTransform;
        public GameObject GemPrefab;

        public IconProgressBar ProgressBar;

        public Transform[] GemPlaceHolders;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform WinDialogRoot;
        public Transform FireworkRoot;

        private WinDialog WinDialog;

        private Vector2[] GemPlaces;

        private List<Gem> Gems = new List<Gem>();

        private static ITimers Timers;
        private ISoundManager SoundManager;
        private static IResourceManager ResourceManager;
        private static IGame Game;

        private UIParticle Firework;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            Timers = CompositionRoot.GetTimers();
            ResourceManager = CompositionRoot.GetResourceManager();
            SoundManager = CompositionRoot.GetSoundManager();

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            var gemsCount = GemPlaceHolders.Length;
            GemPlaces = new Vector2[gemsCount];

            for (int i=0; i< gemsCount; i++)
            {
                GemPlaces[i] = GemPlaceHolders[i].position;
                GemPlaceHolders[i].gameObject.SetActive(false);
            }

            BackButton.onClick.AddListener(() => BackButtonPressed());
            Box.BoxIsFull += HandleFilledBox;
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

        void HandleFilledBox()
        {
            Debug.Log("Box is filled");
            BoxIsFilled();
        }

        public IPromise ShowAllGems(EGemType properColor, EGemType fakeColor)
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = GemPlaces.ToList();

            Gems.Clear();

            for (int i=0; i<GemPlaces.Length; i++)
            {
                var gemGO = Instantiate(GemPrefab, GemsParentTransform);
                gemGO.SetActive(false);

                var gem = gemGO.GetComponent<Gem>();
                gem.SetIndex(i);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                var color = (i < 3) ? properColor : fakeColor;
                gem.SetType(color);

                gem.SetTarget(Box.GetCurrentTargetCollider(), properColor);
                gem.SetStartPosition(place);

                gem.SetPlaced(false);
                gem.TargetReached += HandleGemInBox;
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

            var condition = Box.GetConditionTransform();
            var mainTarget = Box.GetMiddlePlaceTransform();

            arrowTargetsList.Add(condition);

            var handTarget = Gems[0].transform;

            return Tutorial.Show(arrowTargetsList, handTarget, mainTarget, Vector3.zero, new Vector3(50f,0,0))
                .Done(() => BackButton.gameObject.SetActive(true)); // TEMPORARY
        }

        public IPromise RemoveAllGems()
        {
            var deferred = new Deferred();

            foreach (Gem gem in Gems)
            {
                if (gem.IsPlaced())
                {
                    Destroy(gem.gameObject);
                }
                else
                {
                    deferred = (Deferred)gem.DisAppear()
                                       .Done(() => Destroy(gem.gameObject));
                }
            }

            return deferred;
        }

        private void HandleGemInBox(int i)
        {
            LockAllGems();
            Gems[i].SetPlaced(true);

            Box.PutGemInPlace(Gems[i])
                .Done(UnlockUnsortedGems);
        }

        public IPromise RemoveBox()
        {
            return Box.DisAppear();
        }


        public IPromise SetupAndShowBox(EGemType type)
        {
            Box.SetCurrentType(type);

            return Box.Appear();
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

        public void UnlockUnsortedGems()
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