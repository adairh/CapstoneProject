
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Tam giác đều", new string[] { "Độ dài cạnh" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 1) return;
            float length = values[0];

            Vector3 a = new Vector3(-length / 2, 0, 0);
            Vector3 b = new Vector3(length / 2, 0, 0);
            float height = Mathf.Sqrt(3f) / 2f * length;
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
