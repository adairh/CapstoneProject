using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.Light;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the Light's intensity value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanLightIntensity")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Light/Light.intensity" + LeanTransition.MethodsMenuSuffix +
                      "(LeanLightIntensity)")]
    public class LeanLightIntensity : LeanMethodWithStateAndTarget
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

            [Tooltip("The intensity value will transition to this.")] [FormerlySerializedAs("Intensity")]
            public float Value = 1.0f;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private float oldValue;

            public override int CanFill => Target != null && Target.intensity != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.intensity;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.intensity;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.intensity = Mathf.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
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
        public static TARGET intensityTransition(this TARGET target, float value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanLightIntensity.Register(target, value, duration, ease);
            return target;
        }
    }
}