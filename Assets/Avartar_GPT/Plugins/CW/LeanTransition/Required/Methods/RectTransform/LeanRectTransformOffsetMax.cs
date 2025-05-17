using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.RectTransform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the RectTransform's offsetMax value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanRectTransformOffsetMax")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "RectTransform/RectTransform.offsetMax" +
                      LeanTransition.MethodsMenuSuffix + "(LeanRectTransformOffsetMax)")]
    public class LeanRectTransformOffsetMax : LeanMethodWithStateAndTarget
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

            [Tooltip("The offsetMax value will transition to this.")] [FormerlySerializedAs("OffsetMax")]
            public Vector2 Value;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private Vector2 oldValue;

            public override int CanFill => Target != null && Target.offsetMax != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.offsetMax;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.offsetMax;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.offsetMax = Vector2.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
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
        public static TARGET offsetMaxTransition(this TARGET target, Vector2 value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanRectTransformOffsetMax.Register(target, value, duration, ease);
            return target;
        }
    }
}