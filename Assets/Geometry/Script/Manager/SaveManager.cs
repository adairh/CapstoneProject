using System.Collections.Generic;
using System.IO;
using UnityEngine; 

namespace Manipulator
{
    public static class SaveManager
    {
        private const string SavePath = "save.json";

        public static void SaveScene()
        {
            var sceneData = new SceneData();
            foreach (var shape in ShapeStorage.GetAllShapes())
            {
                var data = shape.Serialize();
                if (data != null)
                    sceneData.Shapes.Add(data);
            }

            string json = JsonUtility.ToJson(sceneData, true);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, SavePath), json);
            Debug.Log("Scene saved to " + Path.Combine(Application.persistentDataPath, SavePath));
        }

        public static void LoadScene()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, SavePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("No save file found.");
                return;
            }

            string json = File.ReadAllText(fullPath);
            SceneData data = JsonUtility.FromJson<SceneData>(json);

            // Clear current
            foreach (var shape in ShapeStorage.GetAllShapes())
            {
                if (shape != null)
                    Object.Destroy(shape.gameObject);
            }
            ShapeStorage.Clear();

            Dictionary<string, Point> pointMap = new();

            // Spawn Points first
            foreach (var shapeData in data.Shapes)
            {
                if (shapeData.Type == "Point")
                {
                    var point = ShapeFactory.CreateFromData(shapeData) as Point;
                    pointMap[point.ShapeId] = point;
                }
            }

            // Then spawn all others
            foreach (var shapeData in data.Shapes)
            {
                if (shapeData.Type == "Point") continue;

                var shape = ShapeFactory.CreateFromData(shapeData);
                if (shape is Segment segment && shapeData.ConnectedPoints.Count == 2)
                {
                    string a = shapeData.ConnectedPoints[0];
                    string b = shapeData.ConnectedPoints[1];
                    if (pointMap.ContainsKey(a) && pointMap.ContainsKey(b))
                        segment.SetEndpoints(pointMap[a], pointMap[b]);
                }
            }

            Debug.Log("Scene loaded");
        }
    }
}