
using UnityEngine;

namespace Manipulator
{
    public class SquareSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Hình vuông", new[] { "Độ dài cạnh" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 1) return;
            float side = values[0];

            if (!PerformDrawing.RaycastMouse(out Vector3 origin)) return;

            Vector3 a = origin;
            Vector3 b = origin + new Vector3(side, 0, 0);
            Vector3 c = b + new Vector3(0, side, 0);
            Vector3 d = a + new Vector3(0, side, 0);

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idD = System.Guid.NewGuid().ToString();

            var batch = new CreateShapeBatchAction(new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idD, Type = "Point", Position = d },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idA } }
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
