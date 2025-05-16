using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    public class ShapeInputController : MonoBehaviour
    {
        public DynamicInputPanel inputPanel;
        private IShapeSpawner currentSpawner;

        public void SetSpawner(IShapeSpawner spawner)
        {
            currentSpawner = spawner;
            inputPanel.Build(spawner.GetFieldDefinitions());
        }

        public void OnSubmit()
        {
            var rawInputs = inputPanel.CollectInput();
            var solved = ShapeSolver.TrySolve(currentSpawner.GetFieldDefinitions(), rawInputs);
            inputPanel.FillCalculatedFields(solved);

            if (solved.Count >= 3) // Tùy vào hình học mà quyết định điều kiện đủ
            {
                ShapeData shape = currentSpawner.ComputeShape(solved);
                ShapeFactory.CreateFromData(shape);
            }
            else
            {
                Debug.LogWarning("Chưa đủ dữ kiện để dựng hình.");
            }
        }
    }
}
