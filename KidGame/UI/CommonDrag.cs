using System;
using System.Collections;
using System.Collections.Generic;
using Orbox.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KidGame.UI
{
    public class CommonDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public event Action<int> TargetReached;
        public event Action MistakeMade;

        private Collider2D Collider;
        private Collider2D MatchedCollider;

        protected Enum MatchType;

        private int Index; 

        private Vector2 StartPosition;

        private bool Locked;
        private bool MatchFound;
        private bool MistakeHappens;

        private Dictionary<Collider2D, Enum> Targets = new Dictionary<Collider2D, Enum>();

        private ISoundManager SoundManager;

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider2D>();
            SoundManager = CompositionRoot.GetSoundManager();

            Lock();
        }

        public void Lock()
        {
            Locked = true;
        }

        public Collider2D GetMatchedCollider()
        {
            return MatchedCollider;         
        }

        public void Unlock()
        {
            Locked = false;
        }

        public void SetStartPosition(Vector2 pos)
        {
            StartPosition = pos;
        }

        public void SetIndex(int i)
        {
            Index = i;
        }

        public Vector2 GetStartPosition()
        {
            return StartPosition;
        }

        public void SetTarget(Collider2D col, Enum type) // for single target purposes and for back compatibility
        {
            Targets.Add(col, type);
        }

        public void SetMultiTarget(Dictionary<Collider2D, Enum> targets)
        {
            Targets = targets;
        }

        public void SetType(Enum type)
        {
            MatchType = type;   //Convert.ToInt32(type);
        }



        public void OnDrag(PointerEventData eventData)
        {
            if (!Locked)
                transform.position = eventData.pointerCurrentRaycast.screenPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Locked)
            {
                transform.SetAsLastSibling();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Locked)
            {
                List<Collider2D> colliders = new List<Collider2D>();
                ContactFilter2D contactFilter = new ContactFilter2D();

                bool needReturn = true;
                MatchFound = false;
                MistakeHappens = false;

                var numCollides = Collider.OverlapCollider(contactFilter, colliders);

                if (numCollides > 0)
                {
                    foreach (Collider2D col in colliders)
                    {
                        if (Targets.ContainsKey(col))
                        {
                            if (MatchType.Equals(Targets[col]))
                            {
                                MatchFound = true;
                                MatchedCollider = col;
                            }
                            else
                            {
                                MistakeHappens = true;
                            }

                        }
                    }

                    if (MatchFound)
                    {
                        needReturn = false;
                        Lock();

                        TargetReached(Index);
                    }
                    else
                        if (MistakeHappens)
                        {
                            SoundManager.Play(Sounds.ECommon.UhUh);
                            MistakeMade();
                        }
                }

                if (needReturn)
                {
                    transform.localPosition = StartPosition;
                }
            }

        }


    }
}