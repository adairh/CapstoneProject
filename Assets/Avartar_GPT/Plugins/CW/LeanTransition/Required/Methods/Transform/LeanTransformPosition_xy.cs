using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using TARGET = UnityEngine.Transform;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the Transform's position.xy value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanTransformPosition_xy")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Transform/Transform.position.xy" +
                      LeanTransition.MethodsMenuSuffix + "(LeanTransformPosition_xy)")]
    public class LeanTransformPosition_xy : LeanMethodWithStateAndTarget
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

            [Tooltip("The position value will transition to this.")]
            public Vector2 Value;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private Vector2 oldValue;

            public override int CanFill =>
                Target != null && (Target.position.x != Value.x || Target.position.y != Value.y) ? 1 : 0;

            public override void FillWithTarget()
            {
                var vector = Target.position;

                Value.x = vector.x;
                Value.y = vector.y;
            }

            public override void BeginWithTarget()
            {
                var vector = Target.position;

                oldValue.x = vector.x;
                oldValue.y = vector.y;
            }

            public override void UpdateWithTarget(float progress)
            {
                var vector = Target.position;
                var smooth = Smooth(Ease, progress);

                vector.x = Mathf.LerpUnclamped(oldValue.x, Value.x, smooth);
                vector.y = Mathf.LerpUnclamped(oldValue.y, Value.y, smooth);

                Target.position = vector;
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
        public static TARGET positionTransition_xy(this TARGET target, Vector2 value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanTransformPosition_xy.Register(target, value, duration, ease);
            return target;
        }
    }
}