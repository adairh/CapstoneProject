using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    /// <summary>
    /// Spawner for creating a Segment using the dynamic input panel system.
    /// It collects user input (length, angle), and uses the user's picked point as the starting position.
    /// </summary>
    public class SegmentSpawner : IShapeSpawner
    {
        // Define the input fields shown in the panel
        public List<FieldDefinition> GetFieldDefinitions()
        {
            return new List<FieldDefinition>
            {
                new FieldDefinition
                {
                    Name = "Length", 
                    Type = FieldType.Length,
                    IsRequired = true,
                },
                new FieldDefinition
                {
                    Name = "Angle", 
                    Type = FieldType.Angle,
                    IsRequired = false,
                }
            };
        }

        // This is called by ShapeInputController.OnSubmit() after the panel is filled and validated
        public List<ShapeData> ComputeShape(Dictionary<string, float> solvedInputs)
        {
            // 1. Get required fields from user input
            if (!solvedInputs.TryGetValue("Length", out float length) || length <= 0.0001f)
                throw new Exception("Length must be provided and > 0.");

            float angle = 0;
            if (solvedInputs.ContainsKey("Angle"))
                angle = solvedInputs["Angle"];

            // 2. Get the position where the user clicked on the canvas
            Transform lookingPoint = CameraController.Instance.target;

            Vector3 start = lookingPoint.position;

            // 3. Use the standard Segment drawer method to ensure correct logic/network/undo/redo
            Segment.Drawer.StartSegmentFromPanel(start, length, angle);

            // The system does not expect any returned ShapeData for panel-based spawning, as all logic is handled by the drawer.
            return null;
        }
    }
}
