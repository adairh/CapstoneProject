using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

namespace Manipulator
{
    public class PerformDrawing : NetworkBehaviour
    {
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
            ShapeButtonManager.OnShapeChanged += OnShapeButtonChanged;
        }

        void OnDisable()
        {
            ShapeButtonManager.OnShapeChanged -= OnShapeButtonChanged;
            // nếu thoát scene trong lúc vẽ thì vẫn phải cleanup
            if (_isDrawing) CancelDrawing();
        }

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

        private void BeginDrawing(Vector3 start)
        {
            // nếu có draw dang dở thì dọn luôn trước
            if (_isDrawing) CancelDrawing();

            // bắt đầu track các Shape mới sinh
            _tempShapeIds.Clear();
            ShapeStorage.ShapeAdded += _onShapeAdded;

            // tạo network‐wrapper
            var go = Instantiate(shapeNetworkPrefab);
            _activeSync = go.GetComponent<ShapeNetworkSync>();
            _activeSync.shapeType.Value    = (ShapeNetworkSync.ShapeType)_currentShape;
            _activeSync.startPoint.Value   = start;
            _activeSync.currentPoint.Value = start;
            _activeSync.isDrawing.Value    = true;
            _activeSync.isFinalized.Value  = false;
            _activeSync.ownerClientId.Value= NetworkManager.LocalClientId;
            go.GetComponent<NetworkObject>().Spawn();

            _isDrawing = true;
        }

        private void ContinueDrawing(Vector3 current)
        {
            _activeSync.currentPoint.Value = current;
        }

        private void FinishDrawing(Vector3 end)
        {
            // finalize data
            _activeSync.currentPoint.Value  = end;
            _activeSync.isDrawing.Value     = false;
            _activeSync.isFinalized.Value   = true;

            // dừng track, nhưng giữ lại tất cả Shape đã sinh
            ShapeStorage.ShapeAdded -= _onShapeAdded;

            _isDrawing  = false;
            _activeSync = null;

            // Giờ tool vẫn giữ nguyên cho phép user tái vẽ nếu muốn
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
