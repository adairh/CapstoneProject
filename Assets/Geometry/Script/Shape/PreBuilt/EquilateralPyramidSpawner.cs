
using UnityEngine;

namespace Manipulator
{
    public class EquilateralPyramidSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Chóp đều", new[] { "Cạnh đáy" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 1) return;
            float side = values[0];
            if (!PerformDrawing.RaycastMouse(out Vector3 origin)) return;

            Vector3 a = origin;
            Vector3 b = a + new Vector3(side, 0, 0);
            Vector3 c = a + Quaternion.Euler(0, 0, 60) * new Vector3(side, 0, 0);
            Vector3 apex = (a + b + c) / 3f + Vector3.forward * side;

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idTop = System.Guid.NewGuid().ToString();

            var batch = new CreateShapeBatchAction(new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idTop, Type = "Point", Position = apex },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idA } },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idTop, idA } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idTop, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idTop, idC } },
            });

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
