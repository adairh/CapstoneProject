using System;
using System.Collections.Generic;
using Lean.Common;
using Lean.Common.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	/// <summary>
	///     This component makes this GameObject selectable.
	///     If your game is 3D then make sure this GameObject or a child has a Collider component.
	///     If your game is 2D then make sure this GameObject or a child has a Collider2D component.
	///     If your game is UI based then make sure this GameObject or a child has a graphic with "Raycast Target" enabled.
	///     To then select it, you can add the LeanSelect and LeanFingerTap components to your scene. You can then link up the
	///     LeanFingerTap.OnTap event to LeanSelect.SelectScreenPosition.
	/// </summary>
	[ExecuteInEditMode]
    [DisallowMultipleComponent]
    [HelpURL(LeanTouch.HelpUrlPrefix + "LeanSelectableByFinger")]
    [AddComponentMenu(LeanTouch.ComponentPathPrefix + "Selectable By Finger")]
    public class LeanSelectableByFinger : LeanSelectable
    {
        public enum UseType
        {
            AllFingers,
            OnlySelectingFingers,
            IgnoreSelectingFingers
        }

        [SerializeField] private UseType use;
        [SerializeField] private LeanFingerEvent onSelectedFinger;
        [SerializeField] private LeanFingerEvent onSelectedFingerUp;
        [SerializeField] private LeanSelectFingerEvent onSelectedSelectFinger;
        [SerializeField] private LeanSelectFingerEvent onSelectedSelectFingerUp;

        // The fingers that were used to select this GameObject
        // If a finger goes up then it will be removed from this list

        /// <summary>This allows you to control which fingers will be used by components that require this selectable.</summary>
        public UseType Use
        {
            set => use = value;
            get => use;
        }

        /// <summary>This event is called when selection begins (finger = the finger that selected this).</summary>
        public LeanFingerEvent OnSelectedFinger
        {
            get
            {
                if (onSelectedFinger == null) onSelectedFinger = new LeanFingerEvent();
                return onSelectedFinger;
            }
        }

        /// <summary>
        ///     This event is called when selection begins (selectByFinger = component that selected this, finger = the finger
        ///     that selected this).
        /// </summary>
        public LeanFingerEvent OnSelectedFingerUp
        {
            get
            {
                if (onSelectedFingerUp == null) onSelectedFingerUp = new LeanFingerEvent();
                return onSelectedFingerUp;
            }
        }

        /// <summary>
        ///     This event is called when selection begins (selectByFinger = component that selected this, finger = the finger
        ///     that selected this).
        /// </summary>
        public LeanSelectFingerEvent OnSelectedSelectFinger
        {
            get
            {
                if (onSelectedSelectFinger == null) onSelectedSelectFinger = new LeanSelectFingerEvent();
                return onSelectedSelectFinger;
            }
        }

        /// <summary>This event is called when selection begins (finger = the finger that selected this).</summary>
        public LeanSelectFingerEvent OnSelectedSelectFingerUp
        {
            get
            {
                if (onSelectedSelectFingerUp == null) onSelectedSelectFingerUp = new LeanSelectFingerEvent();
                return onSelectedSelectFingerUp;
            }
        }

        /// <summary>
        ///     This tells you the first or earliest still active finger that initiated selection of this object.
        ///     NOTE: If the selecting finger went up then this may return null.
        /// </summary>
        public LeanFinger SelectingFinger
        {
            get
            {
                if (SelectingPairs.Count > 0) return SelectingPairs[0].Finger;

                return null;
            }
        }

        /// <summary>This tells you every currently active finger that selected this object.</summary>
        [field: NonSerialized]
        public List<SelectedPair> SelectingPairs { get; } = new();

        private bool AnyFingersSet
        {
            get
            {
                for (var i = SelectingPairs.Count - 1; i >= 0; i--)
                    if (SelectingPairs[i].Finger.Set)
                        return true;

                return false;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            LeanTouch.OnFingerUp += HandleFingerUp;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            LeanTouch.OnFingerUp -= HandleFingerUp;
        }

        public static event Action<LeanSelectByFinger, LeanSelectableByFinger, LeanFinger> OnAnySelectedFinger;

        public void SelectSelf(LeanFinger finger)
        {
            if (SelfSelected == false)
            {
                SelfSelected = true;

                if (finger.Up == false) SelectingPairs.Add(new SelectedPair { Finger = finger, Select = null });

                if (onSelectedFinger != null) onSelectedFinger.Invoke(finger);

                if (finger.Up && onSelectedFingerUp != null) onSelectedFingerUp.Invoke(finger);

                if (onSelectedSelectFinger != null) onSelectedSelectFinger.Invoke(null, finger);

                if (finger.Up && onSelectedSelectFingerUp != null) onSelectedSelectFingerUp.Invoke(null, finger);
            }
        }

        /// <summary>
        ///     If requiredSelectable is set and not selected, the fingers list will be empty. If selected then the fingers
        ///     list will only contain the selecting finger.
        /// </summary>
        public static List<LeanFinger> GetFingers(bool ignoreIfStartedOverGui, bool ignoreIfOverGui,
            int requiredFingerCount = 0, LeanSelectable requiredSelectable = null)
        {
            var fingers = LeanTouch.GetFingers(ignoreIfStartedOverGui, ignoreIfOverGui, requiredFingerCount);

            if (requiredSelectable != null)
            {
                if (requiredSelectable.IsSelected)
                {
                    var requiredSelectableByFinger = requiredSelectable as LeanSelectableByFinger;

                    if (requiredSelectableByFinger != null)
                        switch (requiredSelectableByFinger.use)
                        {
                            case UseType.AllFingers:
                            {
                            }
                                break;

                            case UseType.OnlySelectingFingers:
                            {
                                fingers.Clear();

                                foreach (var pair in requiredSelectableByFinger.SelectingPairs)
                                    fingers.Add(pair.Finger);
                            }
                                break;

                            case UseType.IgnoreSelectingFingers:
                            {
                                foreach (var selectingFinger in requiredSelectableByFinger.SelectingPairs)
                                    fingers.Remove(selectingFinger.Finger);
                            }
                                break;
                        }

                    if (requiredFingerCount > 0 && fingers.Count != requiredFingerCount) fingers.Clear();
                }
                else
                {
                    fingers.Clear();
                }
            }

            return fingers;
        }

        /// <summary>If the specified finger selected an object, this will return the first one.</summary>
        public static LeanSelectableByFinger FindSelectable(LeanFinger finger)
        {
            foreach (var selectable in Instances)
            {
                var selectableByFinger = selectable as LeanSelectableByFinger;

                if (selectableByFinger != null && selectableByFinger.IsSelectedBy(finger)) return selectableByFinger;
            }

            return null;
        }

        /// <summary>This tells you if the current selectable was selected by the specified finger.</summary>
        public bool IsSelectedBy(LeanFinger finger)
        {
            for (var i = SelectingPairs.Count - 1; i >= 0; i--)
                if (SelectingPairs[i].Finger == finger)
                    return true;

            return false;
        }

        public static void InvokeAnySelectedFinger(LeanSelectByFinger select, LeanSelectableByFinger selectable,
            LeanFinger finger)
        {
            if (OnAnySelectedFinger != null) OnAnySelectedFinger.Invoke(select, selectable, finger);
        }

        private void HandleFingerUp(LeanFinger finger)
        {
            for (var i = 0; i < SelectingPairs.Count; i++)
            {
                var pair = SelectingPairs[i];

                if (pair.Finger == finger)
                {
                    SelectingPairs.RemoveAt(i);

                    if (onSelectedFingerUp != null) onSelectedFingerUp.Invoke(finger);

                    if (onSelectedSelectFingerUp != null) onSelectedSelectFingerUp.Invoke(pair.Select, finger);
                }
            }
        }

        public struct SelectedPair
        {
            public LeanSelectByFinger Select;
            public LeanFinger Finger;
        }

        [Serializable]
        public class LeanFingerEvent : UnityEvent<LeanFinger>
        {
        }

        [Serializable]
        public class LeanSelectFingerEvent : UnityEvent<LeanSelectByFinger, LeanFinger>
        {
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
    using TARGET = LeanSelectableByFinger;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanSelectableByFinger_Editor : LeanSelectable_Editor
    {
        [NonSerialized] private TARGET tgt;
        [NonSerialized] private TARGET[] tgts;

        protected override void OnInspector()
        {
            GetTargets(out tgt, out tgts);

            Draw("use",
                "This allows you to control which fingers will be used by components that require this selectable.");

            base.OnInspector();
        }

        protected override void DrawEvents(bool showUnusedEvents)
        {
            base.DrawEvents(showUnusedEvents);

            if (showUnusedEvents || Any(tgts, t => t.OnSelectedFinger.GetPersistentEventCount() > 0))
                Draw("onSelectedFinger");

            if (showUnusedEvents || Any(tgts, t => t.OnSelectedFingerUp.GetPersistentEventCount() > 0))
                Draw("onSelectedFingerUp");

            if (showUnusedEvents || Any(tgts, t => t.OnSelectedSelectFinger.GetPersistentEventCount() > 0))
                Draw("onSelectedSelectFinger");

            if (showUnusedEvents || Any(tgts, t => t.OnSelectedSelectFingerUp.GetPersistentEventCount() > 0))
                Draw("onSelectedSelectFingerUp");
        }
    }
}
#endif