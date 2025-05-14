
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Tam giác đều", new[] { "Độ dài cạnh" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 1) return;
            float side = values[0];

            if (!PerformDrawing.RaycastMouse(out Vector3 origin)) return;

            Vector3 a = origin;
            Vector3 b = origin + new Vector3(side, 0, 0);
            float height = Mathf.Sqrt(3f) / 2f * side;
            Vector3 c = (a + b) / 2f + Vector3.up * height;

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();

            var batch = new CreateShapeBatchAction(new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idA } }
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
