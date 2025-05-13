
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class CylinderTriangleDrawer : BaseButton
    {
        protected override void OnButtonClick()
        {
            StartCoroutine(Draw());
        }

        private IEnumerator Draw()
        {
            UIHint.Show("Chọn điểm A (đáy)");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;
            string idA = a.ShapeId;

            UIHint.Show("Chọn điểm B (đáy)");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;
            string idB = b.ShapeId;

            UIHint.Show("Chọn điểm C (đáy)");
            yield return ShapePicker.WaitForPoint();
            var c = ShapePicker.LastPicked as Point;
            if (c == null) yield break;
            string idC = c.ShapeId;

            float height = Vector3.Distance(a.transform.position, b.transform.position);
            Vector3 offset = Vector3.forward * height;

            Vector3 a2 = a.transform.position + offset;
            Vector3 b2 = b.transform.position + offset;
            Vector3 c2 = c.transform.position + offset;

            string idA2 = Guid.NewGuid().ToString();
            string idB2 = Guid.NewGuid().ToString();
            string idC2 = Guid.NewGuid().ToString();

            var datas = new List<ShapeData>
            {
                new ShapeData { Id = idA2, Type = "Point", Position = a2 },
                new ShapeData { Id = idB2, Type = "Point", Position = b2 },
                new ShapeData { Id = idC2, Type = "Point", Position = c2 },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idA } },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA2, idB2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB2, idC2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC2, idA2 } },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idA2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idB2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idC2 } }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(datas));
            UIHint.Hide();
        }
    }
}
