using UnityEngine;
using Orbox.Utils;


using KidGame.Lessons.RepairCrown;
using KidGame.Lessons.SortColoredGems;
using KidGame.Lessons.MakePendant;
using KidGame.Lessons.MakeMedal;
using KidGame.Lessons.Fruit01;
using KidGame.Lessons.Fruit02;
using KidGame.Lessons.Fruit03;
using KidGame.Lessons.Fruit04;
using KidGame.Lessons.Maze01;
using KidGame.Lessons.Maze02;
using KidGame.Lessons.Memory01;
using KidGame.Lessons.Memory02;
using KidGame.Lessons.Memory03;
using KidGame.Lessons.Candy01;
using KidGame.Lessons.Candy02;
using KidGame.Lessons.Candy03;
using KidGame.Lessons.Candy04;
using KidGame.Lessons.Jelly01;
using KidGame.Lessons.GemOddOneOut;
using KidGame.Lessons.CandyOddOneOut;
using KidGame.Lessons.Memory04;
using KidGame.Lessons.FruitOddOneOut;
using KidGame.Lessons.Jelly02;
using KidGame.Lessons.Jelly03;
using KidGame.Lessons.Jelly04;
using KidGame.Lessons.Jelly05;

namespace KidGame.UI
{
    public class TabletViewFactory : IViewFactory
    {

        private readonly IResourceManager ResourceManager;


        public TabletViewFactory(IResourceManager resourceManager)
        {
            ResourceManager = resourceManager;
        }

        public IRepairCrownView CreateRepairCrownView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.RepairCrownTablet);
            var view = go.GetComponent<IRepairCrownView>();

            return view;
        }

        public ICollectionStoreView CreateCollectionStoreView()
        {
            var go = ResourceManager.CreatePrefabInstance(EViews.CollectionStoreTablet); //toDo: Test/change to universal view if all things will be ok
            var view = go.GetComponent<ICollectionStoreView>();

            return view;
        }

        public IMenuView CreateMenuView()
        {
            var go = ResourceManager.CreatePrefabInstance(EViews.MenuSmartphone); //toDo: Temporary for draft menu
            var view = go.GetComponent<IMenuView>();

            return view;
        }

        public ISortColoredGemsView CreateSortColoredGemsView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.SortColoredGemsTablet);
            var view = go.GetComponent<ISortColoredGemsView>();

            return view;
        }

        public IMakePendantView CreateMakePendantView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.MakePendantTablet);
            var view = go.GetComponent<IMakePendantView>();

            return view;
        }

        public IMakeMedalView CreateMakeMedalView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.MakeMedalTablet);
            var view = go.GetComponent<IMakeMedalView>();

            return view;
        }

        public IFruit01View CreateFruit01View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Fruit01Tablet);
            var view = go.GetComponent<IFruit01View>();

            return view;
        }

        public IFruit02View CreateFruit02View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Fruit02Tablet);
            var view = go.GetComponent<IFruit02View>();

            return view;
        }

        public IFruit03View CreateFruit03View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Fruit03Tablet);
            var view = go.GetComponent<IFruit03View>();

            return view;
        }

        public IFruit04View CreateFruit04View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Fruit04Tablet);
            var view = go.GetComponent<IFruit04View>();

            return view;
        }

        public IMaze01View CreateMaze01View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Maze01Tablet);
            var view = go.GetComponent<IMaze01View>();

            return view;
        }

        public IMaze02View CreateMaze02View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Maze02Tablet);
            var view = go.GetComponent<IMaze02View>();

            return view;
        }

        public IMemory01View CreateMemory01View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Memory01Tablet); 
            var view = go.GetComponent<IMemory01View>();

            return view;
        }

        public IMemory02View CreateMemory02View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Memory02Tablet); 
            var view = go.GetComponent<IMemory02View>();

            return view;
        }

        public IMemory03View CreateMemory03View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Memory03Tablet);
            var view = go.GetComponent<IMemory03View>();

            return view;
        }

        public IMemory04View CreateMemory04View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Memory04Tablet);
            var view = go.GetComponent<IMemory04View>();

            return view;
        }

        public ICandy01View CreateCandy01View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Candy01Tablet);
            var view = go.GetComponent<ICandy01View>();

            return view;
        }

        public ICandy02View CreateCandy02View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Candy02Tablet);
            var view = go.GetComponent<ICandy02View>();

            return view;
        }

        public ICandy03View CreateCandy03View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Candy03Tablet);
            var view = go.GetComponent<ICandy03View>();

            return view;
        }

        public ICandy04View CreateCandy04View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Candy04Tablet);
            var view = go.GetComponent<ICandy04View>();

            return view;
        }

        public IJelly01View CreateJelly01View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Jelly01Tablet);
            var view = go.GetComponent<IJelly01View>();

            return view;
        }

        public IGemOddOneOutView CreateGemOddOneOutView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.GemOddOneOutTablet);
            var view = go.GetComponent<IGemOddOneOutView>();

            return view;
        }

        public ICandyOddOneOutView CreateCandyOddOneOutView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.CandyOddOneOutTablet);
            var view = go.GetComponent<ICandyOddOneOutView>();

            return view;
        }


        public IFruitOddOneOutView CreateFruitOddOneOutView()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.FruitOddOneOutTablet);
            var view = go.GetComponent<IFruitOddOneOutView>();

            return view;
        }

        public IJelly02View CreateJelly02View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Jelly02Tablet);
            var view = go.GetComponent<IJelly02View>();

            return view;
        }

        public IJelly03View CreateJelly03View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Jelly03Tablet);
            var view = go.GetComponent<IJelly03View>();

            return view;
        }

        public IJelly04View CreateJelly04View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Jelly04Tablet);
            var view = go.GetComponent<IJelly04View>();

            return view;
        }

        public IJelly05View CreateJelly05View()
        {
            var go = ResourceManager.CreatePrefabInstance(ELessonViews.Jelly05Tablet);
            var view = go.GetComponent<IJelly05View>();

            return view;
        }
    }
}