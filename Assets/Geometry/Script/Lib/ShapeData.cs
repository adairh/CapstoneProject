// Manipulator/Data/ShapeData.cs
using System;
using UnityEngine;

namespace Manipulator {
    [Serializable]
    public abstract class ShapeData {
        public string Name;
        public Vector3 Position;
        public abstract string Type { get; }
    }

    [Serializable]
    public class PointData : ShapeData {
        public override string Type => "Point";
    }

    [Serializable]
    public class SegmentData : ShapeData {
        public override string Type => "Segment";
        public string StartPointName;
        public string EndPointName;
    }

    [Serializable]
    public abstract class ConstraintData {
        public string Type;
    }

    [Serializable]
    public class AngleConstraintData : ConstraintData {
        public string PivotName;
        public string SegmentAName;
        public string SegmentBName;
        public float TargetAngleDeg;
        public AngleConstraintData() { Type = "Angle"; }
    }
}