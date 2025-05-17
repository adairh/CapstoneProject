using System;
using CW.Common;
using Lean.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
    /// <summary>This component allows you to detect when a finger is touching the screen.</summary>
    [HelpURL(LeanTouch.HelpUrlPrefix + "LeanFingerUpdate")]
    [AddComponentMenu(LeanTouch.ComponentPathPrefix + "Finger Update")]
    public class LeanFingerUpdate : MonoBehaviour
    {
        public enum CoordinateType
        {
            ScaledPixels,
            ScreenPixels,
            ScreenPercentage
        }

        [SerializeField] private bool ignoreStartedOverGui = true;
        [SerializeField] private bool ignoreIsOverGui;
        [SerializeField] private bool ignoreIfStatic;
        [SerializeField] private bool ignoreIfDown;
        [SerializeField] private bool ignoreIfUp;
        [SerializeField] private bool ignoreIfHover = true;
        [SerializeField] private LeanSelectable requiredSelectable;
        [SerializeField] private LeanFingerEvent onFinger;
        [SerializeField] private CoordinateType coordinate;
        [SerializeField] private float multiplier = 1.0f;
        [SerializeField] private Vector2Event onDelta;
        [SerializeField] private FloatEvent onDistance;

        /// <summary>
        ///     The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more
        ///     information.
        /// </summary>
        public LeanScreenDepth ScreenDepth = new(LeanScreenDepth.ConversionType.DepthIntercept);

        [SerializeField] private Vector3Event onWorldFrom;
        [SerializeField] private Vector3Event onWorldTo;
        [SerializeField] private Vector3Event onWorldDelta;
        [SerializeField] private Vector3Vector3Event onWorldFromTo;

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

        /// <summary>If the finger didn't move, ignore it?</summary>
        public bool IgnoreIfStatic
        {
            set => ignoreIfStatic = value;
            get => ignoreIfStatic;
        }

        /// <summary>If the finger just began touching the screen, ignore it?</summary>
        public bool IgnoreIfDown
        {
            set => ignoreIfDown = value;
            get => ignoreIfDown;
        }

        /// <summary>If the finger just stopped touching the screen, ignore it?</summary>
        public bool IgnoreIfUp
        {
            set => ignoreIfUp = value;
            get => ignoreIfUp;
        }

        /// <summary>If the finger is the mouse hover, ignore it?</summary>
        public bool IgnoreIfHover
        {
            set => ignoreIfHover = value;
            get => ignoreIfHover;
        }

        /// <summary>If the specified object is set and isn't selected, then this component will do nothing.</summary>
        public LeanSelectable RequiredSelectable
        {
            set => requiredSelectable = value;
            get => requiredSelectable;
        }

        /// <summary>Called on every frame the conditions are met.</summary>
        public LeanFingerEvent OnFinger
        {
            get
            {
                if (onFinger == null) onFinger = new LeanFingerEvent();
                return onFinger;
            }
        }

        /// <summary>The coordinate space of the OnDelta values.</summary>
        public CoordinateType Coordinate
        {
            set => coordinate = value;
            get => coordinate;
        }

        /// <summary>The delta values will be multiplied by this when output.</summary>
        public float Multiplier
        {
            set => multiplier = value;
            get => multiplier;
        }

        /// <summary>
        ///     This event is invoked when the requirements are met.
        ///     Vector2 = Position Delta based on your Coordinates setting.
        /// </summary>
        public Vector2Event OnDelta
        {
            get
            {
                if (onDelta == null) onDelta = new Vector2Event();
                return onDelta;
            }
        }

        /// <summary>
        ///     Called on the first frame the conditions are met.
        ///     Float = The distance/magnitude/length of the swipe delta vector.
        /// </summary>
        public FloatEvent OnDistance
        {
            get
            {
                if (onDistance == null) onDistance = new FloatEvent();
                return onDistance;
            }
        }

        /// <summary>
        ///     Called on the first frame the conditions are met.
        ///     Vector3 = Start point in world space.
        /// </summary>
        public Vector3Event OnWorldFrom
        {
            get
            {
                if (onWorldFrom == null) onWorldFrom = new Vector3Event();
                return onWorldFrom;
            }
        }

        /// <summary>
        ///     Called on the first frame the conditions are met.
        ///     Vector3 = End point in world space.
        /// </summary>
        public Vector3Event OnWorldTo
        {
            get
            {
                if (onWorldTo == null) onWorldTo = new Vector3Event();
                return onWorldTo;
            }
        }

        /// <summary>
        ///     Called on the first frame the conditions are met.
        ///     Vector3 = The vector between the start and end points in world space.
        /// </summary>
        public Vector3Event OnWorldDelta
        {
            get
            {
                if (onWorldDelta == null) onWorldDelta = new Vector3Event();
                return onWorldDelta;
            }
        }

        /// <summary>
        ///     Called on the first frame the conditions are met.
        ///     Vector3 = Start point in world space.
        ///     Vector3 = End point in world space.
        /// </summary>
        public Vector3Vector3Event OnWorldFromTo
        {
            get
            {
                if (onWorldFromTo == null) onWorldFromTo = new Vector3Vector3Event();
                return onWorldFromTo;
            }
        }

        protected virtual void Awake()
        {
            if (requiredSelectable == null) requiredSelectable = GetComponentInParent<LeanSelectable>();
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            requiredSelectable = GetComponentInParent<LeanSelectable>();
        }
#endif

        protected virtual void OnEnable()
        {
            LeanTouch.OnFingerUpdate += HandleFingerUpdate;
        }

        protected virtual void OnDisable()
        {
            LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        }

        private void HandleFingerUpdate(LeanFinger finger)
        {
            if (ignoreStartedOverGui && finger.StartedOverGui) return;

            if (ignoreIsOverGui && finger.IsOverGui) return;

            if (ignoreIfStatic && finger.ScreenDelta.magnitude <= 0.0f) return;

            if (ignoreIfDown && finger.Down) return;

            if (ignoreIfUp && finger.Up) return;

            if (ignoreIfHover && finger.Index == LeanTouch.HOVER_FINGER_INDEX) return;

            if (requiredSelectable != null && requiredSelectable.IsSelected == false) return;

            if (onFinger != null) onFinger.Invoke(finger);

            var finalDelta = finger.ScreenDelta;

            switch (coordinate)
            {
                case CoordinateType.ScaledPixels:
                    finalDelta *= LeanTouch.ScalingFactor;
                    break;
                case CoordinateType.ScreenPercentage:
                    finalDelta *= LeanTouch.ScreenFactor;
                    break;
            }

            finalDelta *= multiplier;

            if (onDelta != null) onDelta.Invoke(finalDelta);

            if (onDistance != null) onDistance.Invoke(finalDelta.magnitude);

            var worldFrom = ScreenDepth.Convert(finger.LastScreenPosition, gameObject);
            var worldTo = ScreenDepth.Convert(finger.ScreenPosition, gameObject);

            if (onWorldFrom != null) onWorldFrom.Invoke(worldFrom);

            if (onWorldTo != null) onWorldTo.Invoke(worldTo);

            if (onWorldDelta != null) onWorldDelta.Invoke(worldTo - worldFrom);

            if (onWorldFromTo != null) onWorldFromTo.Invoke(worldFrom, worldTo);
        }

        [Serializable]
        public class LeanFingerEvent : UnityEvent<LeanFinger>
        {
        }

        [Serializable]
        public class FloatEvent : UnityEvent<float>
        {
        }

        [Serializable]
        public class Vector2Event : UnityEvent<Vector2>
        {
        }

        [Serializable]
        public class Vector3Event : UnityEvent<Vector3>
        {
        }

        [Serializable]
        public class Vector3Vector3Event : UnityEvent<Vector3, Vector3>
        {
        }
    }
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
    using TARGET = LeanFingerUpdate;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TARGET))]
    public class LeanFingerUpdate_Editor : CwEditor
    {
        protected override void OnInspector()
        {
            TARGET tgt;
            TARGET[] tgts;
            GetTargets(out tgt, out tgts);

            Draw("ignoreStartedOverGui", "Ignore fingers with StartedOverGui?");
            Draw("ignoreIsOverGui", "Ignore fingers with OverGui?");
            Draw("ignoreIfStatic", "If the finger didn't move, ignore it?");
            Draw("requiredSelectable",
                "If the specified object is set and isn't selected, then this component will do nothing.");
            Draw("ignoreIfDown", "If the finger just began touching the screen, ignore it?");
            Draw("ignoreIfUp", "If the finger just stopped touching the screen, ignore it?");
            Draw("ignoreIfHover", "If the finger is the mouse hover, ignore it?");

            Separator();

            var usedA = Any(tgts, t => t.OnFinger.GetPersistentEventCount() > 0);
            var usedB = Any(tgts, t => t.OnDelta.GetPersistentEventCount() > 0);
            var usedC = Any(tgts, t => t.OnDistance.GetPersistentEventCount() > 0);
            var usedD = Any(tgts, t => t.OnWorldFrom.GetPersistentEventCount() > 0);
            var usedE = Any(tgts, t => t.OnWorldTo.GetPersistentEventCount() > 0);
            var usedF = Any(tgts, t => t.OnWorldDelta.GetPersistentEventCount() > 0);
            var usedG = Any(tgts, t => t.OnWorldFromTo.GetPersistentEventCount() > 0);

            var showUnusedEvents = DrawFoldout("Show Unused Events", "Show all events?");

            if (usedA || showUnusedEvents) Draw("onFinger");

            if (usedB || usedC || showUnusedEvents)
            {
                Draw("coordinate", "The coordinate space of the OnDelta values.");
                Draw("multiplier", "The delta values will be multiplied by this when output.");
            }

            if (usedB || showUnusedEvents) Draw("onDelta");

            if (usedC || showUnusedEvents) Draw("onDistance");

            if (usedD || usedE || usedF || usedG || showUnusedEvents) Draw("ScreenDepth");

            if (usedD || showUnusedEvents) Draw("onWorldFrom");

            if (usedE || showUnusedEvents) Draw("onWorldTo");

            if (usedF || showUnusedEvents) Draw("onWorldDelta");

            if (usedG || showUnusedEvents) Draw("onWorldFromTo");
        }
    }
}
#endif