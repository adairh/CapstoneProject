
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class TriangleDrawer : BaseButton
    {
        private Point pointA, pointB, pointC;
        private Segment segAB, segBC, segCA;
        private string idA, idB, idC, idAB, idBC, idCA;
        private CreateShapeBatchAction batch;

        protected override void OnButtonClick()
        {
            StartCoroutine(DrawTriangle());
        }

        private System.Collections.IEnumerator DrawTriangle()
        {
            // 1. Chờ chọn 2 điểm đầu bằng raycast
            UIHint.Show("Click điểm A");
            yield return ShapePicker.WaitForPoint();
            pointA = ShapePicker.LastPicked as Point;
            if (pointA == null) yield break;
            idA = pointA.ShapeId;

            UIHint.Show("Click điểm B");
            yield return ShapePicker.WaitForPoint();
            pointB = ShapePicker.LastPicked as Point;
            if (pointB == null) yield break;
            idB = pointB.ShapeId;

            // 2. Tự động tạo điểm C là đỉnh vuông góc từ trung điểm AB
            Vector3 mid = (pointA.transform.position + pointB.transform.position) / 2;
            Vector3 dir = (pointB.transform.position - pointA.transform.position).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
            Vector3 posC = mid + perp * Vector3.Distance(pointA.transform.position, pointB.transform.position) * 0.7f;

            idC = Guid.NewGuid().ToString();
            idAB = Guid.NewGuid().ToString();
            idBC = Guid.NewGuid().ToString();
            idCA = Guid.NewGuid().ToString();

            var dataList = new List<ShapeData>();

            var pointCData = new ShapeData
            {
                Id = idC,
                Type = "Point",
                Position = posC,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new(),
                Settings = new()
            };

            var segABData = new ShapeData
            {
                Id = idAB,
                Type = "Segment",
                ConnectedPoints = new List<string> { idA, idB },
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Scale = Vector3.one
            };
            var segBCData = new ShapeData
            {
                Id = idBC,
                Type = "Segment",
                ConnectedPoints = new List<string> { idB, idC },
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Scale = Vector3.one
            };
            var segCAData = new ShapeData
            {
                Id = idCA,
                Type = "Segment",
                ConnectedPoints = new List<string> { idC, idA },
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Scale = Vector3.one
            };

            dataList.Add(pointCData);
            dataList.Add(segABData);
            dataList.Add(segBCData);
            dataList.Add(segCAData);

            batch = new CreateShapeBatchAction(dataList);
            UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);

            UIHint.Hide();
        }
    }
}
