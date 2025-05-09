using System;
using UnityEngine; 
using Unity.Netcode;

namespace Manipulator
{
    [RequireComponent(typeof(SphereCollider))]
    public class Point : Shape
    {
        private const float Radius = 0.1f;

        private SphereCollider collider;
        private FixedPointConstraint constraint;
        private NetworkPositionSync positionSync;

        public event System.Action<Point> OnChanged;
        public event Action<Point> OnPositionChanged;

        #region INIT

        protected override void Awake()
        {
            base.Awake();

            name = $"Point_{ShapeId}";
 
            // Add mesh
            var mf = gameObject.AddComponent<MeshFilter>();
            var mr = gameObject.AddComponent<MeshRenderer>();
            mf.mesh = MeshGenerator.CreateSphere(Radius);
            mr.material = MaterialLibrary.Get(MaterialType.Default);

            // Collider
            collider = GetComponent<SphereCollider>();
            collider.radius = Radius;
            collider.center = Vector3.zero;

            // Constraint
            constraint = gameObject.AddComponent<FixedPointConstraint>();
            constraint.Owner = this; // Register sẽ được gọi từ OnEnable()

            // Network sync
            if (!TryGetComponent(out positionSync))
                positionSync = gameObject.AddComponent<NetworkPositionSync>();

            // Settings (position)
            //AppendSettings(new PositionSetting(transform.position, this));
            ShapeStorage.Register(this);
        }

        #endregion

        #region MOVE

        /// <summary>
        /// Di chuyển point đến vị trí mới. Nếu silent = true, không gửi sự kiện NotifyChanged.
        /// </summary>
        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            if (transform.position == newPosition) return;

            Vector3 oldPosition = transform.position;
            transform.position = newPosition;
            
            Vector3 delta = newPosition - oldPosition;
// ✅ Apply constraint với delta đúng
            // constraint.ApplyConstraint(this, delta);
            // ConstraintManager.Instance.ApplyConstraints(this, delta);
            
            // Gửi sync vị trí nếu là host

            Debug.LogError($"[Point Move To] {newPosition}");
            
            OnPositionChanged?.Invoke(this);


            if (!silent && IsHost && TryGetComponent<NetworkPositionSync>(out var sync))
            {
                sync.syncedPosition.Value = newPosition;
            }

            // Ghi undo và thông báo thay đổi
            if (!silent)
            {
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(new MoveShapeAction(ShapeId, oldPosition, newPosition), queue);
                NotifyChanged();
            }
        }


        public Vector3 GetCurrentPosition() => transform.position;

        #endregion

        #region SERIALIZATION

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Point";
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);
        }

        #endregion

        #region ATTACH

        public void AttachProcess()
        {
            var mm = ManipulationManager.Instance;
            var shape = mm.GetPinnedShape();

            if (shape != null && shape != this && !(shape is Point))
            {
                constraint.AddDepend(this, shape);
            }
        }

        public FixedPointConstraint GetPointConstraint() => constraint;

        #endregion

        #region OVERRIDES

        public override void CompleteDraw()
        {
            base.CompleteDraw();
            UpdateHitbox();
        }

        public override void UpdateHitbox()
        {
            if (collider == null)
                collider = GetComponent<SphereCollider>();
        }

        public override void NotifyChanged(bool silent = false)
        {
            base.NotifyChanged(silent);
            if (!silent)
                OnChanged?.Invoke(this);
        }

        #endregion
        
        
        
        public static class Drawer
        {
            public static void UpdatePointInput()
            {
                if (!NetworkManager.Singleton.IsHost) return;

                if (Input.GetMouseButtonDown(0))
                {
                    if (!PerformDrawing.RaycastMouse(out Vector3 pos)) return;

                    string id = Guid.NewGuid().ToString();
                    var data = new ShapeData
                    {
                        Id = id,
                        Type = "Point",
                        Position = pos,
                        Rotation = Quaternion.identity,
                        Scale = Vector3.one,
                        ConnectedPoints = new(),
                        Settings = new()
                    };
                    NetworkShapeSpawner.Instance.CreateShapeNetworked(data, out Shape p);

                    PerformDrawing.ResetMode();
                }
            }
        }
        
        
    }
    
    
    
}