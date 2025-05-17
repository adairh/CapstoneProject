using System;
using CW.Common;
using Lean.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
	/// <summary>
	///     This component fires events on the first frame where a finger has been touching the screen for more than
	///     <b>TapThreshold</b> seconds, and is therefore no longer eligible for tap or swipe events.
	/// </summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanFingerOld")]
    [AddComponentMenu(LeanTouch.ComponentPathPrefix + "Finger Old")]
    public class LeanFingerOld : MonoBehaviour
    {
        [SerializeField] private bool ignoreStartedOverGui = true;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private LeanSelectable requiredSelectable;
        [SerializeField] private LeanFingerEvent onFinger;

        /// <summary>
        ///     The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more
        ///     information.
        /// </summary>
        public LeanScreenDepth ScreenDepth = new(LeanScreenDepth.ConversionType.DepthIntercept);

        [SerializeField] private Vector3Event onWorld;
        [SerializeField] private Vector2Event onScreen;

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

        /// <summary>This event will be called if the above conditions are met when your finger becomes old.</summary>
        public LeanFingerEvent OnFinger
        {
            get
            {
                if (onFinger == null) onFinger = new LeanFingerEvent();
                return onFinger;
            }
        }

        /// <summary>
        ///     This event will be called if the above conditions are met when your finger becomes old.
        ///     Vector3 = Finger position in world space.
        /// </summary>
        public Vector3Event OnWorld
        {
            get
            {
                if (onWorld == null) onWorld = new Vector3Event();
                return onWorld;
            }
        }

        /// <summary>
        ///     This event will be called if the above conditions are met when your finger becomes old.
        ///     Vector2 = Finger position in screen space.
        /// </summary>
        public Vector2Event OnScreen
        {
            get
            {
                if (onScreen == null) onScreen = new Vector2Event();
                return onScreen;
            }
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
            LeanTouch.OnFingerOld += HandleFingerOld;
        }

        protected virtual void OnDisable()
        {
            LeanTouch.OnFingerOld -= HandleFingerOld;
        }

        private void HandleFingerOld(LeanFinger finger)
        {
            // Ignore?
            if (ignoreStartedOverGui && finger.StartedOverGui) return;

            if (ignoreIsOverGui && finger.IsOverGui) return;

            if (requiredSelectable != null && requiredSelectable.IsSelected == false) return;

            if (onFinger != null) onFinger.Invoke(finger);

            if (onWorld != null)
            {
                var position = ScreenDepth.Convert(finger.ScreenPosition, gameObject);

                onWorld.Invoke(position);
            }

            if (onScreen != null) onScreen.Invoke(finger.ScreenPosition);
        }

        [Serializable]
        public class LeanFingerEvent : UnityEvent<LeanFinger>
        {
        }

        [Serializable]
        public class Vector3Event : UnityEvent<Vector3>
        {
        }

        [Serializable]
        public class Vector2Event : UnityEvent<Vector2>
        {
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
    using TARGET = LeanFingerOld;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanFingerOld_Editor : CwEditor
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

            Separator();

            var showUnusedEvents = DrawFoldout("Show Unused Events", "Show all events?");

            if (Any(tgts, t => t.OnFinger.GetPersistentEventCount() > 0) || showUnusedEvents) Draw("onFinger");

            if (Any(tgts, t => t.OnWorld.GetPersistentEventCount() > 0) || showUnusedEvents)
            {
                Draw("ScreenDepth");
                Draw("onWorld");
            }

            if (Any(tgts, t => t.OnScreen.GetPersistentEventCount() > 0) || showUnusedEvents) Draw("onScreen");
        }
    }
}
#endif