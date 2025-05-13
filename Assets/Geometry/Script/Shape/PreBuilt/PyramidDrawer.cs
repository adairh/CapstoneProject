
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class PyramidDrawer : BaseButton
    {
        private CreateShapeBatchAction batch;

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

            Vector3 center = (a.transform.position + b.transform.position + c.transform.position) / 3f;
            Vector3 normal = Vector3.Cross(b.transform.position - a.transform.position, c.transform.position - a.transform.position).normalized;
            float baseSize = Vector3.Distance(a.transform.position, b.transform.position);
            Vector3 apex = center + normal * baseSize;

            string idD = Guid.NewGuid().ToString();

            var dataList = new List<ShapeData>
            {
                new ShapeData { Id = idD, Type = "Point", Position = apex },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idB } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idC } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idA } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idA, idD } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idB, idD } },
                new ShapeData { Id = Guid.NewGuid().ToString(), Type = "Segment", ConnectedPoints = new() { idC, idD } }
            };

            batch = new CreateShapeBatchAction(dataList);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
            UIHint.Hide();
        }
    }
}
