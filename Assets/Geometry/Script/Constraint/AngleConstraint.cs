using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Manipulator
{
    /// <summary>
    /// MonoBehaviour constraint gắn lên một Point pivot để giữ góc cố định giữa
    /// hai Segment. Phản hồi khi kéo pivot, segment hoặc endpoint.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AngleConstraint : Constraint
    {
        [Header("Segments & Pivot")]
        [SerializeField] private Segment segmentA;
        [SerializeField] private Segment segmentB;
        [SerializeField] private Point pivot;


        public Segment GetA() => segmentA;
        public Segment GetB() => segmentB;
        public Point GetPivot() => pivot;
        
        
        [Header("Target Angle (°)")]
        [SerializeField] private float targetAngleDeg;

        private Point freeA, freeB;
        private Vector3 rotationAxis;
        private bool initialized;
        private bool applying;

        /// <summary>
        /// Cấu hình runtime nếu không dùng Inspector.
        /// </summary>

        public float GetAngle()
        {
            return targetAngleDeg;
        }

        public void SetAngle(float angle)
        {
            targetAngleDeg = angle;
        }
        
        public override ConstraintData Serialize() => new AngleConstraintData {
            PivotName      = pivot.Name,
            SegmentAName   = segmentA.Name,
            SegmentBName   = segmentB.Name,
            TargetAngleDeg = targetAngleDeg
        };
        
        public void AddDependencies(Segment segA, Segment segB, Point pivotPoint, float angleDeg)
        {
            segmentA = segA;
            segmentB = segB;
            pivot = pivotPoint;
            targetAngleDeg = angleDeg;
            InitializeInternal();
        }

        private void Start()
        {
            if (!initialized)
            {
                if (segmentA == null || segmentB == null || pivot == null)
                {
                    Debug.LogError("AngleConstraint: Assign segmentA, segmentB & pivot.", this);
                    enabled = false;
                    return;
                }
                InitializeInternal();
            }
        }
        
        private float _lenA, _lenB;
        
        private void InitializeInternal()
        {
            if (initialized) return;
            freeA = segmentA.GetOtherEndpoint(pivot);
            freeB = segmentB.GetOtherEndpoint(pivot);
    
            // Cố định khoảng cách ban đầu
            _lenA = (freeA.Position - pivot.Position).magnitude;
            _lenB = (freeB.Position - pivot.Position).magnitude;
            
            // Tính trục xoay
            Vector3 dirA = (freeA.Position - pivot.Position).normalized;
            Vector3 dirB = (freeB.Position - pivot.Position).normalized;
            rotationAxis = Vector3.Cross(dirA, dirB);
            if (rotationAxis.sqrMagnitude < Mathf.Epsilon)
                rotationAxis = Vector3.up;
            else
                rotationAxis.Normalize();

            // Đăng ký với ConstraintManager
            AddShape(pivot);
            AddShape(segmentA);
            AddShape(segmentB);
            AddShape(freeA);
            AddShape(freeB);
            ConstraintManager.Instance.RegisterConstraint(this);

            initialized = true;
        }
 

        public override void ApplyConstraint(Shape movedShape, Vector3 movement)
        {
            targetAngleDeg = Vector3.Angle(
                (segmentA.End.Position - segmentA.Start.Position),
                (segmentB.End.Position - segmentB.Start.Position)
            );
            
             Debug.LogError(((AngleConstraint)Holo.GetConstraint()).GetAngle());
             Holo.SetText();
         
        }


        private void MoveEndpointInternal(Point endpoint, Vector3 movement, Segment seg)
        {
            if (!ConstraintContext.TryBegin()) return;
            try
            {
                // 1) Cập nhật dữ liệu
                Vector3 newPos = endpoint.Position + movement;
                endpoint.Position = newPos;
                endpoint.GO.transform.position = newPos;
        
                // 2) Redraw segment
                seg.ApplyTransform(false, true);
            }
            finally
            {
                ConstraintContext.End();
            }
        }



        public void RotateOther(Segment moved, Segment other, bool reverseDelta)
        {
            Vector3 dirMoved = (moved.GetOtherEndpoint(pivot).Position - pivot.Position).normalized;
            Vector3 dirOther = (other.GetOtherEndpoint(pivot).Position - pivot.Position).normalized;

            float current = Vector3.SignedAngle(dirMoved, dirOther, rotationAxis);
            
            // Reverse góc nếu cần
            float target = reverseDelta ? -targetAngleDeg : targetAngleDeg;

// Góc hiện tại giữa hai vector
            float delta = target - current;

// Debug thông tin
            Debug.Log($"[AngleConstraint] target: {target}, current: {current}, delta: {delta}");

            if (delta > 180f) delta -= 360f;
            else if (delta < -180f) delta += 360f;


            // 👇 Chỉ đảo hướng xoay, không đảo current

            // Nếu delta quá nhỏ thì thôi
            if (Mathf.Abs(delta) < 0.01f) return;

            // Lấy free-point của “other”
            Point freePt = other.GetOtherEndpoint(pivot);

            // Chọn chiều dài tương ứng
            float len = (other == segmentA ? _lenA : _lenB);

            // Tạo direction chuẩn
            Vector3 unitDir = (freePt.Position - pivot.Position).normalized;

            // Xoay direction rồi scale lại
            Vector3 newDir = Quaternion.AngleAxis(delta, rotationAxis) * unitDir;
            Vector3 newPos = pivot.Position + newDir * len;

            freePt.MoveToPosition(newPos, /*silent=*/true);
            other.ApplyTransform(updatePoints: false, silent: true);

        }


        public override string GetLabelText()
        {
            return $"{GetAngle():F1}°";
        }

    }

    public static class SegmentExtensions
    {
        public static Point GetOtherEndpoint(this Segment seg, Point pivot)
        {
            if (seg.Start == pivot) return seg.End;
            if (seg.End == pivot) return seg.Start;
            throw new ArgumentException("Pivot isn't an endpoint of the segment.");
        }
    }
    
    class AngleConstraintSnapshot : IConstraintSnapshot
    {
        private readonly Segment _a, _b;
        private readonly Point   _pivot;
        private readonly float   _angle;

        // constructor chụp trạng thái:
        public AngleConstraintSnapshot(AngleConstraint c)
        {
            _a     = c.GetA();
            _b     = c.GetB();
            _pivot = c.GetPivot();
            _angle = c.GetAngle();
        }

        public void Remove()
        {
            Object.Destroy(_pivot.GO.GetComponent<AngleConstraint>());
        }

        public void Restore()
        {
            var c = _pivot.GO.AddComponent<AngleConstraint>();
            c.AddDependencies(_a, _b, _pivot, _angle);
            c.Owner = _pivot;
        }
    }
    
    
}
