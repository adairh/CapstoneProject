
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RectangleDrawer : BaseButton
    {
        private CreateShapeBatchAction batch;

        protected override void OnButtonClick()
        {
            StartCoroutine(Draw());
        }

        private System.Collections.IEnumerator Draw()
        {
            UIHint.Show("Chọn điểm A (góc trái dưới)");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;
            string idA = a.ShapeId;

            UIHint.Show("Chọn điểm B (góc phải dưới)");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;
            string idB = b.ShapeId;

            Vector3 ab = b.transform.position - a.transform.position;
            float length = ab.magnitude;
            Vector3 dir = ab.normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
            Vector3 dPos = a.transform.position + normal * (length * 0.6f);
            Vector3 cPos = b.transform.position + normal * (length * 0.6f);

            string idC = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();
            string idAB = Guid.NewGuid().ToString();
            string idBC = Guid.NewGuid().ToString();
            string idCD = Guid.NewGuid().ToString();
            string idDA = Guid.NewGuid().ToString();

            var dataList = new List<ShapeData>
            {
                new ShapeData { Id = idC, Type = "Point", Position = cPos },
                new ShapeData { Id = idD, Type = "Point", Position = dPos },
                new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = idCD, Type = "Segment", ConnectedPoints = new() { idC, idD } },
                new ShapeData { Id = idDA, Type = "Segment", ConnectedPoints = new() { idD, idA } }
            };

            batch = new CreateShapeBatchAction(dataList);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
            UIHint.Hide();
        }
    }
}
