
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class PyramidSquareDrawer : BaseButton
    {
        protected override void OnButtonClick()
        {
            StartCoroutine(Draw());
        }

        private IEnumerator Draw()
        {
            UIHint.Show("Chọn điểm A (góc trái đáy vuông)");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;
            string idA = a.ShapeId;

            UIHint.Show("Chọn điểm B (góc phải đáy vuông)");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;
            string idB = b.ShapeId;

            Vector3 ab = b.transform.position - a.transform.position;
            float length = ab.magnitude;
            Vector3 dir = ab.normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;

            Vector3 dPos = a.transform.position + normal * length;
            Vector3 cPos = b.transform.position + normal * length;

            Vector3 center = (a.transform.position + b.transform.position + cPos + dPos) / 4f;
            Vector3 apex = center + Vector3.forward * length;

            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idE = Guid.NewGuid().ToString(); // apex

            var datas = new List<ShapeData>
            {
                new ShapeData { Id = idC, Type = "Point", Position = cPos },
                new ShapeData { Id = idD, Type = "Point", Position = dPos },
                new ShapeData { Id = idE, Type = "Point", Position = apex },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idA } },

                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idE } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idE } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idE } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idD, idE } }
            };

            UndoRedoNetworkBridge.Instance.DoAndBroadcast(new CreateShapeBatchAction(datas));
            UIHint.Hide();
        }
    }
}
