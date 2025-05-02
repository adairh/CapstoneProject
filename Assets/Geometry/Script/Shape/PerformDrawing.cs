using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

namespace Manipulator
{
    public class PerformDrawing : NetworkBehaviour
    {

        public static PerformDrawing Instance;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject shapeNetworkPrefab;

        private static IShapeButton.ShapeType _currentShape = IShapeButton.ShapeType.None;
        private ShapeNetworkSync    _activeSync;
        private bool                _isDrawing;

        // Dùng để track tất cả Shape sinh ra trong 1 lần draw
        private List<string>        _tempShapeIds;
        private Action<string>      _onShapeAdded;

        void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            // chuẩn bị callback
            _tempShapeIds  = new List<string>();
            _onShapeAdded  = id => _tempShapeIds.Add(id);
        }

        void OnEnable()
        {
            Instance = this;
            ShapeButtonManager.OnShapeChanged += OnShapeButtonChanged;
        }

        void OnDisable()
        {
            ShapeButtonManager.OnShapeChanged -= OnShapeButtonChanged;
            // nếu thoát scene trong lúc vẽ thì vẫn phải cleanup
            if (_isDrawing) CancelDrawing();
        }

        public GameObject GetShapeNetwork() => shapeNetworkPrefab;
        
        private void OnShapeButtonChanged(IShapeButton.ShapeType newShape)
        {
            // nếu đang vẽ dở và user chọn tool khác, hủy triệt để
            if (_isDrawing && newShape != _currentShape)
                CancelDrawing();

            _currentShape = newShape;
        }

        void Update()
        {
            if (!IsOwner || _currentShape == IShapeButton.ShapeType.None) return;

            Vector3 hitPoint = ComputeHitPointUnderCursor();
            if (hitPoint == Vector3.zero) return;

            if (Input.GetMouseButtonDown(0))
                BeginDrawing(hitPoint);
            else if (Input.GetMouseButton(0) && _isDrawing)
                ContinueDrawing(hitPoint);
            else if (Input.GetMouseButtonUp(0) && _isDrawing)
                FinishDrawing(hitPoint);
        }

        private Vector3                  _beginPoint;      // nhớ start point
        private IShapeButton.ShapeType   _beginType;       // nhớ tool type
        private ulong                    _activeWrapperId;

        private void BeginDrawing(Vector3 start)
        {
            if (_isDrawing) CancelDrawing();

            
            _tempShapeIds.Clear();
            ShapeStorage.ShapeAdded += _onShapeAdded;
            Debug.Log("[UNDO DEBUG] Subscribed to ShapeStorage.ShapeAdded");

            _beginPoint   = start;
            _beginType    = _currentShape;

            var go = Instantiate(shapeNetworkPrefab);
            _activeSync = go.GetComponent<ShapeNetworkSync>();
            _activeSync.shapeType.Value    = (ShapeNetworkSync.ShapeType)_currentShape;
            _activeSync.startPoint.Value   = start;
            _activeSync.currentPoint.Value = start;
            _activeSync.isDrawing.Value    = true;
            _activeSync.isFinalized.Value  = false;
            _activeSync.ownerClientId.Value= NetworkManager.LocalClientId;

            var netObj = go.GetComponent<NetworkObject>();
            netObj.Spawn();
            _activeWrapperId = netObj.NetworkObjectId;

            _isDrawing = true;
            
            _onShapeAdded = id =>
            {
                Debug.Log($"[UNDO DEBUG] ShapeAdded event fired for ID: {id}");
                _tempShapeIds.Add(id);
            };

// then, after your yield return null in FinishAndTrackCoroutine, just before you unsubscribe:
            Debug.Log($"[UNDO DEBUG] About to unsubscribe.  Tracked IDs ({_tempShapeIds.Count}): {string.Join(", ", _tempShapeIds)}");

// also dump the entire storage at that moment:
            var allNames = ShapeStorage
                .GetAllShapes()
                .Select(s => s.Name)
                .OrderBy(n => n)
                .ToArray();
            Debug.Log($"[UNDO DEBUG] ShapeStorage currently contains ({allNames.Length}): {string.Join(", ", allNames)}");

            ShapeStorage.ShapeAdded -= _onShapeAdded;
            
            
        }
        
        private void ContinueDrawing(Vector3 current)
        {
            _activeSync.currentPoint.Value = current;
        }
        

        // Trong PerformDrawing.cs

        private void FinishDrawing(Vector3 end)
        {
            // 1) finalize network
            _activeSync.currentPoint.Value  = end;
            _activeSync.isDrawing.Value     = false;
            _activeSync.isFinalized.Value   = true;

            // 2) trì hoãn unsubscribe & ghi undo đến cuối frame
            StartCoroutine( FinishAndTrackCoroutine() );
        }

        private IEnumerator FinishAndTrackCoroutine()
        {
            // chờ 1 frame để ShapeNetworkSync.OnFinalizedChanged chạy xong,
            // EndSketch tạo xong hẳn Segment + Point
            yield return null;

            // giờ mới tạm dừng track
            ShapeStorage.ShapeAdded -= _onShapeAdded;

            // và push action
            var action = new CreateShapeBatchAction(
                _tempShapeIds,
                _activeWrapperId,
                _currentShape,
                _beginPoint,
                _activeSync.currentPoint.Value
            );
            UndoManager.Instance.Do(action);

            // reset state
            _isDrawing   = false;
            _activeSync  = null;
        }





        private void CancelDrawing()
        {
            // 1) Hủy network object
            if (_activeSync != null)
            {
                Destroy(_activeSync.gameObject);
                _activeSync = null;
            }

            // 2) Unsubscribe và dọn sạch tất cả Shape tạm
            ShapeStorage.ShapeAdded -= _onShapeAdded;
            foreach (var id in _tempShapeIds)
            {
                var s = ShapeStorage.GetShapeByID(id);
                if (s != null)
                {
                    s.Destroy();           // gọi Destroy() để xoá GameObject + từ ShapeStorage
                }
            }
            _tempShapeIds.Clear();

            _isDrawing = false;
        }

        private Vector3 ComputeHitPointUnderCursor()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
                return hit.point;

            var ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out var enter))
                return ray.GetPoint(enter);

            return Vector3.zero;
        }
        
        public static void ResetShape()
        {
            _currentShape = IShapeButton.ShapeType.None;
            ShapeButtonManager.SetActiveShape(IShapeButton.ShapeType.None);
        }
    }
}
