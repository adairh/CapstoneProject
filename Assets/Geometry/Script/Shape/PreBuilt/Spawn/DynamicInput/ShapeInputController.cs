using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manipulator
{
    public class ShapeInputController : MonoBehaviour
    {
        public DynamicInputPanel inputPanel;

        private IShapeSpawner currentSpawner;
        public static ShapeInputController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetSpawner(IShapeSpawner spawner)
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                inputPanel.gameObject.SetActive(true);
                
                Vector3 startPos;
                if (PerformDrawing.RaycastMouse(out startPos))
                {
                    ManipulationManager.Instance.TrackingPoint = startPos;
                    if (currentSpawner == spawner || spawner == null) return;

                    currentSpawner = spawner;
                    inputPanel.Build(spawner.GetFieldDefinitions());
                }
            }
        }

        public void ResetSpawner()
        {
            currentSpawner = null;
            inputPanel.Clear();
            inputPanel.gameObject.SetActive(false);
        }

        public void OnSubmit()
        {
            var rawInputs = inputPanel.CollectInput();
            var solved = ShapeSolver.TrySolve(currentSpawner.GetFieldDefinitions(), rawInputs);
            inputPanel.FillCalculatedFields(solved);

            if (solved.Count >= 3) // Tùy vào hình học mà quyết định điều kiện đủ
            {
                var shape = currentSpawner.ComputeShape(solved);

                // ✅ Ensure Id is generated
                if (string.IsNullOrEmpty(shape.Id))
                    shape.Id = Guid.NewGuid().ToString();

                //ShapeFactory.CreateFromData(shape);
                ResetSpawner();
            }
            else
            {
                Debug.LogWarning("Chưa đủ dữ kiện để dựng hình.");
            }
        }
    }
}