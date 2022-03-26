using System;
using System.Collections;
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

namespace KidGame.Lessons.RepairCrown
{
    public class RepairCrownView : MonoBehaviour, IRepairCrownView
    {
        public event Action<int, EGemType> GemInPlace;
        public event Action MistakeHappened;
        public event Action BackButtonPressed;


        public Crown Crown;

        public Transform GemsParentTransform;
        public GameObject[] GemPrefabs;

        public IconProgressBar ProgressBar;

        public Transform[] GemsPlaceHolders;

        public Button BackButton;
        public Tutorial Tutorial;

        public ScreenShading Shading;

        public Transform FireworkRoot;
        public Transform WinDialogRoot;

        private WinDialog WinDialog;

        private Vector2[] GemPlaces = new Vector2[4];

        private List<Gem> Gems = new List<Gem>();

        private Dictionary<EGemType, GameObject> GemPrefabsDict = new Dictionary<EGemType, GameObject>();
        private Gem GemToRemove;

        private UIParticle Firework;

        private static ITimers Timers;
        private static IResourceManager ResourceManager;
        private static ISoundManager SoundManager;
        private static IGame Game;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            ResourceManager = CompositionRoot.GetResourceManager();
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            WinDialog = (Game.Layout == Layouts.Tablet) ? ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogTablet) : ResourceManager.CreatePrefabInstance<EWidgets, WinDialog>(EWidgets.WinDialogSmartphone);
            WinDialog.transform.SetParent(WinDialogRoot, false);
            WinDialog.gameObject.SetActive(false);

            GemPlaces[0] = GemsPlaceHolders[0].localPosition;
            GemPlaces[1] = GemsPlaceHolders[1].localPosition;
            GemPlaces[2] = GemsPlaceHolders[2].localPosition;
            GemPlaces[3] = GemsPlaceHolders[3].localPosition;

            GemsPlaceHolders[0].gameObject.SetActive(false);
            GemsPlaceHolders[1].gameObject.SetActive(false);
            GemsPlaceHolders[2].gameObject.SetActive(false);
            GemsPlaceHolders[3].gameObject.SetActive(false);

            BackButton.onClick.AddListener(() => BackButtonPressed());

            for (int i = 0; i < GemPrefabs.Length; i++)
            {
                var gemType = GemPrefabs[i].GetComponent<Gem>().Type;
                GemPrefabsDict.Add(gemType, GemPrefabs[i]);
            }

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
            if (Firework!=null)
            {
                Firework.Stop();
                Firework.gameObject.SetActive(false);
                Destroy(Firework.gameObject);
            }
        }

        public IPromise ShowAllGems(List<EGemType> hand)
        {
            var deferred = new Deferred();
            float wait = 0f;

            List<Vector2> PlacesHeap = GemPlaces.ToList();

            Gems.Clear();

            var i = 0;

            foreach(EGemType igem in hand)
            {
                var gemGO = Instantiate(GemPrefabsDict[igem], GemsParentTransform);

                gemGO.SetActive(false);

                var gem = gemGO.GetComponent<Gem>();

                var s = i;
                gem.SetIndex(s);

                var pos = Random.Range(0, PlacesHeap.Count);
                var place = PlacesHeap[pos];
                PlacesHeap.Remove(place);

                gem.SetStartPosition(place); 


                gem.TargetReached += HandleRightGemInPlace;
                gem.MistakeMade += MistakeHappened;

                Gems.Add(gem);

                i++;
            }

            foreach (Gem gem in Gems)
            {
                wait += 0.15f; // 0.1f

                deferred = (Deferred)Timers.Wait(wait)
                                           .Then(gem.Appear);
            }

            return deferred;
        }

        public IPromise ShowTutorial()
        {
            // TEMPORARY
            BackButton.gameObject.SetActive(false);
            //

            var arrowTargetsList = new List<Transform>();

            var mainTarget = Crown.GetTargetCollider().transform;
            arrowTargetsList.Add(mainTarget);

            var handTarget = Gems[0].transform;

            return Tutorial.Show(arrowTargetsList, handTarget, mainTarget, Vector3.zero, new Vector3(0f, 20f, 0))
                .Done(() => BackButton.gameObject.SetActive(true)); // TEMPORARY
        }

        public IPromise HighlightAnswer()
        {
            return new Deferred();
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

        public void ResetProgress()
        {
            ProgressBar.Reset();
        }

        public IPromise CheckpointReached()
        {
            return ProgressBar.ActivateNext();
        }

        public IPromise TranslateToNextCheckpoint()
        {
            return ProgressBar.TranslateToNext();
        }


        public void SetTargetTypeForGems()
        {
            var targetCollider = Crown.GetTargetCollider();
            var gemType = Crown.GetCurrentType();

            foreach (Gem gem in Gems)
            {
                gem.SetTarget(targetCollider, gemType);
            }
        }

        public void UnlockAllGems()
        {
            foreach (Gem gem in Gems)
            {
                gem.Unlock();
            }
        }

        public void LockAllGems()
        {
            foreach (Gem gem in Gems)
            {
                gem.Lock();
            }
        }

        public int GetTargetTypeCount()
        {
            return Crown.GetTargetsCount();
        }

        public IPromise SetupCrown(EGemType gemType)
        {
            return Crown.Appear(gemType);
        }

        public IPromise RemoveCrown()
        {
            return Crown.DisAppear()
                        .Done(() =>
                        {
                          if (GemToRemove != null)  Destroy(GemToRemove.gameObject);
                        });
        }

        public IPromise InsertGemToCrown(int i)
        {
            GemToRemove = Gems[i];

            return Gems[i].InsertInCrown()
                .Done(() => Gems.RemoveAt(i));
        }


        private void HandleRightGemInPlace(int i)
        {
            
            var targetPos = Crown.GetTargetCollider().transform.position;
            var targetRot = Crown.GetTargetCollider().transform.rotation;

            Gems[i].transform.position = targetPos;
            Gems[i].transform.rotation = targetRot;

            Gems[i].AttachToCrown(Crown.transform);

            Gems[i].TargetReached -= HandleRightGemInPlace;

            GemInPlace(i, Gems[i].Type);
        }


        public IPromise RemoveAllGems() 
        {
            var deferred = new Deferred();

            foreach (Gem gem in Gems)
            {
               deferred = (Deferred)gem.DisAppear()
                                      .Done(() => Destroy(gem.gameObject));
            }

            return deferred;
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
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
    }
}