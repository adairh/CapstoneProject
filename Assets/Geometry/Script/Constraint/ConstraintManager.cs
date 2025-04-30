using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class ConstraintManager : MonoBehaviour
    {
        public static ConstraintManager Instance { get; private set; }
        private List<Constraint> constraints = new List<Constraint>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterConstraint(Constraint constraint)
        {
            if (!constraints.Contains(constraint))
            {
                constraints.Add(constraint);
            }
        }

        public void ApplyConstraints(Shape movedShape, Vector3 movement)
        {
            foreach (Constraint constraint in constraints)
            {
                if (constraint.HasShape(movedShape))
                {
                    constraint.ApplyConstraint(movement);
                }
            }
        }
    }
}