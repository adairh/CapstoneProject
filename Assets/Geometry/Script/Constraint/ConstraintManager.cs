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

        // Bây giờ gọi ApplyConstraint với cả movedShape
        public void ApplyConstraints(Shape movedShape, Vector3 movement = new Vector3())
        {
            foreach (var constraint in constraints)
            {
                if (constraint is not FixedPointConstraint)
                {
                    if (constraint.HasShape(movedShape))
                        constraint.ApplyConstraint(movedShape, movement);
                }
            }
        }
    }
}