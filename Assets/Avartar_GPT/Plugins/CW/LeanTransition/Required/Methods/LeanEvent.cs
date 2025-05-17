using System;
using System.Collections.Generic;
using Lean.Transition.Method;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Transition.Method
{
    /// <summary>This component allows you to invoke a custom action after the specified duration.</summary>
    [HelpURL(LeanTransition.HelpUrlPrefix + "LeanEvent")]
    [AddComponentMenu(LeanTransition.MethodsMenuPrefix + "Event" + LeanTransition.MethodsMenuSuffix + "(LeanEvent)")]
    public class LeanEvent : LeanMethodWithState
    {
        public State Data;

        public override void Register()
        {
            PreviousState = Register(Data.Event, Data.Duration);
        }

        public static LeanState Register(Action action, float duration)
        {
            var state = LeanTransition.Spawn(State.Pool);

            state.Action = action;
            state.Event = null;

            return LeanTransition.Register(state, duration);
        }

        public static LeanState Register(UnityEvent action, float duration)
        {
            var state = LeanTransition.Spawn(State.Pool);

            state.Action = null;
            state.Event = action;

            return LeanTransition.Register(state, duration);
        }

        [Serializable]
        public class State : LeanState
        {
            public static Stack<State> Pool = new();

            [Tooltip("The event that will be invoked.")]
            public UnityEvent Event;

            [NonSerialized] public Action Action;

            public override ConflictType Conflict => ConflictType.None;

            public override void Begin()
            {
                // No state to begin from
            }

            public override void Update(float progress)
            {
                if (progress == 1.0f)
                {
                    if (Event != null) Event.Invoke();

                    if (Action != null) Action.Invoke();
                }
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
        public static T EventTransition<T>(this T target, Action action, float duration = 0.0f)
            where T : Component
        {
            LeanEvent.Register(action, duration);
            return target;
        }

        public static GameObject EventTransition(this GameObject target, Action action, float duration = 0.0f)
        {
            LeanEvent.Register(action, duration);
            return target;
        }
    }
}