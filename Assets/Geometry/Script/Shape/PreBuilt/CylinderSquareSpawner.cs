
using UnityEngine;

namespace Manipulator
{
    public class CylinderSquareSpawner : BaseButton
    {
        protected override void OnButtonClick()
        {
            PrebuiltSpawnPanel.Show("Trụ đứng (đáy vuông)", new string[] { "Cạnh đáy", "Chiều cao" }, OnConfirm);
        }

        private void OnConfirm(float[] values)
        {
            if (values.Length < 2) return;
            float side = values[0];
            float height = values[1];

            Vector3 a = Vector3.zero;
            Vector3 b = new Vector3(side, 0, 0);
            Vector3 c = new Vector3(side, side, 0);
            Vector3 d = new Vector3(0, side, 0);

            Vector3 offset = new Vector3(0, 0, height);
            Vector3 a2 = a + offset;
            Vector3 b2 = b + offset;
            Vector3 c2 = c + offset;
            Vector3 d2 = d + offset;

            string idA = System.Guid.NewGuid().ToString();
            string idB = System.Guid.NewGuid().ToString();
            string idC = System.Guid.NewGuid().ToString();
            string idD = System.Guid.NewGuid().ToString();
            string idA2 = System.Guid.NewGuid().ToString();
            string idB2 = System.Guid.NewGuid().ToString();
            string idC2 = System.Guid.NewGuid().ToString();
            string idD2 = System.Guid.NewGuid().ToString();

            var datas = new System.Collections.Generic.List<ShapeData>
            {
                new ShapeData { Id = idA, Type = "Point", Position = a },
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idD, Type = "Point", Position = d },
                new ShapeData { Id = idA2, Type = "Point", Position = a2 },
                new ShapeData { Id = idB2, Type = "Point", Position = b2 },
                new ShapeData { Id = idC2, Type = "Point", Position = c2 },
                new ShapeData { Id = idD2, Type = "Point", Position = d2 },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idA } },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA2, idB2 } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB2, idC2 } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC2, idD2 } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD2, idA2 } },

                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idA2 } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idB2 } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idC2 } },
                new ShapeData { Id = System.Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idD2 } }
            };

            var batch = new CreateShapeBatchAction(datas);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
        }
    }
}
