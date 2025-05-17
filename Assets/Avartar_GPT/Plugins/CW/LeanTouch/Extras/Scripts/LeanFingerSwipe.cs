using Lean.Common;
using UnityEditor;
using UnityEngine;

namespace Lean.Touch
{
	/// <summary>
	///     This component fires events if a finger has swiped across the screen.
	///     A swipe is defined as a touch that began and ended within the LeanTouch.TapThreshold time, and moved more than the
	///     LeanTouch.SwipeThreshold distance.
	/// </summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanFingerSwipe")]
    [AddComponentMenu(LeanTouch.ComponentPathPrefix + "Finger Swipe")]
    public class LeanFingerSwipe : LeanSwipeBase
    {
        [SerializeField] private bool ignoreStartedOverGui = true;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private LeanSelectable requiredSelectable;

        /// <summary>Ignore fingers with StartedOverGui?</summary>
        public bool IgnoreStartedOverGui
        {
            set => ignoreStartedOverGui = value;
            get => ignoreStartedOverGui;
        }

        /// <summary>Ignore fingers with OverGui?</summary>
        public bool IgnoreIsOverGui
        {
            set => ignoreIsOverGui = value;
            get => ignoreIsOverGui;
        }

        /// <summary>If the specified object is set and isn't selected, then this component will do nothing.</summary>
        public LeanSelectable RequiredSelectable
        {
            set => requiredSelectable = value;
            get => requiredSelectable;
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            requiredSelectable = GetComponentInParent<LeanSelectable>();
        }
#endif

        protected virtual void Start()
        {
            if (requiredSelectable == null) requiredSelectable = GetComponentInParent<LeanSelectable>();
        }

        protected virtual void OnEnable()
        {
            LeanTouch.OnFingerSwipe += HandleFingerSwipe;
        }

        protected virtual void OnDisable()
        {
            LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
        }

        private void HandleFingerSwipe(LeanFinger finger)
        {
            if (ignoreStartedOverGui && finger.StartedOverGui) return;

            if (ignoreIsOverGui && finger.IsOverGui) return;

            if (requiredSelectable != null && requiredSelectable.IsSelected == false) return;

            HandleFingerSwipe(finger, finger.StartScreenPosition, finger.ScreenPosition);
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
    using TARGET = LeanFingerSwipe;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanFingerSwipe_Editor : LeanSwipeBase_Editor
    {
        protected override void OnInspector()
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            Draw("ignoreStartedOverGui", "Ignore fingers with StartedOverGui?");
            Draw("ignoreIsOverGui", "Ignore fingers with OverGui?");
            Draw("requiredSelectable",
                "If the specified object is set and isn't selected, then this component will do nothing.");

            base.OnInspector();
        }
    }
}
#endif