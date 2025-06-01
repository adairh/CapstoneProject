using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Manipulator
{
    [RequireComponent(typeof(BoxCollider))]
    public class Point : Shape
    {
        private const float Side = 0.1f; // Adjusted to roughly match original sphere's volume

        private BoxCollider collider;
        private FixedPointConstraint constraint;
        private NetworkPositionSync positionSync;

        #region INIT

        protected override void Awake()
        {
            base.Awake();

            // Add mesh (Cube instead of Sphere)
            var mf = gameObject.AddComponent<MeshFilter>();
            var mr = gameObject.AddComponent<MeshRenderer>();
            mf.mesh = MeshGenerator.CreateCube(Side); // You need to have a method to create a cube mesh
            mr.material = DefaultMat;

            // Collider (Box)
            collider = GetComponent<BoxCollider>();
            collider.size = Vector3.one * Side * 5;
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

                        const float endpointSnapDist = 0.15f;

                        if (minDistToEndpoints > endpointSnapDist)
                        {
                            var id = Guid.NewGuid().ToString();
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

                            /*EditorApplication.delayCall += () =>
                            {*/
                                var pt = ShapeStorage.GetById(id) as Point;
                                if (pt != null)
                                {
                                    var c = pt.gameObject.AddComponent<RelativePointConstraint>();
                                    c.Owner = pt;
                                    c.SetTarget(segment, RelativeTargetType.Segment, FindTOnSegment(segment, pos), 0, 0);
                                }
                            /*};*/
                            PerformDrawing.ResetMode();
                            return;
                        }
                    }

                    // Fallback: Normal point creation
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
                labelDisplay.transform.localPosition = new Vector3(0, Side * 0.7f, 0); // Slightly above the cube
            }

            var text = labelDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            label.OnValueChanged += (oldVal, newVal) => { UpdateLabelDisplay(newVal); };

            if (!string.IsNullOrEmpty(Data?.LogicalName))
            {
                if (IsServer)
                    label.Value = Data.LogicalName;
                else
                    UpdateLabelDisplay(Data.LogicalName);
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
            data.LogicalName = Data.LogicalName;
            return data;
        }

        public override void Deserialize(ShapeData data)
        {
            base.Deserialize(data);

            if (!string.IsNullOrEmpty(data.LogicalName))
            {
                if (IsServer)
                    label.Value = data.LogicalName;
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
                collider = GetComponent<BoxCollider>();
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
