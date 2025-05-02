using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    [RequireComponent(typeof(Collider))]
    public class FixedPointConstraint : Constraint
    {
        // đây là Point bạn gắn component này lên
        private Point _point;

        // mỗi Shape (ví dụ Segment) → RatioCalculator
        private readonly Dictionary<Shape, RatioCalculator> _constraints
            = new Dictionary<Shape, RatioCalculator>();

        void OnDestroy()
        {
            // Hủy đăng ký để tránh leak
            foreach (var kv in _constraints)
                kv.Key.OnChanged -= OnDependentShapeChanged;
        }

        /// <summary>
        /// Gọi khi bạn muốn point này phụ thuộc vào 1 Shape.
        /// </summary>
        public void AddDepend(Point pt, Shape shape)
        {
            _point = pt;                         // gán luôn Point
            if (_constraints.ContainsKey(shape)) 
                return;

            // tạo calculator dựa trên pivot của shape
            _constraints[shape] = new RatioCalculator(_point, shape.GetPivots());

            // subscribe event OnChanged của shape
            shape.OnChanged += OnDependentShapeChanged;
            
            AddShape(shape);
            //   point di chuyển thì ApplyConstraint cũng sẽ được gọi
            AddShape(pt);
            
        }

        /// <summary>
        /// khi shape.NotifyChange() được gọi
        /// </summary>
        
        private bool _isApplying;
        
        private void OnDependentShapeChanged(Shape movedShape)
        {
            if (_isApplying || !ConstraintContext.TryBegin()) return;

            if (!_constraints.TryGetValue(movedShape, out var ratio)) 
            {
                ConstraintContext.End();
                return;
            }

            _isApplying = true;
            try
            {
                ratio.RecalculatePosition();
            }
            finally
            {
                _isApplying = false;
                ConstraintContext.End();
            }
        }


        #region — RatioCalculator —
        private class RatioCalculator
        {
            private readonly Point _pt;
            private readonly List<Point> _pivots;
            private readonly Dictionary<Point,(Vector3 dir, float dist)> _data
                = new Dictionary<Point,(Vector3, float)>();

            public RatioCalculator(Point pt, IEnumerable<Point> pivots)
            {
                _pt = pt;
                _pivots = new List<Point>(pivots);
                foreach (var p in _pivots)
                {
                    var dir  = (_pt.Position - p.Position).normalized;
                    var dist = Vector3.Distance(_pt.Position, p.Position);
                    _data[p]  = (dir, dist);
                }
            }

            public void RecalculatePosition()
            {
                if (_pivots.Count == 0) return;
                Vector3 sum = Vector3.zero;
                foreach (var kv in _data)
                {
                    var pivot = kv.Key;
                    var (dir, dist) = kv.Value;
                    sum += pivot.Position + dir * dist;
                }
                // move point
                //_pt.MoveToPosition(sum / _pivots.Count, true);
                ConstraintContext.QueueMove(_pt, sum / _pivots.Count);

            }
        }
        #endregion

        public override void ApplyConstraint(Shape movedShape, Vector3 movement = new Vector3())
        {
            
            foreach (var shape in GetLinkedShapes())
            {
                shape.OnPointMoved((Point)Owner);
            }

        }
    }
}
