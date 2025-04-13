using System.Collections;
using System.Collections.Generic;
using Manipulator;
using UnityEngine;
using Unity.Netcode;


public class ShapeNetworkSync : NetworkBehaviour
{
    private ManipulationManager mm = ManipulationManager.Instance;
    public enum ShapeType
    {
        None, Circle, Rectangle, Triangle, Segment
    }

    public NetworkVariable<ShapeType> shapeType = new NetworkVariable<ShapeType>(ShapeType.None);
    public NetworkVariable<Vector3> startPoint = new NetworkVariable<Vector3>(Vector3.zero);
    public NetworkVariable<Vector3> currentPoint = new NetworkVariable<Vector3>(Vector3.zero);
    public NetworkVariable<bool> isDrawing = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isFinalized = new NetworkVariable<bool>(false);
    public NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue); // Tracks who created the shape

    private Shape currentShape;

    private string LogPrefix => $"[{(ownerClientId.Value == 0 ? "Host" : "Client")}:{ownerClientId.Value}]";
    
    // ==============================================================================================
    // Chỉnh cái khúc này cho nó theo logic của phép move bình thường
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveServerRpc(Vector3 newPosition)
    {
        ApplyPositionChange(newPosition);

        // Gửi cho các client còn lại
        ApplyMoveClientRpc(newPosition);
    }

    [ClientRpc]
    private void ApplyMoveClientRpc(Vector3 newPosition)
    {
        if (!IsOwner) // Tránh áp dụng lại với người đã gọi
            ApplyPositionChange(newPosition);
    }

    
    private void ApplyPositionChange(Vector3 newPosition)
    {
        if (currentShape != null)
            currentShape.MoveToPosition(newPosition);
    }

    public void MoveShape(Vector3 newPos)
    {
        if (IsServer)
        {
            ApplyPositionChange(newPos);
            ApplyMoveClientRpc(newPos);
        }
        else
        {
            RequestMoveServerRpc(newPos); // Client gọi Server để xử lý
        }
    }

    // ==============================================================================================
    
    public override void OnNetworkSpawn()
    { 
        Debug.Log($"{LogPrefix} [ShapeNetworkSync] OnNetworkSpawn - Local IsHost: {IsHost}, LocalClientId: {NetworkManager.LocalClientId}, OwnerClientId: {ownerClientId.Value}");
        shapeType.OnValueChanged += OnShapeChanged;
        startPoint.OnValueChanged += OnShapeChanged;
        currentPoint.OnValueChanged += OnShapeChanged;
        isDrawing.OnValueChanged += OnDrawingChanged;
        isFinalized.OnValueChanged += OnFinalizedChanged;

        if (isDrawing.Value && !mm.IsDrawing())
            StartShape();
        UpdateShape();
    }

    public override void OnNetworkDespawn()
    {
        shapeType.OnValueChanged -= OnShapeChanged;
        startPoint.OnValueChanged -= OnShapeChanged;
        currentPoint.OnValueChanged -= OnShapeChanged;
        isDrawing.OnValueChanged -= OnDrawingChanged;
        isFinalized.OnValueChanged -= OnFinalizedChanged;

        if (currentShape != null && currentShape.GO != null)
            Destroy(currentShape.GO);
    }

    private void OnShapeChanged<T>(T oldValue, T newValue)
    {
        if (!isDrawing.Value || isFinalized.Value) return;
        UpdateShape();
    }

    private void OnDrawingChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            StartShape();
        else if (!isFinalized.Value)
            UpdateShape();
    }

    private void OnFinalizedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            FinalizeShape();
    }

    private void StartShape()
    {
        Segment.BeginSketch(startPoint.Value);
    }
    private void UpdateShape()
    {
        Segment.UpdateSketch(currentPoint.Value);
    }
    private void FinalizeShape()
    {
        Segment.EndSketch(currentPoint.Value);
    }
    
    /*private void StartShape()
    {
        if (currentShape != null && currentShape.GO != null)
            Destroy(currentShape.GO);

        switch (shapeType.Value)
        {
            case ShapeType.Circle:
                currentShape = new Circle(startPoint.Value, 0);
                Debug.Log($"{LogPrefix} [ShapeNetworkSync] Created Circle at {startPoint.Value}");
                break;
            case ShapeType.Rectangle:
                currentShape = new Rectangle(startPoint.Value, 0, 0);
                Debug.Log($"{LogPrefix} [ShapeNetworkSync] Created Rectangle at {startPoint.Value}");
                break;
            case ShapeType.Triangle:
                currentShape = new Triangle(startPoint.Value, startPoint.Value, startPoint.Value);
                Debug.Log($"{LogPrefix} [ShapeNetworkSync] Created Triangle at {startPoint.Value}");
                break;
            case ShapeType.Segment:
                //currentShape = new Segment(new Point(startPoint.Value), new Point(startPoint.Value));
                //Debug.Log($"{LogPrefix} [ShapeNetworkSync] Created Segment at {startPoint.Value}");
                Segment.BeginSketch(startPoint.Value);
                break;
        }

        /*if (currentShape != null && currentShape.GO != null)
        {
            currentShape.GO.transform.SetParent(transform, false);
            currentShape.GO.SetActive(true);
            Debug.Log($"{LogPrefix} [ShapeNetworkSync] Shape GO: {currentShape.GO.name} at {currentShape.GO.transform.position}");
        }
        else
        {
            Debug.LogError($"{LogPrefix} [ShapeNetworkSync] Shape or GO is null after creation!");
        }#1#
        
        //mm.SetDrawing(true);
    }

    private void UpdateShape()
    {
        if (currentShape == null || !isDrawing.Value)
        {
            Debug.LogWarning($"{LogPrefix} [ShapeNetworkSync] UpdateShape skipped: Shape null or not drawing");
            return;
        }

        Debug.Log($"{LogPrefix} [ShapeNetworkSync] Updating {shapeType.Value} - Start: {startPoint.Value}, Current: {currentPoint.Value}");
        switch (shapeType.Value)
        {
            case ShapeType.Circle:
                ((Circle)currentShape).Radius = Vector3.Distance(startPoint.Value, currentPoint.Value);
                currentShape.Draw();
                break;
            case ShapeType.Rectangle:
                Vector3 size = currentPoint.Value - startPoint.Value;
                ((Rectangle)currentShape).Width = Mathf.Abs(size.x);
                ((Rectangle)currentShape).Height = Mathf.Abs(size.z);
                ((Rectangle)currentShape).Position = startPoint.Value + new Vector3(size.x / 2, 0, size.z / 2);
                currentShape.Draw();
                break;
            case ShapeType.Triangle:
                Vector3 thirdPoint = startPoint.Value + Vector3.Cross(currentPoint.Value - startPoint.Value, Vector3.up).normalized * Vector3.Distance(startPoint.Value, currentPoint.Value) * 0.5f;
                ((Triangle)currentShape).Corners[0].Position = startPoint.Value;
                ((Triangle)currentShape).Corners[1].Position = currentPoint.Value;
                ((Triangle)currentShape).Corners[2].Position = thirdPoint;
                currentShape.Draw();
                break;
            case ShapeType.Segment:
                //((Segment)currentShape).End.Position = currentPoint.Value;
                //currentShape.Draw();
                break;
        }

        if (currentShape.GO != null)
            Debug.Log($"{LogPrefix} [ShapeNetworkSync] Shape position: {currentShape.GO.transform.position}");
    }

    private void FinalizeShape()
    {
        if (currentShape != null)
        {
            currentShape.CompleteDraw();
            Debug.Log($"{LogPrefix} [ShapeNetworkSync] Finalized {shapeType.Value}");
            currentShape = null;
            mm.SetDrawing(false);
            
            
            switch (shapeType.Value)
            { 
                case ShapeType.Segment:
//                    ((Segment)currentShape).End.AttachToShape(currentShape);
//                    ((Segment)currentShape).Start.AttachToShape(currentShape);
                    break;
            }
            
            
        }
    }*/
}