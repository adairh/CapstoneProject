
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class IsoscelesTriangleDrawer : BaseButton
    {
        private CreateShapeBatchAction batch;

        protected override void OnButtonClick()
        {
            StartCoroutine(Draw());
        }

        private System.Collections.IEnumerator Draw()
        {
            UIHint.Show("Chọn điểm đáy A");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;
            string idA = a.ShapeId;

            UIHint.Show("Chọn điểm đáy B");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;
            string idB = b.ShapeId;

            Vector3 mid = (a.transform.position + b.transform.position) / 2;
            Vector3 dir = (b.transform.position - a.transform.position).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
            Vector3 cPos = mid + perp * Vector3.Distance(a.transform.position, b.transform.position) * 0.8f;

            string idC = Guid.NewGuid().ToString();
            string idAB = Guid.NewGuid().ToString();
            string idBC = Guid.NewGuid().ToString();
            string idCA = Guid.NewGuid().ToString();

            var dataList = new List<ShapeData>
            {
                new ShapeData { Id = idC, Type = "Point", Position = cPos },
                new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = idCA, Type = "Segment", ConnectedPoints = new() { idC, idA } }
            };

            batch = new CreateShapeBatchAction(dataList);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
            UIHint.Hide();
        }
    }
}
