using System;
using System.Collections;
using System.Collections.Generic;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using KidGame.Collection;
using UnityEngine.UI;
using DaikonForge.Tween;
using KidGame.Localization;

namespace KidGame.UI
{
    public class CollectionStoreView : MonoBehaviour, ICollectionStoreView
    {
        public event Action<int> ThumbnailClicked;
        public event Action BackButtonClicked;


        public int Columns = 4;

        public float CellSize = 420f; // toDo: take it from item widget size
        public float CellSpace = 20f;

        public Button BackButton;

        public Transform BigImageRoot;
        public RectTransform ScrollContentRoot;
        public ScreenShading Shading;

        public StarChest Chest;

        private List<CollectionElement> Collection = new List<CollectionElement>();
        private Vector3 InitScrollPos;

        private Image BackButtonImage;

        private static ITimers Timers;
        private static IResourceManager ResourceManager;
        private static ISoundManager SoundManager;
        private static IGame Game;

        private int TimeToBackHighlight;

        private void Awake()
        {
            Game = CompositionRoot.GetGame();
            ResourceManager = CompositionRoot.GetResourceManager();
            SoundManager = CompositionRoot.GetSoundManager();
            Timers = CompositionRoot.GetTimers();

            SetupScrollContentRoot();

            PopulateCollectionItems();

            Chest.gameObject.SetActive(false);
            InitScrollPos = ScrollContentRoot.localPosition;

            BackButton.onClick.AddListener(() => BackButtonClicked());
            BackButtonImage = BackButton.GetComponent<Image>();

        }

        public IPromise ShowBigPicture(CollectionElement item)
        {
            TimeToBackHighlight = 0;

            if (!item.Unlocked())
            {
                DisableCollectionInteraction();

                item.HighlightLockedItem();
                var sound = LocalVoices.GetVoice(LocalVoices.VoiceKeys.CollectMoreStars);

                return SoundManager.PlayAndNotify(sound)
                    .Done(EnableCollectionInteraction);
            }

            item.StopHighlighting();

            Shading.Shade(true, 0.5f, 0.5f);
            StopAllCoroutines();

            return item.ShowBigImage(BigImageRoot)
                .Done(() => StartCoroutine(BackButtonHighlightWaiter()))
                .Done(Shading.UnshadeInstant);
        }

        private void DisableCollectionInteraction()
        {
            foreach(var item in Collection)
            {
                item.GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }

        private void EnableCollectionInteraction()
        {
            foreach (var item in Collection)
            {
                item.GetComponent<Button>().onClick.AddListener(() => ShowBigPicture(item));
            }
        }


        public void SwitchCollection(int collection)
        {
            throw new NotImplementedException();
        }

        private void SetupScrollContentRoot()
        {
            var total = Enum.GetValues(typeof(ECollection)).Length;
            var rows = Mathf.CeilToInt((float)total / Columns);

            var neededHeight = rows * (CellSize + CellSpace) + CellSpace;
            var currentWidth = ScrollContentRoot.rect.width;
            ScrollContentRoot.sizeDelta = new Vector2(currentWidth, neededHeight);
            ScrollContentRoot.localPosition = new Vector3(0, -neededHeight / 2, 0);

        }

        private void PopulateCollectionItems()
        {
            var total = Enum.GetValues(typeof(ECollection)).Length;

            int col = 0, row = 0;

            float verticalShift = ScrollContentRoot.rect.height / 2 - CellSize / 2; 
            float centerShift = 0;
            float totalShift;

            if (Columns % 2 == 0)
            {
                centerShift = -(CellSize + CellSpace)/2;
                totalShift = -(Columns / 2 - 1) * (CellSize + CellSpace) + centerShift;
            }
            else
            {
                totalShift = -(Columns - 1) / 2 * (CellSize + CellSpace);
            }

            for (int i=0; i<total; i++)
            {
                var item = ResourceManager.CreatePrefabInstance<ECollection, CollectionElement>((ECollection)i);

                var xPos = totalShift + col * (CellSize + CellSpace);
                var yPos = verticalShift - row * (CellSize + CellSpace);

                item.transform.SetParent(ScrollContentRoot);
                item.transform.localScale = Vector3.one;
                item.transform.localPosition = new Vector3(xPos, yPos, 0);
                item.transform.SetAsFirstSibling();

                item.SetupThumb();

                item.GetComponent<Button>().onClick.AddListener(() => ShowBigPicture(item));
                //

                Collection.Add(item);

                col++;

                if (col>=Columns)
                {
                    col = 0;
                    row++;
                }
            }
        }

        public IPromise ScrollToNextLocked()
        {
            var deferred = new Deferred();

            var row = Mathf.FloorToInt((float)Game.CurrentCardUnlocked / 4f);
            var rowHeight = CellSize + CellSpace;

            var targetY = row * rowHeight;
            var targetPos = InitScrollPos + new Vector3(0, targetY, 0);

            Debug.Log("Item: " + Game.CurrentCardUnlocked + " Row: " + row + " Pos: " + targetPos);

            ScrollContentRoot.transform.TweenPosition(true)
                .SetAutoCleanup(true)
                .SetDuration(0.7f)
                .SetEndValue(targetPos)
                .SetEasing(TweenEasingFunctions.Bounce)
               .OnCompleted((t) =>
               {

                   deferred.Resolve();
               })
            .Play();


            return deferred;
        }

        public void StartHighlightBackButton()
        {
            throw new NotImplementedException();
        }

        public void StopHighlightBackButton()
        {
            throw new NotImplementedException();
        }

        public IPromise UnlockNextItem()
        {
            var card = Collection[Game.CurrentCardUnlocked];

            var moveStars = Chest.MoveStarsToItem(card.transform);
            var showItem = card.AnimateUnlocking();

            card.SetAvailability(true);

            return Deferred.All(moveStars, showItem)
                        .Done(card.HighlightThumb);
        }

        public IPromise AddStars()
        {
            throw new NotImplementedException();
        }

        //--

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent, false);
        }

        public IPromise Hide()
        {
            StopAllCoroutines();

            return Shading.Shade(true, 0.5f)
                .Done(() => gameObject.SetActive(false));
        }

        public IPromise Show()
        {
            gameObject.SetActive(true);
            Shading.ShadeInstant();

            StartCoroutine(BackButtonHighlightWaiter());

            return Shading.Shade(false, 1.0f);
        }

        public IPromise ShowWithChest()
        {
            gameObject.SetActive(true);
            Shading.ShadeInstant();

            Chest.gameObject.SetActive(true);
            Chest.ResetPosition();
            Chest.SwitchState(false);
            Chest.AnimateClosed();

            return Shading.Shade(false, 1.0f)
                .Done(() => ScrollToNextLocked()) // Scroll grid to first unlocked
                .Then(Chest.MoveDown)
                .Then(() => Timers.Wait(0.5f))
                .Then(Chest.ErruptStars)
                .Then(UnlockNextItem)
                .Then(Chest.RemoveAfterErruption)
                .Then(PlayCongratSound);
                //.Done(AskForReview);
        }

        private IPromise PlayCongratSound()
        {
            Debug.Log("Unlocked cards = " + Game.CurrentCardUnlocked);
            var currentCard = Game.CurrentCardUnlocked;

            var sound = (currentCard > 0)? LocalVoices.GetVoice(LocalVoices.VoiceKeys.Hello): LocalVoices.GetVoice(LocalVoices.VoiceKeys.UnlockFirstDyno);

            return SoundManager.PlayAndNotify(sound);
        }

        private IEnumerator BackButtonHighlightWaiter()
        {

            while(TimeToBackHighlight < 25)
            {
                yield return new WaitForSecondsRealtime(1f);

                TimeToBackHighlight++;
                Debug.Log("Time to highlight: " + TimeToBackHighlight);
            }

            Debug.Log("TIME TO ACTION!");
        }

        /* REMOVE DUE TO POLITICAL DECISION
        void AskForReview()
        {
            if (!CheckIfNeedToAskReview()) return;

#if UNITY_IOS
            UnityEngine.iOS.Device.RequestStoreReview();
#elif UNITY_ANDROID
        //Not yet implemented
#endif
        }

        bool CheckIfNeedToAskReview()
        {
            return true;
        }
        */


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
