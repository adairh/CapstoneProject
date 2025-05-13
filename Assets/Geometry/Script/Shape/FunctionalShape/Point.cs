using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine; 
using Unity.Netcode;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

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
            if (IsHost)
                label.Value = GenerateNextLabel();

            // Add mesh
            var mf = gameObject.AddComponent<MeshFilter>();
            var mr = gameObject.AddComponent<MeshRenderer>();
            mf.mesh = MeshGenerator.CreateSphere(Radius);
            mr.material = DefaultMat;

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
            AutoConstraintManager.TryAutoAttachConstraint(this);

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

            //Debug.LogError($"[Point Move To] {newPosition}");
            
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

        
        private NetworkVariable<string> label = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private GameObject labelDisplay;

        public string GetLabel() => label.Value;

        public void SetLabel(string value)
        {
            if (IsServer)
                label.Value = value;
            else
                SubmitLabelRequestServerRpc(value);

            UpdateLabelDisplay(value);
        }

        private void UpdateLabelDisplay(string value)
        {
            if (labelDisplay == null)
            {
                var prefab = UIManager.Instance.GetUIComponent("LabelDisplayPrefab");
                labelDisplay = Instantiate(prefab, transform);
                labelDisplay.transform.localPosition = new Vector3(0, 0.4f, 0);
            }

            var text = labelDisplay.GetComponentInChildren<TextMeshPro>();
            if (text != null)
                text.text = value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            label.OnValueChanged += (oldVal, newVal) =>
            {
                UpdateLabelDisplay(newVal);
            };
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitLabelRequestServerRpc(string newLabel)
        {
            label.Value = newLabel;
        }

        private static int labelCounter = 0;

        private string GenerateNextLabel()
        {
            int n = labelCounter++;
            string label = "";
            do
            {
                label = (char)('A' + (n % 26)) + label;
                n = (n / 26) - 1;
            } while (n >= 0);
            return label;
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

        /*
        public void AttachProcess()
        {
            var mm = ManipulationManager.Instance;
            var shape = mm.GetPinnedShape();

            if (shape != null && shape != this && !(shape is Point))
            {
                constraint.AddDepend(this, shape);
            }
        }*/

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
                    
                    var v = new CreateShapeAction(data);
                    UndoRedoNetworkBridge.Instance.DoAndBroadcast(v);
                    
                    PerformDrawing.ResetMode();
                }
            }
        }


        public bool IsOnlyConnectedTo(Shape shape)
        {
            return ShapeStorage.GetAllShapes().OfType<Segment>()
                .Count(seg => seg.StartPoint == this || seg.EndPoint == this) == 1;
        }
        
        public override List<ISetting> GetSettings()
        {
            return new List<ISetting>(base.GetSettings())
            {
                new LabelSetting(LabelGenerator.Next(), this)
            }; 
        }


    }
    
    
    
}