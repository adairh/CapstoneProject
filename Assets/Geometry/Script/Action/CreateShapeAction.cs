using UnityEngine;

namespace Manipulator
{
    public class CreateShapeAction : IUndoableAction
    {
        public readonly ShapeData data;
        private Shape createdShape;
        private bool shapeWasAlreadyPresent;

        public CreateShapeAction(ShapeData shapeData)
        {
            data = shapeData;
        }

        public void Redo()
        {
            if (ShapeStorage.Contains(data.Id))
            {
                shapeWasAlreadyPresent = true;
                createdShape = ShapeStorage.GetById(data.Id);
                return;
            }

            shapeWasAlreadyPresent = false;
            //createdShape = ShapeFactory.CreateFromData(data);


            UndoRedoNetworkBridge.Instance.SpawnFromData(data, shape =>
            {
                createdShape = shape;

                if (shape is Point pt)
                    Segment.Drawer.OnStartPointReady(pt);
                else if (shape is Segment seg)
                    Segment.Drawer.OnSegmentReady(seg);
            });


            /*if (createdShape is Point pt)
            {
                Segment.Drawer.OnStartPointReady(pt);
            }
            else if (createdShape is Segment seg)
            {
                Segment.Drawer.OnSegmentReady(seg);
                Debug.LogError($"Start point {seg.StartPoint != null} ; End point {seg.EndPoint != null}");
            }*/
        }

        public void Undo()
        {
            if (shapeWasAlreadyPresent || createdShape == null)
                return;

            // Ensure it's not used elsewhere before deletion (e.g., shared snap point)
            if (createdShape is Point pt)
            {
                var referenceCount = 0;
                foreach (var shape in ShapeStorage.GetAllShapes())
                    if (shape is Segment seg && (seg.StartPoint == pt || seg.EndPoint == pt))
                        referenceCount++;

                if (referenceCount > 0)
                {
                    Debug.LogWarning($"[CreateShapeAction.Undo] Preventing deletion of shared point: {pt.ShapeId}");
                    return;
                }
            }

            createdShape.DestroyShape(); // Should deregister and destroy
            createdShape = null;
        }
    }
}