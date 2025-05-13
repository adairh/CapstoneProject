
using UnityEngine;

namespace Manipulator
{
    public class IsoscelesTriangleSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Tam giác cân", new string[] { "Độ dài đáy", "Chiều cao" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 2) return;
            float baseLength = values[0];
            float height = values[1];

            Vector3 a = new Vector3(-baseLength / 2, 0, 0);
            Vector3 b = new Vector3(baseLength / 2, 0, 0);
            Vector3 c = new Vector3(0, height, 0);

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idAB = System.Guid.NewGuid().ToString();
            string idBC = System.Guid.NewGuid().ToString();
            string idCA = System.Guid.NewGuid().ToString();

            var dataList = new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = idCA, Type = "Segment", ConnectedPoints = new() { idC, idA } }
            };

            var batch = new CreateShapeBatchAction(dataList);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
