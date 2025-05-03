using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manipulator
{
    public static class ConstraintContext
    {
        // Đang trong một chu kỳ apply constraint chưa?
        public static bool IsApplying { get; private set; }

        // Queue giữ lại (Point → WorldPos) cần di chuyển
        private static readonly Dictionary<Point, Vector3> _queuedMoves
            = new Dictionary<Point, Vector3>();

        public static bool TryBegin()
        {
            if (IsApplying) return false;
            IsApplying = true;
            return true;
        }

        public static void End()
        {
            IsApplying = false;
            Flush();
        }

        // Queuing
        public static void QueueMove(Point pt, Vector3 newPos)
        {
            _queuedMoves[pt] = newPos;
        }

        // Sau khi hết tất cả ApplyConstraint, flush một lần
        private static void Flush()
        {
            // 1) Cập nhật tất cả Point.Position & Transform đồng loạt
            foreach (var kv in _queuedMoves)
            {
                var pt = kv.Key;
                var pos = kv.Value;
                pt.Position = pos;
                pt.GO.transform.position = pos;
            }

            // 2) Update lại mọi segment liên quan
            //    Giả sử bạn có thể lấy danh sách tất cả segment
            foreach (var seg in ShapeStorage.GetAllShapes().OfType<Segment>())
            {
                seg.ApplyTransform(updatePoints: false, silent: true);
            }

            _queuedMoves.Clear();
        }
    }



    public class ConstraintManager : MonoBehaviour
    {
        public static ConstraintManager Instance { get; private set; }
        private List<Constraint> constraints = new List<Constraint>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void RegisterConstraint(Constraint constraint)
        {
            if (!constraints.Contains(constraint))
            {
                constraints.Add(constraint);
            }
        }

        public void UnregisterConstraint(Constraint constraint)
        {
            if (constraints.Contains(constraint))
            {
                constraints.Remove(constraint);
            }
        }
        
        /// <summary>
        /// Khi một shape bị soft-delete, ta cũng cần unregister nó khỏi danh sách.
        /// </summary>
        public void UnregisterShape(Shape s)
        {
            foreach (var c in constraints.ToArray())
            {
                if (c.HasShape(s))
                    UnregisterConstraint(c);
            }
        }

        public void RegisterShape(Shape s)
        {
            // Nếu có constraint nào gắn trên s trước đó, bạn có thể gọi lại RegisterConstraint tương ứng.
            // Với ví dụ đơn giản này, chúng ta chỉ cho phép các constraint đã đăng ký trở lại.
        }
        

        // Bây giờ gọi ApplyConstraint với cả movedShape
        public void ApplyConstraints(Shape movedShape, Vector3 movement = new Vector3())
        {
            ManipulationManager mm = ManipulationManager.Instance;
            if (mm.IsDrawing()) return;
            foreach (var constraint in constraints)
            {
                if (constraint is not FixedPointConstraint)
                {
                    if (constraint.HasShape(movedShape))
                        constraint.ApplyConstraint(movedShape, movement);
                }
            }
        }

        public List<Constraint> GetAllConstraints()
        {
            return constraints;
        }
    }
}