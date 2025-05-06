using UnityEngine; 

namespace Manipulator
{
    public class PerformDrawing : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        void Start() {
            if (cam == null)
                cam = Camera.main;
        }

        private Point currentStartPoint;
        private Segment currentPreviewSegment;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                TryStartDrawing();

            if (currentPreviewSegment != null)
                UpdatePreviewSegment();

            if (Input.GetMouseButtonUp(0))
                FinishDrawing();
        }

        void TryStartDrawing()
        {
            if (!RaycastMouse(out Vector3 pos)) return;

            if (!RaycastPoint(out Point existing))
            {
                var pointData = new ShapeData
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new System.Collections.Generic.List<string>(),
                    Settings = new System.Collections.Generic.Dictionary<string, string>()
                };

                NetworkShapeSpawner.Instance.CreateShapeNetworked(pointData);
                existing = ShapeStorage.GetById(pointData.Id) as Point; // may not resolve immediately on client
            }

            currentStartPoint = existing;

            var segData = new ShapeData
            {
                Id = System.Guid.NewGuid().ToString(),
                Type = "Segment",
                Position = pos,
                Rotation = Quaternion.identity,
                Scale = Vector3.one,
                ConnectedPoints = new System.Collections.Generic.List<string> { existing.ShapeId, existing.ShapeId },
                Settings = new System.Collections.Generic.Dictionary<string, string>()
            };

            NetworkShapeSpawner.Instance.CreateShapeNetworked(segData);
            currentPreviewSegment = ShapeStorage.GetById(segData.Id) as Segment;
        }

        void UpdatePreviewSegment()
        {
            if (!RaycastMouse(out Vector3 pos)) return;
            currentPreviewSegment.EndPoint.MoveTo(pos);
        }

        void FinishDrawing()
        {
            if (currentStartPoint == null || currentPreviewSegment == null)
                return;

            Point end;
            if (!RaycastPoint(out end))
            {
                if (!RaycastMouse(out Vector3 pos)) return;

                var endPointData = new ShapeData
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Type = "Point",
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    ConnectedPoints = new System.Collections.Generic.List<string>(),
                    Settings = new System.Collections.Generic.Dictionary<string, string>()
                };

                NetworkShapeSpawner.Instance.CreateShapeNetworked(endPointData);
                end = ShapeStorage.GetById(endPointData.Id) as Point;
            }

            currentPreviewSegment.SetEndpoints(currentStartPoint, end);

            currentStartPoint = null;
            currentPreviewSegment = null;
        }

        bool RaycastMouse(out Vector3 pos)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                pos = hit.point;
                return true;
            }

            pos = Vector3.zero;
            return false;
        }

        bool RaycastPoint(out Point point)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                point = hit.collider.GetComponent<Point>();
                return point != null;
            }

            point = null;
            return false;
        }
    }
}
