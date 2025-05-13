
using UnityEngine;

namespace Manipulator
{
    public class SquareSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Hình vuông", new string[] { "Độ dài cạnh" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 1) return;
            float length = values[0];

            Vector3 a = Vector3.zero;
            Vector3 b = new Vector3(length, 0, 0);
            Vector3 c = new Vector3(length, length, 0);
            Vector3 d = new Vector3(0, length, 0);

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idD = System.Guid.NewGuid().ToString();
            string idAB = System.Guid.NewGuid().ToString();
            string idBC = System.Guid.NewGuid().ToString();
            string idCD = System.Guid.NewGuid().ToString();
            string idDA = System.Guid.NewGuid().ToString();

            var dataList = new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idD, Type = "Point", Position = d },
                new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = idCD, Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = idDA, Type = "Segment", ConnectedPoints = new() { idD, idA } }
            };

            var batch = new CreateShapeBatchAction(dataList);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
