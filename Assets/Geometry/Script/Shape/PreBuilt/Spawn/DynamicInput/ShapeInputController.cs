using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class ShapeInputController : MonoBehaviour
    {
        public DynamicInputPanel inputPanel;
        public static ShapeInputController Instance { get; private set; }

        private IShapeSpawner currentSpawner;

        private void Awake()
        {
            Instance = this;
        }

        public void SetSpawner(IShapeSpawner spawner)
        {
            if (currentSpawner == spawner || spawner == null) return;

            currentSpawner = spawner;
            inputPanel.Build(spawner.GetFieldDefinitions());
        }

        public void ResetSpawner()
        {
            currentSpawner = null;
            inputPanel.Clear();
        }

        public void OnSubmit()
        {
            var rawInputs = inputPanel.CollectInput();
            var solved = ShapeSolver.TrySolve(currentSpawner.GetFieldDefinitions(), rawInputs);
            inputPanel.FillCalculatedFields(solved);

            if (solved.Count >= 3) // Tùy vào hình học mà quyết định điều kiện đủ
            {
                ShapeData shape = currentSpawner.ComputeShape(solved);

                // ✅ Ensure Id is generated
                if (string.IsNullOrEmpty(shape.Id))
                    shape.Id = System.Guid.NewGuid().ToString();

                ShapeFactory.CreateFromData(shape);
            }
            else
            {
                Debug.LogWarning("Chưa đủ dữ kiện để dựng hình.");
            }
        }
    }
}
