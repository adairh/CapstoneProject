using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class ConstraintManager : MonoBehaviour
    {
        private readonly List<Constraint> allConstraints = new();
        public static ConstraintManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void RegisterConstraint(Constraint constraint)
        {
            if (!allConstraints.Contains(constraint))
                allConstraints.Add(constraint);
        }

        public void UnregisterConstraint(Constraint constraint)
        {
            allConstraints.Remove(constraint);
        }

        public void ApplyConstraints(Shape changedShape, Vector3 delta)
        {
            foreach (var constraint in allConstraints)
                if (constraint.HasShape(changedShape))
                    constraint.ApplyConstraint(changedShape, delta);
        }

        public IEnumerable<ConstraintData> SerializeAll()
        {
            foreach (var constraint in allConstraints)
                yield return constraint.Serialize();
        }

        public void ClearAll()
        {
            foreach (var constraint in allConstraints)
                constraint.Cleanup();
            allConstraints.Clear();
        }
    }
}