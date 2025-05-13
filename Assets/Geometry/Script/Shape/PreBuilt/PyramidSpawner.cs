
using UnityEngine;

namespace Manipulator
{
    public class PyramidSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Chóp đều (tam giác)", new string[] { "Độ dài cạnh đáy", "Chiều cao" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 2) return;
            float side = values[0];
            float height = values[1];

            Vector3 a = Vector3.zero;
            Vector3 b = new Vector3(side, 0, 0);
            Vector3 c = new Vector3(side / 2f, Mathf.Sqrt(3f) / 2f * side, 0);
            Vector3 apex = (a + b + c) / 3f + Vector3.forward * height;

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idD = System.Guid.NewGuid().ToString(); // apex

            var datas = new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idD, Type = "Point", Position = apex },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idA } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idD } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idD } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } }
            };

            var batch = new CreateShapeBatchAction(datas);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
