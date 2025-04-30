// --- 2) Lớp AngleConstraint ---

using System;
using UnityEngine;

namespace Manipulator
{
    public class AngleConstraint : Constraint
    {
        private Segment segA, segB;
        private Point pivot, freeA, freeB;
        private float angleRad;               // lưu góc bằng radian
        private Vector3 rotationAxis;         // trục xoay cố định

        /// <summary>
        /// Góc (độ) của constraint. Gán vào sẽ tự gọi Satisfy lại.
        /// </summary>
        public float Angle
        {
            get => angleRad * Mathf.Rad2Deg;
            set
            {
                angleRad = value * Mathf.Deg2Rad;
                UpdateSegmentB();  // giữ segA cố định, xoay segB theo góc mới
            }
        }

        /// <param name="segA">Segment thứ nhất</param>
        /// <param name="segB">Segment thứ hai</param>
        /// <param name="initialAngleDegrees">Góc ban đầu (độ)</param>
        public AngleConstraint(Segment segA, Segment segB, float initialAngleDegrees)
        {
            this.segA = segA;
            this.segB = segB;
            this.angleRad = initialAngleDegrees * Mathf.Deg2Rad;

            // 1) Tìm điểm chung (pivot) và hai điểm tự do
            if (segA.Start == segB.Start)
            {
                pivot = segA.Start; freeA = segA.End;   freeB = segB.End;
            }
            else if (segA.Start == segB.End)
            {
                pivot = segA.Start; freeA = segA.End;   freeB = segB.Start;
            }
            else if (segA.End   == segB.Start)
            {
                pivot = segA.End;   freeA = segA.Start; freeB = segB.End;
            }
            else if (segA.End   == segB.End)
            {
                pivot = segA.End;   freeA = segA.Start; freeB = segB.Start;
            }
            else
            {
                throw new ArgumentException("Hai segment phải nối với nhau tại một điểm chung.");
            }

            // 2) Xác định trục xoay (theo mặt phẳng chứa hai vector ban đầu)
            Vector3 vA = (freeA.Position - pivot.Position).normalized;
            Vector3 vB = (freeB.Position - pivot.Position).normalized;
            rotationAxis = Vector3.Cross(vA, vB).normalized;
            if (rotationAxis == Vector3.zero)
                rotationAxis = Vector3.forward; // mặc định nếu thẳng hàng

            // 3) Đăng ký shapes vào constraint và vào manager
            AddShape(pivot);
            AddShape(freeA);
            AddShape(freeB);
            ConstraintManager.Instance.RegisterConstraint(this);

            // 4) Thiết lập ban đầu để hai segment khớp đúng góc
            UpdateSegmentB();
        }

        /// <summary>
        /// Mỗi khi một Shape trong linkedShapes di chuyển, ConstraintManager sẽ gọi vào đây.
        /// movement là vector dịch chuyển của movedShape.
        /// </summary>
        public override void ApplyConstraint(Shape movedShape, Vector3 movement)
        {
            // a) Nếu di chuyển pivot: dịch cả hai đầu tự do theo để giữ nguyên hình học
            if (movedShape == pivot)
            {
                freeA.Position += movement;
                freeA.GO.transform.position += movement;
                segA.ApplyTransform(false);

                freeB.Position += movement;
                freeB.GO.transform.position += movement;
                segB.ApplyTransform(false);
            }
            // b) Nếu di chuyển freeA: xoay segB để giữ góc cố định
            else if (movedShape == freeA)
            {
                UpdateSegmentB();
            }
            // c) Nếu di chuyển freeB: xoay segA theo chiều ngược lại
            else if (movedShape == freeB)
            {
                UpdateSegmentA();
            }
        }

        // Hàm xoay segment B quanh pivot theo góc đã lưu
        private void UpdateSegmentB()
        {
            Vector3 dirA = (freeA.Position - pivot.Position);
            float lenB = (freeB.Position - pivot.Position).magnitude;
            if (lenB < Mathf.Epsilon) return;

            Quaternion q = Quaternion.AngleAxis(angleRad * Mathf.Rad2Deg, rotationAxis);
            Vector3 newDirB = q * dirA.normalized;
            Vector3 newPosB = pivot.Position + newDirB * lenB;

            freeB.Position = newPosB;
            freeB.GO.transform.position = newPosB;
            segB.ApplyTransform(false);
        }

        // Hàm xoay segment A khi freeB di chuyển
        private void UpdateSegmentA()
        {
            Vector3 dirB = (freeB.Position - pivot.Position);
            float lenA = (freeA.Position - pivot.Position).magnitude;
            if (lenA < Mathf.Epsilon) return;

            Quaternion q = Quaternion.AngleAxis(-angleRad * Mathf.Rad2Deg, rotationAxis);
            Vector3 newDirA = q * dirB.normalized;
            Vector3 newPosA = pivot.Position + newDirA * lenA;

            freeA.Position = newPosA;
            freeA.GO.transform.position = newPosA;
            segA.ApplyTransform(false);
        }
    }
}
