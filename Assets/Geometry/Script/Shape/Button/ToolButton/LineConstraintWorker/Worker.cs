using UnityEngine;
using System;
using System.Collections.Generic;

namespace Manipulator
{
    [Serializable]
    public class ParallelConstraintData : ConstraintData
    {
        public string Segment1Id;
        public string Segment2Id;

        public override void Restore()
        {
            // Locate the segment objects by their IDs
            Segment seg1 = ShapeStorage.GetById(Segment1Id) as Segment;
            Segment seg2 = ShapeStorage.GetById(Segment2Id) as Segment;
            if (seg1 == null || seg2 == null) return;
            // Attach ParallelConstraint script to one of the segment objects (attach to seg2 for example)
            var constraint = seg2.gameObject.AddComponent<ParallelConstraint>();
            constraint.Segment1 = seg1;
            constraint.Segment2 = seg2;
            // Register the constraint with the manager
            ConstraintManager.Instance.RegisterConstraint(constraint);
        }
    }

    public class ParallelConstraint : Constraint
    {
        public Segment Segment1;  // Original segment
        public Segment Segment2;  // New segment that should remain parallel to Segment1

        public override bool HasShape(Shape shape)
        {
            if (shape == null) return false;
            // Check if the changed shape is either of the two segments or their endpoints
            if (shape == Segment1 || shape == Segment2) return true;
            // Also react if an endpoint point of either segment moves
            if (shape is Point)
            {
                if (shape == Segment1.StartPoint || shape == Segment1.EndPoint ||
                    shape == Segment2.StartPoint || shape == Segment2.EndPoint)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ApplyConstraint(Shape changedShape, Vector3 delta)
        {
            if (Segment1 == null || Segment2 == null) return;
            // Determine which segment was moved, and which needs to be adjusted
            bool segment1Moved = (changedShape == Segment1 || changedShape == Segment1.StartPoint || changedShape == Segment1.EndPoint);
            bool segment2Moved = (changedShape == Segment2 || changedShape == Segment2.StartPoint || changedShape == Segment2.EndPoint);

            // Calculate current direction vectors for both segments
            Vector3 dir1 = (Segment1.EndPoint.transform.position - Segment1.StartPoint.transform.position).normalized;
            Vector3 dir2 = (Segment2.EndPoint.transform.position - Segment2.StartPoint.transform.position).normalized;

            if (segment1Moved && !segment2Moved)
            {
                // Align Segment2 to Segment1's direction
                AlignSegmentDirection(Segment2, dir1);
            }
            else if (segment2Moved && !segment1Moved)
            {
                // Align Segment1 to Segment2's direction
                AlignSegmentDirection(Segment1, dir2);
            }
            // If both segments moved simultaneously, we do nothing (or could choose one as master).
        }

        /// <summary>
        /// Rotate the given segment around its first endpoint to align its direction with targetDir.
        /// </summary>
        private void AlignSegmentDirection(Segment segment, Vector3 targetDir)
        {
            if (segment == null) return;
            // We'll keep the segment's StartPoint fixed, and move the EndPoint to align direction.
            Point fixedPoint = segment.StartPoint;
            Point movingPoint = segment.EndPoint;
            float length = Vector3.Distance(fixedPoint.transform.position, movingPoint.transform.position);
            // New position for movingPoint = fixedPoint + targetDir * length
            Vector3 newPos = fixedPoint.transform.position + targetDir * length;
            // Apply the move
            movingPoint.transform.position = newPos;
            // If the segment object has a visual or collider, you might update it here if not handled elsewhere.
        }

        public override ConstraintData Serialize()
        {
            return new ParallelConstraintData
            {
                ConstraintId = this.ConstraintId,
                Segment1Id = Segment1.ShapeId,
                Segment2Id = Segment2.ShapeId,
                Type = "Parallel"
            };
        }

        // Optional: cleanup logic if needed (e.g., remove references)
        public override void Cleanup() { }
        public override IEnumerable<Shape> GetRelatedShapes()
        {
            return null;
        }
    }

    [Serializable]
    public class PerpendicularConstraintData : ConstraintData
    {
        public string Segment1Id;
        public string Segment2Id;

        public override void Restore()
        {
            Segment seg1 = ShapeStorage.GetById(Segment1Id) as Segment;
            Segment seg2 = ShapeStorage.GetById(Segment2Id) as Segment;
            if (seg1 == null || seg2 == null) return;
            var constraint = seg2.gameObject.AddComponent<PerpendicularConstraint>();
            constraint.Segment1 = seg1;
            constraint.Segment2 = seg2;
            ConstraintManager.Instance.RegisterConstraint(constraint);
        }
    }

    public class PerpendicularConstraint : Constraint
    {
        public Segment Segment1;
        public Segment Segment2;

        public override bool HasShape(Shape shape)
        {
            if (shape == null) return false;
            if (shape == Segment1 || shape == Segment2) return true;
            if (shape is Point)
            {
                if (shape == Segment1.StartPoint || shape == Segment1.EndPoint ||
                    shape == Segment2.StartPoint || shape == Segment2.EndPoint)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ApplyConstraint(Shape changedShape, Vector3 delta)
        {
            if (Segment1 == null || Segment2 == null) return;
            bool segment1Moved = (changedShape == Segment1 || changedShape == Segment1.StartPoint || changedShape == Segment1.EndPoint);
            bool segment2Moved = (changedShape == Segment2 || changedShape == Segment2.StartPoint || changedShape == Segment2.EndPoint);

            // Current direction vectors
            Vector3 dir1 = (Segment1.EndPoint.transform.position - Segment1.StartPoint.transform.position).normalized;
            Vector3 dir2 = (Segment2.EndPoint.transform.position - Segment2.StartPoint.transform.position).normalized;

            if (segment1Moved && !segment2Moved)
            {
                // Make Segment2 perpendicular to Segment1
                Vector3 targetDir = GetPerpendicularDirClosestTo(dir2, dir1);
                AlignSegmentDirection(Segment2, targetDir);
            }
            else if (segment2Moved && !segment1Moved)
            {
                // Make Segment1 perpendicular to Segment2
                Vector3 targetDir = GetPerpendicularDirClosestTo(dir1, dir2);
                AlignSegmentDirection(Segment1, targetDir);
            }
        }

        /// <summary> Compute a unit vector perpendicular to baseDir that is closest to currentDir. </summary>
        private Vector3 GetPerpendicularDirClosestTo(Vector3 currentDir, Vector3 baseDir)
        {
            // Two possible perpendiculars to baseDir in plane (assuming horizontal plane relevance)
            Vector3 perp1 = Vector3.Cross(baseDir, Vector3.up).normalized;
            if (perp1 == Vector3.zero)
                perp1 = Vector3.Cross(baseDir, Vector3.forward).normalized;
            Vector3 perp2 = -perp1;
            // Choose the perpendicular that is closest to the current direction of the segment being adjusted
            return (Vector3.Dot(currentDir, perp1) >= Vector3.Dot(currentDir, perp2)) ? perp1 : perp2;
        }

        private void AlignSegmentDirection(Segment segment, Vector3 targetDir)
        {
            if (segment == null) return;
            Point fixedPoint = segment.StartPoint;
            Point movingPoint = segment.EndPoint;
            float length = Vector3.Distance(fixedPoint.transform.position, movingPoint.transform.position);
            Vector3 newPos = fixedPoint.transform.position + targetDir * length;
            movingPoint.transform.position = newPos;
        }

        public override ConstraintData Serialize()
        {
            return new PerpendicularConstraintData
            {
                ConstraintId = this.ConstraintId,
                Segment1Id = Segment1.ShapeId,
                Segment2Id = Segment2.ShapeId,
                Type = "Perpendicular"
            };
        }

        public override void Cleanup() { }
        public override IEnumerable<Shape> GetRelatedShapes()
        {
            return null;
        }
    }
}
