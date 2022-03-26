using UnityEngine;

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


    public interface IViewFactory
    {
        IMenuView CreateMenuView();
        ICollectionStoreView CreateCollectionStoreView();

        IRepairCrownView CreateRepairCrownView();
        ISortColoredGemsView CreateSortColoredGemsView();
        IMakePendantView CreateMakePendantView();
        IMakeMedalView CreateMakeMedalView();
        IFruit01View CreateFruit01View();
        IFruit02View CreateFruit02View();
        IFruit03View CreateFruit03View();
        IFruit04View CreateFruit04View();
        IMaze01View CreateMaze01View();
        IMaze02View CreateMaze02View();
        IMemory01View CreateMemory01View();
        IMemory02View CreateMemory02View();
        IMemory03View CreateMemory03View();
        IMemory04View CreateMemory04View();
        ICandy01View CreateCandy01View();
        ICandy02View CreateCandy02View();
        ICandy03View CreateCandy03View();
        ICandy04View CreateCandy04View();

        IJelly01View CreateJelly01View();
        IGemOddOneOutView CreateGemOddOneOutView();
        ICandyOddOneOutView CreateCandyOddOneOutView();
        IFruitOddOneOutView CreateFruitOddOneOutView();
        IJelly02View CreateJelly02View();
        IJelly03View CreateJelly03View();
        IJelly04View CreateJelly04View();
        IJelly05View CreateJelly05View();

    }
}
