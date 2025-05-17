// Manipulator/Data/ShapeData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    [Serializable]
    public class ShapeData
    {
        public string Id;
        public string Type;
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
        public List<string> ConnectedPoints = new(); // Cho Segment, Polygon...
        public Dictionary<string, string> Settings = new(); // Color, Thickness...
    }
}