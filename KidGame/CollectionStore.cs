using System;

using KidGame.UI;
using Orbox.Utils;
using UnityEngine;

namespace KidGame
{
    public class CollectionStore 
    {
        public event Action Ended;

        private ICollectionStoreView View;

        private IGame Game;
        private IResourceManager Resources;

        public CollectionStore(Transform parent)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateCollectionStoreView();
            View.SetParent(parent);


            View.BackButtonClicked += Exit;

            View.HideInstant();

        }

        public void Start()
        {
            View.Show();
        }

        public void StartWithChest()
        {
            View.ShowWithChest()
                .Done(() => Game.CurrentCardUnlocked++);
        }

        public void Exit()
        {
            View.Hide()
                    .Done(Ended);
        }
    }
}