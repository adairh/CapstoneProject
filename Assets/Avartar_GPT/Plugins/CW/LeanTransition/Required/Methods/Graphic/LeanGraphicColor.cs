using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Serialization;
using TARGET = UnityEngine.UI.Graphic;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to transition the Graphic's color value.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanGraphicColor")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Graphic/Graphic.color" + LeanTransition.MethodsMenuSuffix +
                      "(LeanGraphicColor)")]
    public class LeanGraphicColor : LeanMethodWithStateAndTarget
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

        public static LeanState Register(TARGET target, Color value, float duration, LeanEase ease = LeanEase.Smooth)
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

            [Tooltip("The color value will transition to this.")] [FormerlySerializedAs("Color")]
            public Color Value = Color.white;

            [Tooltip("This allows you to control how the transition will look.")]
            public LeanEase Ease = LeanEase.Smooth;

            [NonSerialized] private Color oldValue;

            public override int CanFill => Target != null && Target.color != Value ? 1 : 0;

            public override void FillWithTarget()
            {
                Value = Target.color;
            }

            public override void BeginWithTarget()
            {
                oldValue = Target.color;
            }

            public override void UpdateWithTarget(float progress)
            {
                Target.color = Color.LerpUnclamped(oldValue, Value, Smooth(Ease, progress));
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
        public static TARGET colorTransition(this TARGET target, Color value, float duration,
            LeanEase ease = LeanEase.Smooth)
        {
            LeanGraphicColor.Register(target, value, duration, ease);
            return target;
        }
    }
}