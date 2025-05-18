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
            var fieldDefs = currentSpawner.GetFieldDefinitions();
            var fieldSolver = new FieldSolver(fieldDefs);
            var solved = fieldSolver.Solve(rawInputs);

            inputPanel.FillCalculatedFields(solved);
 
            try
            {
                currentSpawner.ComputeShape(solved);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Chưa đủ dữ kiện để dựng hình. {e.Message}");
            }
            ResetSpawner(); 
            
        }

    }
}