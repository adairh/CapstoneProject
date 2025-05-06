using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine; 

namespace Manipulator
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static void SaveAll()
        {
            var allShapes = ShapeStorage.GetAllShapes();
            var saveData = allShapes.Select(shape => shape.Serialize()).ToList();
            string json = JsonUtility.ToJson(new ShapeDataList { shapes = saveData });
            PlayerPrefs.SetString("SaveData", json);
        }

        public static void LoadAll()
        {
            ShapeStorage.Clear();
            var json = PlayerPrefs.GetString("SaveData");
            var dataList = JsonUtility.FromJson<ShapeDataList>(json);

            foreach (var data in dataList.shapes)
            {
                ShapeFactory.CreateFromData(data);
            }
        }

        [Serializable]
        public class ShapeDataList
        {
            public List<ShapeData> shapes;
        }
    }

}