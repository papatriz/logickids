using System;
using System.Collections.Generic;
using System.Linq;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KidGame.Lessons.CandyOddOneOut
{
    public enum EForm
    {
        Round,
        Square,
        Chupa,
        Rectangle,
        Truffel
    }

    public enum EColor
    {
        Flat,
        Stars,
        Circles,
        Stripes
    }


    public struct CandyType
    {
        public readonly int ID;

        private readonly EForm Form;
        private readonly EColor Color;

        public CandyType(EForm form, EColor color)
        {
            Form = form;
            Color = color;
            ID = (int)form * 4 + (int)color;
        }

        public override bool Equals(object obj)
        {
            return obj is CandyType type &&
                   ID == type.ID;
        }
    }

    public class CandyOddOneOutLesson : ILesson
    {
        public event Action Ended;
        public event Action EndedWithReward;

        private ICandyOddOneOutView View;

        private IGame Game;
        private IResourceManager Resources;

        private CandyType RightAnswer;

        private int RightAnswerCount;
        private int MistakeCount;

        private bool ChestIsFilled;

        private ELessons LessonID;

        public CandyOddOneOutLesson(Transform parent, ELessons lesson)
        {
            Game = CompositionRoot.GetGame();
            Resources = CompositionRoot.GetResourceManager();

            LessonID = lesson;

            var factory = CompositionRoot.GetViewFactory();
            View = factory.CreateCandyOddOneOutView();
            View.SetParent(parent);

            View.ChoosedObject += (obj) => HandleChoiceMade(obj);

            View.BackButtonPressed += BreakLesson;

            View.HideInstant();

        }

        public void Start()
        {
            RightAnswerCount = 0;
            View.Show();
            View.ResetProgress();
            SetScene();
        }


        private List<CandyType> GetHand()
        {
            var hand = new List<CandyType>();

            var heapForms = Enum.GetValues(typeof(EForm)).Cast<EForm>().ToList();
            var heapColors = Enum.GetValues(typeof(EColor)).Cast<EColor>().ToList();

            var property = Random.Range(0, 2);

            // property = 0; // toDo: FOR TEST< HAVE TO TRY BEFORE POPULATE

            switch (property)
            {
                case 0: // Main property will be Form

                    int commonFormIndex = Random.Range(0, heapForms.Count);
                    var commonForm = heapForms[commonFormIndex];

                    int[] colorsCount = new int[heapColors.Count];

                    for (int i = 0; i < 4; i++)
                    {
                        int colorIndex = Random.Range(0, heapColors.Count);
                        EColor color = heapColors[colorIndex];

                        CandyType handElement = new CandyType(commonForm, color);

                        hand.Add(handElement);

                        colorsCount[(int)color]++;
                        if (colorsCount[(int)color] > 2) // Non-main properties should not occur in more than three elements
                        {
                            heapColors.Remove(color);
                        }

                    }

                    heapForms.Remove(commonForm);
                    int uncommonFormIndex = Random.Range(0, heapForms.Count);
                    var uncommonForm = heapForms[uncommonFormIndex];

                    int lastColorIndex = Random.Range(0, heapColors.Count);
                    EColor lastcolor = heapColors[lastColorIndex];

                    CandyType uncommonHandElement = new CandyType(uncommonForm, lastcolor);

                    hand.Add(uncommonHandElement);

                    break;

                case 1: // Main property will be Color

                    int commonColorIndex = Random.Range(0, heapColors.Count);
                    var commonColor = heapColors[commonColorIndex];

                    int[] formsCount = new int[heapForms.Count];

                    for (int i = 0; i < 4; i++)
                    {
                        int formIndex = Random.Range(0, heapForms.Count);
                        EForm form = heapForms[formIndex];

                        CandyType handElement = new CandyType(form, commonColor);

                        hand.Add(handElement);

                        formsCount[(int)form]++;
                        if (formsCount[(int)form] > 2) // Non-main properties should not occur in more than three elements
                        {
                            heapForms.Remove(form);
                        }

                    }

                    heapColors.Remove(commonColor);
                    int uncommonColorIndex = Random.Range(0, heapColors.Count);
                    var uncommonColor = heapColors[uncommonColorIndex];

                    int lastFormIndex = Random.Range(0, heapForms.Count);
                    EForm lastform = heapForms[lastFormIndex];

                    uncommonHandElement = new CandyType(lastform, uncommonColor);

                    hand.Add(uncommonHandElement);

                    break;

                default:

                    break;
            }

            return hand;
        }

        private void HandleChoiceMade(CandyType gem)
        {
            View.SetObjectsInteractable(false);
            View.ChangeBackButtonState(false);

            if (gem.Equals(RightAnswer))
            {
                ProcessRightAnswer(gem);
            }
            else
            {
                ProcessWrongAnswer(gem);
            }
        }


        private void ProcessWrongAnswer(CandyType jelly)
        {
            MistakeCount++;

            View.ProccessAnswer(jelly, false)
                .Then(() => View.RemoveObjects())
                .Done(SetScene);
        }


        private void ProcessRightAnswer(CandyType jelly)
        {
            //toDo: make some cool animation in addition to sound
            View.ProccessAnswer(jelly, true)
                .Then(View.CheckpointReached)
                .Then(View.RemoveObjects)
                .Done(() =>
                {
                    RightAnswerCount++;


                    if (RightAnswerCount == 1) Game.AddLessonStartCount(LessonID); // Add lesson start with at least one right answer given count

                    if (RightAnswerCount < 7)
                    {
                        View.TranslateToNextCheckpoint();
                        SetScene();
                    }
                    else
                    {
                        View.ShowWinDialog(Game.StarsForGivenMistakes(MistakeCount))
                             .Then(WinRoutine);
                    }
                });
        }


        private void SetScene()
        {
            var hand = GetHand();

            RightAnswer = hand[4];

            Debug.Log(RightAnswer);

            View.SetObjectsInteractable(false);
            View.ChangeBackButtonState(false);

            View.SetupAndShowObjects(hand)
                .Then(ShowTutorialIfNeed)
                .Done(() => View.SetObjectsInteractable(true))
                .Done(() => View.ChangeBackButtonState(true));

        }

        private IPromise ShowTutorialIfNeed()
        {
            if ((RightAnswerCount == 0) && (Game.GetLessonStartCount(LessonID) == 0)) // Show Tutorial only if this is first start of lesson and this is first question
            {
                return View.ShowTutorial();
            }
            else
            {
                return new Deferred().Resolve();
            }

        }

        private void BreakLesson()
        {
            View.ChangeBackButtonState(false);

            var removeAll = View.RemoveObjects();

            removeAll
                .Done(() =>
                {
                    View.Hide();
                    Ended();
                });
        }


        // ----------------- WIN ROUTINE METHODS ----------------
        private IPromise WinRoutine()
        {
            IPromise routine;

            AddEarnedStars();

            if (ChestIsFilled)
            {
                View.CloseAndAnimateChest();

                routine = View.WaitForWinDialogClose()
                    .Then(View.PrepareForTransit)
                    .Done(EndedWithReward)
                    .Done(View.HideInstant);
            }
            else
            {
                routine = View.WaitForWinDialogClose()
                        .Then(View.HideWinDialog)
                        .Then(View.Hide)
                        .Done(Ended);
            }

            return routine;
        }

        private void AddEarnedStars()
        {
            var amount = Game.StarsForGivenMistakes(MistakeCount);

            if ((Game.StarsInChest + amount) >= Game.StarsToNextUnlock)
            {
                ChestIsFilled = true;

                var stepsToUnlock = Game.StarsToNextUnlock - Game.StarsInChest;
                var overAmount = amount - stepsToUnlock;
                Game.StarsInChest = overAmount;
            }
            else
            {
                ChestIsFilled = false;
                Game.StarsInChest += amount;
            }
        }
        // ------------- END OF WIN ROUTINE METHODS ----------------
    }
}