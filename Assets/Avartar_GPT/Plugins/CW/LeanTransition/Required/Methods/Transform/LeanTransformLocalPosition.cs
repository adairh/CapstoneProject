using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.Transform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the Transform's localPosition value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformLocalPosition")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.localPosition" +
                      LeanTransition.MethodsMenuSuffix + "(LeanTransformLocalPosition)")]
    public class LeanTransformLocalPosition : LeanMethodWithStateAndTarget
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

            [Tooltip("The localPosition value will transition to this.")] [FormerlySerializedAs("Position")]
            public Vector3 Value;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private Vector3 oldValue;

            public override int CanFill => Target != null && Target.localPosition != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.localPosition;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.localPosition;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.localPosition = Vector3.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
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
        public static TARGET localPositionTransition(this TARGET target, Vector3 value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanTransformLocalPosition.Register(target, value, duration, ease);
            return target;
        }
    }
}