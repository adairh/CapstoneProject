using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.Transform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the Transform's localScale value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformLocalScale")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.localScale" +
                      LeanTransition.MethodsMenuSuffix + "(LeanTransformLocalScale)")]
    public class LeanTransformLocalScale : LeanMethodWithStateAndTarget
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

        public static LeanState Register(TARGET target, Vector3 value, float duration, LeanEase ease = LeanEase.Smooth)
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

            [Tooltip("The localScale value will transition to this.")] [FormerlySerializedAs("Scale")]
            public Vector3 Value = Vector3.one;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private Vector3 oldValue;

            public override int CanFill => Target != null && Target.localScale != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.localScale;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.localScale;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.localScale = Vector3.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
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
        public static TARGET localScaleTransition(this TARGET target, Vector3 value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanTransformLocalScale.Register(target, value, duration, ease);
            return target;
        }
    }
}