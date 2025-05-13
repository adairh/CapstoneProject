
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class CylinderSquareDrawer : BaseButton
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

            Vector3 ab = b.transform.position - a.transform.position;
            float length = ab.magnitude;
            Vector3 dir = ab.normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;

            Vector3 c = b.transform.position + normal * length;
            Vector3 d = a.transform.position + normal * length;

            Vector3 offset = Vector3.forward * length;

            Vector3 a2 = a.transform.position + offset;
            Vector3 b2 = b.transform.position + offset;
            Vector3 c2 = c + offset;
            Vector3 d2 = d + offset;

            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idA2 = Guid.NewGuid().ToString();
            string idB2 = Guid.NewGuid().ToString();
            string idC2 = Guid.NewGuid().ToString();
            string idD2 = Guid.NewGuid().ToString();

            var datas = new List<ShapeData>
            {
                new ShapeData { Id = idC, Type = "Point", Position = c },
                new ShapeData { Id = idD, Type = "Point", Position = d },
                new ShapeData { Id = idA2, Type = "Point", Position = a2 },
                new ShapeData { Id = idB2, Type = "Point", Position = b2 },
                new ShapeData { Id = idC2, Type = "Point", Position = c2 },
                new ShapeData { Id = idD2, Type = "Point", Position = d2 },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idA } },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA2, idB2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB2, idC2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC2, idD2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD2, idA2 } },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idA2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idB2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idC2 } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idD2 } }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(datas));
            UIHint.Hide();
        }
    }
}
