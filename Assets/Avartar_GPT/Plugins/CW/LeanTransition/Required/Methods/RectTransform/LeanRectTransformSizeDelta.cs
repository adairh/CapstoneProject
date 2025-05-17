using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.RectTransform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the RectTransform's sizeDelta value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformSizeDelta")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.sizeDelta" +
                      LeanTransition.MethodsMenuSuffix + "(LeanRectTransformSizeDelta)")]
    public class LeanRectTransformSizeDelta : LeanMethodWithStateAndTarget
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

        public static LeanState Register(TARGET target, Vector2 value, float duration, LeanEase ease = LeanEase.Smooth)
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

            [Tooltip("The sizeDelta value will transition to this.")] [FormerlySerializedAs("SizeDelta")]
            public Vector2 Value;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private Vector2 oldValue;

            public override int CanFill => Target != null && Target.sizeDelta != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.sizeDelta;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.sizeDelta;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.sizeDelta = Vector2.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
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
        public static TARGET sizeDeltaTransition(this TARGET target, Vector2 value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanRectTransformSizeDelta.Register(target, value, duration, ease);
            return target;
        }
    }
}