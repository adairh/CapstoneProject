
using UnityEngine;

namespace Manipulator
{
    public class RhombusSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Hình thoi", new string[] { "Độ dài cạnh", "Góc giữa 2 cạnh (độ)" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 2) return;
            float side = values[0];
            float angleDeg = values[1];
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 a = Vector3.zero;
            Vector3 b = new Vector3(side, 0, 0);
            Vector3 c = b + new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * side;
            Vector3 d = c - new Vector3(side, 0, 0);

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
