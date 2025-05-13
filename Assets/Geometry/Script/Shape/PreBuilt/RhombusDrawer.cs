
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class RhombusDrawer : BaseButton
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

            UIHint.Show("Chọn điểm C (đối đỉnh A)");
            yield return ShapePicker.WaitForPoint();
            var c = ShapePicker.LastPicked as Point;
            if (c == null) yield break;
            string idC = c.ShapeId;

            Vector3 center = (a.transform.position + c.transform.position) / 2;
            Vector3 ac = c.transform.position - a.transform.position;
            Vector3 perp = Vector3.Cross(ac.normalized, Vector3.forward).normalized;
            float half = ac.magnitude / 2;

            Vector3 b = center + perp * half;
            Vector3 d = center - perp * half;

            string idB = Guid.NewGuid().ToString();
            string idD = Guid.NewGuid().ToString();

            string idAB = Guid.NewGuid().ToString();
            string idBC = Guid.NewGuid().ToString();
            string idCD = Guid.NewGuid().ToString();
            string idDA = Guid.NewGuid().ToString();

            var dataList = new List<ShapeData>
            {
                new ShapeData { Id = idB, Type = "Point", Position = b },
                new ShapeData { Id = idD, Type = "Point", Position = d },
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
