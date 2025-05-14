
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class EquilateralTriangleDrawer : IPrebuiltDrawer
{
    private string idA, idB, idC, idAB, idBC, idCA;

    private Point a, b, c;
    private Segment ab, bc, ca;

    private float baseLength;

    public void Begin(Vector3 startPos)
    {
        idA = Guid.NewGuid().ToString();
        idB = Guid.NewGuid().ToString();
        idC = Guid.NewGuid().ToString();

        idAB = Guid.NewGuid().ToString();
        idBC = Guid.NewGuid().ToString();
        idCA = Guid.NewGuid().ToString();

        var datas = new List<ShapeData>
        {
            new ShapeData { Id = idA, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
            new ShapeData { Id = idB, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },
            new ShapeData { Id = idC, Type = "Point", Position = startPos, Rotation = Quaternion.identity, Scale = Vector3.one },

            new ShapeData { Id = idAB, Type = "Segment", ConnectedPoints = new() { idA, idB }},
            new ShapeData { Id = idBC, Type = "Segment", ConnectedPoints = new() { idB, idC }},
            new ShapeData { Id = idCA, Type = "Segment", ConnectedPoints = new() { idC, idA }},
        };

        var batch = new CreateShapeBatchAction(datas);

        batch.OnShapeSpawned = shape =>
        {
            if (shape is Point pt)
            {
                if (pt.ShapeId == idA) a = pt;
                if (pt.ShapeId == idB) b = pt;
                if (pt.ShapeId == idC) c = pt;
            }

            if (shape is Segment s)
            {
                if (s.ShapeId == idAB) ab = s;
                if (s.ShapeId == idBC) bc = s;
                if (s.ShapeId == idCA) ca = s;
            }

            TryConnectSegments();
        };

        UndoRedoNetworkBridge.Instance.DoAndBroadcast(batch);
    }

    private void TryConnectSegments()
    {
        if (a != null && b != null && c != null && ab != null && bc != null && ca != null)
        {
            a.SetRaycastIgnore(true); b.SetRaycastIgnore(true); c.SetRaycastIgnore(true);
            ab.SetRaycastIgnore(true); bc.SetRaycastIgnore(true); ca.SetRaycastIgnore(true);
            ab.MarkAsPreview(); bc.MarkAsPreview(); ca.MarkAsPreview();

            ab.SetStartPoint(a); ab.SetEndPoint(b);
            bc.SetStartPoint(b); bc.SetEndPoint(c);
            ca.SetStartPoint(c); ca.SetEndPoint(a);
        }
    }

    public void Working(Vector3 currentPos)
    {
        if (a == null || b == null || c == null) return;

        b.MoveTo(currentPos, queue: false);

        Vector3 dir = (b.transform.position - a.transform.position).normalized;
        baseLength = Vector3.Distance(a.transform.position, b.transform.position);

        Vector3 right = Vector3.Cross(dir, Vector3.forward); // Mặt phẳng XY
        float height = Mathf.Sqrt(3f) / 2f * baseLength;
        Vector3 midpoint = (a.transform.position + b.transform.position) / 2f;
        Vector3 cPos = midpoint + right * height;

        c.MoveTo(cPos, queue: false);
    }

    public void End(Vector3 finalPos)
    {
        a.SetRaycastIgnore(false);
        b.SetRaycastIgnore(false);
        c.SetRaycastIgnore(false);
        ab.SetRaycastIgnore(false);
        bc.SetRaycastIgnore(false);
        ca.SetRaycastIgnore(false); 
    }

    public void Cancel()
    {
        a?.DestroyShape(); b?.DestroyShape(); c?.DestroyShape();
        ab?.DestroyShape(); bc?.DestroyShape(); ca?.DestroyShape();
    }
}

}
