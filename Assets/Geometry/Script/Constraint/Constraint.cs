using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    /// <summary>
    /// Base class cho tất cả constraint. Mỗi constraint nên kế thừa từ đây.
    /// </summary>
    public abstract class Constraint : MonoBehaviour
    {
        public string ConstraintId { get; private set; }

        protected virtual void Awake()
        {
            ConstraintId = Guid.NewGuid().ToString();
        }

        protected virtual void OnEnable()
        {
            if (ConstraintManager.Instance != null)
                ConstraintManager.Instance.RegisterConstraint(this);
        }

        protected virtual void OnDisable()
        {
            if (ConstraintManager.Instance != null)
                ConstraintManager.Instance.UnregisterConstraint(this);
        }

        /// <summary>
        /// Xác định constraint có liên quan tới shape này không.
        /// </summary>
        public abstract bool HasShape(Shape shape);

        /// <summary>
        /// Gọi khi có shape di chuyển hoặc thay đổi.
        /// </summary>
        public abstract void ApplyConstraint(Shape changedShape, Vector3 delta);

        /// <summary>
        /// Serialize constraint về dạng dữ liệu lưu trữ được.
        /// </summary>
        public abstract ConstraintData Serialize();

        /// <summary>
        /// Xóa toàn bộ sự kiện và liên kết.
        /// </summary>
        public virtual void Cleanup() { }

        /// <summary>
        /// Danh sách các Shape liên quan.
        /// </summary>
        public abstract IEnumerable<Shape> GetRelatedShapes();
    }
 
} 
