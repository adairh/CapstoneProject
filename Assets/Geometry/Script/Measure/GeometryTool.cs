using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public static class GeometryTool
    {
        /// <summary>
        /// Create a new segment parallel to the line defined by points a-b, passing through point p (or position).
        /// If p is an existing Point shape, the new segment will use p as one endpoint. Otherwise, p is treated as a position (Vector3).
        /// </summary>
        public static void CreateParallelThrough(Point a, Point b, Point throughPoint, float length = 3f)
        {
            // Calculate direction vector parallel to segment (a,b)
            Vector3 baseDir = (b.transform.position - a.transform.position).normalized;
            CreateLineThrough(baseDir, throughPoint, "Parallel");
        }

        /// <summary>
        /// Create a new segment perpendicular to the line defined by points a-b, passing through point p (or position).
        /// If p is an existing Point shape, the new segment will use p as one endpoint.
        /// </summary>
        public static void CreatePerpendicularThrough(Point a, Point b, Point throughPoint, float length = 3f)
        {
            // Calculate a perpendicular direction vector to segment (a,b)
            Vector3 baseDir = (b.transform.position - a.transform.position).normalized;
            // Try cross with global up to get perpendicular in horizontal plane; fall back to global forward if needed
            Vector3 perpDir = Vector3.Cross(baseDir, Vector3.up).normalized;
            if (perpDir == Vector3.zero)
                perpDir = Vector3.Cross(baseDir, Vector3.forward).normalized;
            CreateLineThrough(perpDir, throughPoint, "Perpendicular");
        }

        /// <summary>
        /// Generic helper to create a line (segment) in a given direction, passing through the given point or position.
        /// ConstraintType should be "Parallel" or "Perpendicular" to denote which constraint to apply.
        /// </summary>
        private static void CreateLineThrough(Vector3 direction, Point throughPoint, string constraintType, float length = 3f)
        {
            // Prepare shape data list for Undo/Redo batch creation
            var newShapes = new List<ShapeData>();

            // Determine positions for new segment's endpoints
            Vector3 p1, p2;
            string throughPointId = null;

            if (throughPoint != null)
            {
                // Use the existing point as one endpoint of the new segment
                throughPointId = throughPoint.ShapeId;
                p1 = throughPoint.transform.position;                       // Anchor at existing point
                p2 = throughPoint.transform.position + direction * length;  // Extend in one direction
                // Create one new point (the second endpoint)
                string newPointId = Guid.NewGuid().ToString();
                var pointData = new ShapeData
                {
                    Id        = newPointId,
                    Type      = "Point",
                    Position  = p2,
                    Rotation  = Quaternion.identity,
                    Scale     = Vector3.one,
                    ConnectedPoints = new List<string>(),
                    Settings  = new Dictionary<string, string>()
                };
                newShapes.Add(pointData);

                // Create segment connecting the existing point and the new point
                string segId = Guid.NewGuid().ToString();
                var segmentData = new ShapeData
                {
                    Id        = segId,
                    Type      = "Segment",
                    // Position at midpoint for consistency (this may be adjusted in Deserialize)
                    Position  = (p1 + p2) / 2,
                    Rotation  = Quaternion.identity,
                    Scale     = Vector3.one,
                    ConnectedPoints = new List<string> { throughPointId, newPointId },
                    Settings  = new Dictionary<string, string>()
                };
                newShapes.Add(segmentData);
            }
            else
            {
                // throughPoint is null: treat Input.mousePosition point as a position (no existing point)
                // We'll create two new points such that the line passes through that position (centered)
                Vector3 center = ManipulationManager.Instance.TrackingPoint; 
                // (TrackingPoint will have been set to last clicked position on plane in our Button script)
                // If not set, default center as origin
                if (center == default) center = Vector3.zero;

                p1 = center - direction * (length / 2f);
                p2 = center + direction * (length / 2f);
                string id1 = Guid.NewGuid().ToString();
                string id2 = Guid.NewGuid().ToString();
                string segId = Guid.NewGuid().ToString();

                var point1Data = new ShapeData
                {
                    Id        = id1,
                    Type      = "Point",
                    Position  = p1,
                    Rotation  = Quaternion.identity,
                    Scale     = Vector3.one,
                    ConnectedPoints = new List<string>(),
                    Settings  = new Dictionary<string, string>()
                };
                var point2Data = new ShapeData
                {
                    Id        = id2,
                    Type      = "Point",
                    Position  = p2,
                    Rotation  = Quaternion.identity,
                    Scale     = Vector3.one,
                    ConnectedPoints = new List<string>(),
                    Settings  = new Dictionary<string, string>()
                };
                var segmentData = new ShapeData
                {
                    Id        = segId,
                    Type      = "Segment",
                    Position  = center,
                    Rotation  = Quaternion.identity,
                    Scale     = Vector3.one,
                    ConnectedPoints = new List<string> { id1, id2 },
                    Settings  = new Dictionary<string, string>()
                };

                newShapes.Add(point1Data);
                newShapes.Add(point2Data);
                newShapes.Add(segmentData);
            }

            // Create all new shapes in one batch (undo-redo aware)
            var batchAction = new CreateShapeBatchAction(newShapes);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batchAction);

            // After shapes are created, set up the appropriate constraint if possible
            // The ConstraintData will link the original segment and the newly created segment.
            if (!string.IsNullOrEmpty(constraintType) && throughPointId != null)
            {
                // If throughPoint exists, we likely have its ShapeId. Find original segment via throughPoint? (In practice, we will handle constraint in button logic where original segment is known.)
            }
            // Note: Constraint creation is handled in the UI Button script after calling this, where original segment reference is available.
        }
    }
}
