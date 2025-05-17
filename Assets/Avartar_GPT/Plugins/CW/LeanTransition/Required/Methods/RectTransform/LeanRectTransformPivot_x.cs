using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.RectTransform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the RectTransform's pivot.x value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformPivot_x")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.pivot.x" +
                      LeanTransition.MethodsMenuSuffix + "(LeanRectTransformPivot_x)")]
    public class LeanRectTransformPivot_x : LeanMethodWithStateAndTarget
    {
        public State Data;

        public override Type GetTargetType()
        {
            return typeof(TARGET);
        }

        public override void Register()
        {
            PreviousState = Register(GetAliasedTarget(Data.Target), Data.Value, Data.Duration, Data.Ease);
        }

        public static LeanState Register(TARGET target, float value, float duration, LeanEase ease = LeanEase.Smooth)
        {
            var state = LeanTransition.SpawnWithTarget(State.Pool, target);

            state.Value = value;

            state.Ease = ease;

            return LeanTransition.Register(state, duration);
        }

        [Serializable]
        public class State : LeanStateWithTarget<TARGET>
        {
            public static Stack<State> Pool = new();

            [Tooltip("The pivot value will transition to this.")] [FormerlySerializedAs("Pivot")]
            public float Value;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private float oldValue;

            public override int CanFill => Target != null && Target.pivot.x != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.pivot.x;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.pivot.x;
            }

            public override void UpdateWithTarget(float progress)
            {
                var vector = Target.pivot;

                vector.x = Mathf.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));

                Target.pivot = vector;
            }

            public override void Despawn()
            {
                Pool.Push(this);
            }
        }
    }
}

namespace Lean.Transition
{
    public static partial class LeanExtensions
    {
        public static TARGET pivotTransition_x(this TARGET target, float value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanRectTransformPivot_x.Register(target, value, duration, ease);
            return target;
        }
    }
}