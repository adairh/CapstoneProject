using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using TARGET = UnityEngine.RectTransform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the RectTransform's sizeDelta.x value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformSizeDelta_x")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.sizeDelta.x" +
                      LeanTransition.MethodsMenuSuffix + "(LeanRectTransformSizeDelta_x)")]
    public class LeanRectTransformSizeDelta_x : LeanMethodWithStateAndTarget
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

            [Tooltip("The sizeDelta value will transition to this.")]
            public float Value;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private float oldValue;

            public override int CanFill => Target != null && Target.sizeDelta.x != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.sizeDelta.x;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.sizeDelta.x;
            }

            public override void UpdateWithTarget(float progress)
            {
                var vector = Target.sizeDelta;

                vector.x = Mathf.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));

                Target.sizeDelta = vector;
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
        public static TARGET sizeDeltaTransition_x(this TARGET target, float value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanRectTransformSizeDelta_x.Register(target, value, duration, ease);
            return target;
        }
    }
}