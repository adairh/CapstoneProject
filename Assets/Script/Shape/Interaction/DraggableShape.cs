/*
using UnityEngine;
using System.Collections.Generic;

public class DraggableShape : MonoBehaviour
{
    private int state = 0; // 0: XZ movement, 1: Y movement, 2: No movement
    private Vector3 allowedAxis = new Vector3(1, 0, 1); // Start with XZ movement
    private bool isDragging = false;
    private Vector3 offset;
    private Plane movePlane;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click to change movement state
        {
            ChangeState();
        }

        if (Input.GetMouseButtonDown(0)) // Left-click to start dragging
        {
            StartDragging();
        }

        if (isDragging && Input.GetMouseButton(0)) // Continue dragging
        {
            DragObject();
        }

        if (Input.GetMouseButtonUp(0)) // Stop dragging
        {
            isDragging = false;
        }
    }

    private void ChangeState()
    {
        state = (state + 1) % 3;
        switch (state)
        {
            case 0:
                allowedAxis = new Vector3(1, 0, 1); // Move along XZ
                movePlane = new Plane(Vector3.up, transform.position);
                break;
            case 1:
                allowedAxis = new Vector3(0, 1, 0); // Move along Y
                movePlane = new Plane(Vector3.right + Vector3.forward, transform.position);
                break;
            case 2:
                allowedAxis = Vector3.zero; // No movement
                break;
        }
    }

    private void StartDragging()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the hit object is the parent or any of its children
            if (IsPartOfShape(hit.collider.transform))
            {
                offset = transform.position - hit.point;
                isDragging = true;

                // Set the move plane based on the current allowed axis
                if (state == 0)
                    movePlane = new Plane(Vector3.up, transform.position); // XZ movement
                else if (state == 1)
                    movePlane = new Plane(Vector3.right + Vector3.forward, transform.position); // Y movement
            }
        }
    }

    private void DragObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (movePlane.Raycast(ray, out float distance))
        {
            Vector3 targetPosition = ray.GetPoint(distance) + offset;
            transform.position = Vector3.Lerp(transform.position, Vector3.Scale(targetPosition, allowedAxis) + Vector3.Scale(transform.position, Vector3.one - allowedAxis), Time.deltaTime * 10);
        }
    }

    // Helper function to check if the clicked object is part of this shape
    private bool IsPartOfShape(Transform hitTransform)
    {
        return hitTransform == transform || hitTransform.IsChildOf(transform);
    }
}
*/
