
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleDrawer : BaseButton
    {
        private CreateShapeBatchAction batch;

        protected override void OnButtonClick()
        {
            StartCoroutine(Draw());
        }

        private System.Collections.IEnumerator Draw()
        {
            UIHint.Show("Chọn điểm A");
            yield return ShapePicker.WaitForPoint();
            var a = ShapePicker.LastPicked as Point;
            if (a == null) yield break;
            string idA = a.ShapeId;

            UIHint.Show("Chọn điểm B");
            yield return ShapePicker.WaitForPoint();
            var b = ShapePicker.LastPicked as Point;
            if (b == null) yield break;
            string idB = b.ShapeId;

            Vector3 aPos = a.transform.position;
            Vector3 bPos = b.transform.position;
            Vector3 ab = bPos - aPos;
            float len = ab.magnitude;
            Vector3 dir = ab.normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
            float height = Mathf.Sqrt(3f) / 2f * len;
            Vector3 cPos = (aPos + bPos) / 2 + normal * height;

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
