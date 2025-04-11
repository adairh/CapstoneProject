using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

namespace Manipulator
{
    public class PerformDrawing : NetworkBehaviour
    {
        public Camera mainCamera; // Assign in Inspector
        public GameObject shapeNetworkPrefab; // Assign ShapeNetworkSync prefab in Inspector

        private static IShapeButton.ShapeType currentShape = IShapeButton.ShapeType.None;
        private ShapeNetworkSync currentShapeObject;

        private string LogPrefix => $"[{(IsHost ? "Host" : "Client")}:{NetworkManager.LocalClientId}]";

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            ShapeButtonManager.OnShapeChanged += HandleShapeChange;
            Debug.Log($"{LogPrefix} [PerformDrawing] Start - IsHost: {IsHost}, IsClient: {IsClient}, LocalClientId: {NetworkManager.LocalClientId}");
        }

        void OnDestroy()
        {
            ShapeButtonManager.OnShapeChanged -= HandleShapeChange;
        }

        void HandleShapeChange(IShapeButton.ShapeType newShape)
        {
            Debug.Log($"{LogPrefix} [PerformDrawing] Shape changed to: {newShape}");
            currentShape = newShape;
        }

        void Update()
        {
            if (!IsOwner) return;
            if (mainCamera == null) return;
            if (currentShape == IShapeButton.ShapeType.None) return;

            Vector3 hitPoint = GetHitPoint();
            if (hitPoint == Vector3.zero)
            {
                Debug.LogWarning($"{LogPrefix} [PerformDrawing] Invalid hit point, skipping");
                return;
            }

            if (Input.GetMouseButtonDown(0) && currentShapeObject == null)
            {
                StartDrawing(hitPoint);
            }
            else if (Input.GetMouseButton(0) && currentShapeObject != null)
            {
                UpdateDrawing(hitPoint);
            }
            else if (Input.GetMouseButtonUp(0) && currentShapeObject != null)
            {
                FinishDrawing(hitPoint);
            }
        }

        public static void ResetShape()
        {
            currentShape = IShapeButton.ShapeType.None;
            ShapeButtonManager.SetActiveShape(IShapeButton.ShapeType.None);
        }

        private void StartDrawing(Vector3 hitPoint)
        {
            GameObject shapeObj = Instantiate(shapeNetworkPrefab);
            currentShapeObject = shapeObj.GetComponent<ShapeNetworkSync>();
            currentShapeObject.shapeType.Value = (ShapeNetworkSync.ShapeType)currentShape;
            currentShapeObject.startPoint.Value = hitPoint;
            currentShapeObject.currentPoint.Value = hitPoint;
            currentShapeObject.isDrawing.Value = true;
            currentShapeObject.isFinalized.Value = false;
            currentShapeObject.ownerClientId.Value = NetworkManager.LocalClientId;

            NetworkObject netObj = shapeObj.GetComponent<NetworkObject>();
            netObj.Spawn();
            Debug.Log($"{LogPrefix} [PerformDrawing] Spawned ShapeNetworkSync for {currentShape} at {hitPoint}");
        }

        private void UpdateDrawing(Vector3 hitPoint)
        {
            currentShapeObject.currentPoint.Value = hitPoint;
            Debug.Log($"{LogPrefix} [PerformDrawing] Updated currentPoint to {hitPoint}");
        }

        private void FinishDrawing(Vector3 hitPoint)
        {
            currentShapeObject.currentPoint.Value = hitPoint;
            currentShapeObject.isDrawing.Value = false;
            currentShapeObject.isFinalized.Value = true;
            Debug.Log($"{LogPrefix} [PerformDrawing] Finalized shape");
            currentShapeObject = null;
            ResetShape();
        }

        private Vector3 GetHitPoint()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"{LogPrefix} [PerformDrawing] Raycast hit at {hit.point}");
                return hit.point;
            }

            UnityEngine.Plane groundPlane = new UnityEngine.Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 point = ray.GetPoint(enter);
                Debug.Log($"{LogPrefix} [PerformDrawing] Ground plane hit at {point}");
                return point;
            }

            Debug.LogWarning($"{LogPrefix} [PerformDrawing] No hit detected");
            return Vector3.zero;
        }
    }
}
