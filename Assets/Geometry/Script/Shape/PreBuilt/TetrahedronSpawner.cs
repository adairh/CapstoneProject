
using UnityEngine;

namespace Manipulator
{
    public class TetrahedronSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Tứ diện đều", new string[] { "Độ dài cạnh" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 1) return;
            float length = values[0];

            Vector3 a = Vector3.zero;
            Vector3 b = new Vector3(length, 0, 0);
            Vector3 c = new Vector3(length / 2f, Mathf.Sqrt(3f) / 2f * length, 0);
            float height = Mathf.Sqrt(2f / 3f) * length;
            Vector3 d = new Vector3(length / 2f, Mathf.Sqrt(3f) / 6f * length, height);

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idD = System.Guid.NewGuid().ToString();

            var segmentData = new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idD, Type = "Point", Position = d },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idA } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idD } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idD } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } }
            };

            var batch = new CreateShapeBatchAction(segmentData);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
