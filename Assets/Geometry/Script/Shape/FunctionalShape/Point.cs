using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Manipulator
{
    [RequireComponent(typeof(SphereCollider))]
    public class Point : Shape
    {
        private const float Radius = 0.1f;

        private SphereCollider collider;
        private FixedPointConstraint constraint;
        private NetworkPositionSync positionSync;

        #region INIT

        protected override void Awake()
        {
            base.Awake();

            // PATCH: Name is set during Initialize/Deserialize, not here

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

            ShapeStorage.Register(this);
            AutoConstraintManager.TryAutoAttachConstraint(this);
        }

        #endregion

        public event Action<Point> OnChanged;
        public event Action<Point> OnPositionChanged;

        public FixedPointConstraint GetPointConstraint()
        {
            return constraint;
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
                new LabelSetting(GetLabel(), this) // PATCH: Use label from data!
            };
        }

        public static class Drawer
        {
            public static void UpdatePointInput()
            {
                if (!NetworkManager.Singleton.IsHost) return;

                if (Input.GetMouseButtonDown(0))
                {
                    if (!PerformDrawing.RaycastMouse(out var pos, out var hitShape)) return;

                    // If hit a segment and NOT near any of its endpoints
                    if (hitShape is Segment segment)
                    {
                        float minDistToEndpoints = Mathf.Min(
                            Vector3.Distance(pos, segment.StartPoint.transform.position),
                            Vector3.Distance(pos, segment.EndPoint.transform.position)
                        );

                        const float endpointSnapDist = 0.15f; // Your endpoint snap threshold

                        if (minDistToEndpoints > endpointSnapDist)
                        {
                            // Place point and remember it should get a constraint!
                            var id = Guid.NewGuid().ToString();

                            // PATCH: Generate label here ONCE, and store in LogicalName
                            var label = LabelGenerator.Next();

                            var data = new ShapeData
                            {
                                Id = id,
                                Type = "Point",
                                LogicalName = label,
                                Position = pos,
                                Rotation = Quaternion.identity,
                                Scale = Vector3.one,
                                ConnectedPoints = new List<string>(),
                                Settings = new Dictionary<string, string>()
                            };
                            var v = new CreateShapeAction(data);
                            UndoRedoNetworkBridge.Instance.DoAndBroadcast(v);

                            // Wait for point creation, then attach constraint
                            EditorApplication.delayCall += () =>
                            {
                                var pt = ShapeStorage.GetById(id) as Point;
                                if (pt != null)
                                {
                                    // Attach the constraint
                                    var c = pt.gameObject.AddComponent<RelativePointConstraint>();
                                    c.Owner = pt;
                                    c.SetTarget(segment, RelativeTargetType.Segment, FindTOnSegment(segment, pos), 0, 0);
                                }
                            };
                            PerformDrawing.ResetMode();
                            return;
                        }
                    }

                    // Fallback: Normal point creation (not on segment body)
                    var id2 = Guid.NewGuid().ToString();
                    var label2 = LabelGenerator.Next();

                    var data2 = new ShapeData
                    {
                        Id = id2,
                        Type = "Point",
                        LogicalName = label2,
                        Position = pos,
                        Rotation = Quaternion.identity,
                        Scale = Vector3.one,
                        ConnectedPoints = new List<string>(),
                        Settings = new Dictionary<string, string>()
                    };
                    var v2 = new CreateShapeAction(data2);
                    UndoRedoNetworkBridge.Instance.DoAndBroadcast(v2);

                    PerformDrawing.ResetMode();
                }
            }

            static float FindTOnSegment(Segment seg, Vector3 pos)
            {
                var a = seg.StartPoint.transform.position;
                var b = seg.EndPoint.transform.position;
                var ab = b - a;
                var t = Vector3.Dot(pos - a, ab.normalized) / ab.magnitude;
                return Mathf.Clamp01(t);
            }
        }

        #region MOVE

        /// <summary>
        ///     Di chuyển point đến vị trí mới. Nếu silent = true, không gửi sự kiện NotifyChanged.
        /// </summary>
        public override void MoveTo(Vector3 newPosition, bool silent = false, bool queue = true)
        {
            if (transform.position == newPosition) return;

            var oldPosition = transform.position;
            transform.position = newPosition;

            var delta = newPosition - oldPosition;

            OnPositionChanged?.Invoke(this);

            if (!silent && IsHost && TryGetComponent<NetworkPositionSync>(out var sync))
                sync.syncedPosition.Value = newPosition;

            if (!silent)
            {
                UndoRedoNetworkBridge.Instance.DoAndBroadcast(new MoveShapeAction(ShapeId, oldPosition, newPosition),
                    queue);
                NotifyChanged();
            }
        }

        private readonly NetworkVariable<string> label = new("");

        private GameObject labelDisplay;

        public string GetLabel()
        {
            // PATCH: Always return LogicalName
            return Data != null && !string.IsNullOrEmpty(Data.LogicalName) ? Data.LogicalName : label.Value;
        }

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

            var text = labelDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            label.OnValueChanged += (oldVal, newVal) => { UpdateLabelDisplay(newVal); };

            // PATCH: Here, ensure the label is synced after the object is network-registered
            if (!string.IsNullOrEmpty(Data?.LogicalName))
            {
                if (IsServer)
                    label.Value = Data.LogicalName; // triggers UI update for everyone
                else
                    UpdateLabelDisplay(Data.LogicalName); // for the local user until RPC syncs
            }
        }


        [ServerRpc(RequireOwnership = false)]
        private void SubmitLabelRequestServerRpc(string newLabel)
        {
            label.Value = newLabel;
        }

        public Vector3 GetCurrentPosition()
        {
            return transform.position;
        }

        #endregion

        #region SERIALIZATION

        public override ShapeData Serialize()
        {
            var data = base.Serialize();
            data.Type = "Point";
            // PATCH: Always keep LogicalName!
            data.LogicalName = Data.LogicalName;
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);

            // PATCH: DO NOT call SetLabel() here. Instead, set local state only.
            if (!string.IsNullOrEmpty(data.LogicalName))
            {
                // Set internal label variable; do not call ServerRpc yet!
                if (IsServer)
                    label.Value = data.LogicalName;
                // Always update UI locally for the spawned object
                name = data.LogicalName;
                UpdateLabelDisplay(data.LogicalName);
            }
        }


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
    }
}
