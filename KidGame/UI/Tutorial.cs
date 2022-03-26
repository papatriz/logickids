using System.Collections;
using System.Collections.Generic;
using DaikonForge.Tween;
using Orbox.Async;
using Orbox.Utils;
using UnityEngine;

namespace KidGame.UI
{
    public class Tutorial : MonoBehaviour
    {

        public GameObject HandPrefab;
        public GameObject ArrowPrefab;

        private Vector3 ArrowShift = new Vector3(-25f, 0, 0);
        private Vector3 HandShift = new Vector3(0, -20f, 0);

        private List<GameObject> Arrows;
        private  List<Tween<Vector3>> ArrowTweens;

        private Tween<Vector3> HintTween;

        private GameObject Hand;

        private const float HandSpeed = 2f; 

        public IPromise Show(List<Transform> Targets, Transform Answer, Transform ColliderTransform, Vector3 TargetPosTuning = default(Vector3), Vector3 ArrowPosTuning = default(Vector3), bool OnlyShowPointer=false)
        {
            var deferred = new Deferred();

            ArrowTweens = new List<Tween<Vector3>>();
            Arrows = new List<GameObject>();

            //foreach(Transform t in Targets)
            //{
            //    var initPos = t.position + new Vector3(-130f,0,0) + ArrowPosTuning;
            //    var shiftedPos = initPos + ArrowShift;

            //    var arrowGO = Instantiate(ArrowPrefab, this.transform);
            //    arrowGO.transform.position = initPos;

            //    var tween = arrowGO.transform.TweenPosition()
            //        .SetAutoCleanup(true)
            //        .SetStartValue(initPos)
            //        .SetEndValue(shiftedPos)
            //        .SetDuration(0.5f)
            //        .SetLoopCount(0)
            //        .SetLoopType(TweenLoopType.Pingpong);

            //    Arrows.Add(arrowGO);
            //    ArrowTweens.Add(tween);

            //    tween.Play();

            //}

            var answerOriginalPos = Answer.position;
            var answerOriginalRotation = Answer.rotation;

            var handInitPos = Answer.position + new Vector3(20f, -100f,0);
            if (OnlyShowPointer) handInitPos += ArrowPosTuning; // Костыль

            var handShiftedPos = handInitPos + HandShift;
            var handTargetPos = ColliderTransform.position + new Vector3(20f, -100f, 0) + TargetPosTuning;
            var handTargetShiftedPos = handTargetPos + HandShift;
            var answerPos = ColliderTransform.position + TargetPosTuning;

            Answer.SetAsLastSibling();
            Hand = Instantiate(HandPrefab, this.transform);
            Hand.transform.position = handInitPos;

            var handShakeAtEnd = Hand.transform.TweenPosition()
                .SetAutoCleanup(true)
                    .SetStartValue(handTargetPos)
                    .SetEndValue(handTargetShiftedPos)
                    .SetDuration(0.4f)
                    .SetLoopCount(2)
                    .SetLoopType(TweenLoopType.Pingpong)
                    .OnCompleted((t) =>
                    {
                        Answer.transform.position = answerOriginalPos;
                        Answer.transform.rotation = answerOriginalRotation;

                        StopAndDestroy(deferred);
                    });

            var handMoveToTarget = Hand.transform.TweenPosition()
                                        .SetAutoCleanup(true)
                                        .SetStartValue(handInitPos)
                                        .SetEndValue(handTargetPos)
                                        .SetDuration(HandSpeed)
                                        .SetEasing(TweenEasingFunctions.EaseInQuad)
                                        .OnCompleted((t) =>
                                        {
                                            handShakeAtEnd.Play();
                                        });

            var answerMove = Answer.transform.TweenPosition()
                            .SetAutoCleanup(true)
                            .SetStartValue(answerOriginalPos)
                            .SetEndValue(answerPos)
                            .SetDuration(HandSpeed)
                            .SetEasing(TweenEasingFunctions.EaseInQuad)
                            .OnCompleted((t) =>
                            {
                                Answer.transform.rotation = ColliderTransform.rotation;
                            });

            Hand.transform.TweenPosition()
                .SetAutoCleanup(true)
                    .SetStartValue(handInitPos)
                   // .SetDelay(1f)
                    .SetEndValue(handShiftedPos)
                    .SetDuration(0.4f)
                    .SetLoopCount(4)
                    .SetLoopType(TweenLoopType.Pingpong)
                    .OnCompleted((t) =>
                    {
                        if (!OnlyShowPointer)
                        {
                            handMoveToTarget.Play();
                            answerMove.Play();
                        }
                        else
                        {
                            StopAndDestroy(deferred);
                        }
                    })
                    .Play();

            return deferred;
        }

        public void HintShow(Transform t)
        {
            HintTween = t.TweenScale()
                    .SetAutoCleanup(true)
                    .SetDuration(0.5f)
                    .SetLoopType(TweenLoopType.Pingpong)
                    .SetLoopCount(0)
                    .SetEasing(TweenEasingFunctions.Bounce)
                    .SetStartValue(Vector3.one)
                    .SetEndValue(1.2f*Vector3.one);

            HintTween.Play();
        }

        public void HintStop()
        {
            HintTween.Stop();
        }

        private void StopAndDestroy(Deferred deferred)
        {
            Destroy(Hand, 0.1f);

            //for(int i=0; i<Arrows.Count; i++)
            //{
            //    ArrowTweens[i].Stop();
            //    Destroy(Arrows[i], 0.1f);
            //}

            deferred.Resolve();
        }

    }
}
