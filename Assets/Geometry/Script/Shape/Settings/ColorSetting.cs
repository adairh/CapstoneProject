using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manipulator
{
    public class ColorSetting : Setting<MaterialType>
    {
        private readonly List<MaterialType> colorOptions = new()
        {
            MaterialType.Red,
            MaterialType.Green,
            MaterialType.Blue,
            MaterialType.Yellow,
            MaterialType.White,
            MaterialType.Black,
            MaterialType.Cyan,
            MaterialType.Magenta,
            MaterialType.Gray
        };

        public ColorSetting(MaterialType initial, Shape shape)
            : base(initial, ISetting.SettingType.NONNUMERIC, typeof(Shape))
        {
            targetShape = shape;
            prefab = UIManager.Instance.GetUIComponent("ColorSettingPalettePrefab");
        }

        public override GameObject GetUI()
        {
            uiInstance = Object.Instantiate(prefab);
            var container = uiInstance.transform.Find("ColorGrid");

            foreach (var matType in colorOptions)
            {
                var btn = new GameObject("ColorBtn", typeof(Image), typeof(Button));
                btn.transform.SetParent(container, false);

                var img = btn.GetComponent<Image>();
                img.color = MaterialLibrary.GetColorForType(matType);

                var button = btn.GetComponent<Button>();
                // IMPORTANT: Capture variable for closure
                var capturedMatType = matType;
                button.onClick.AddListener(() =>
                {
                    Value = capturedMatType;
                    Apply();
                });

                var layout = btn.AddComponent<LayoutElement>();
                layout.preferredWidth = 40;
                layout.preferredHeight = 40;
            }

            Update();
            return uiInstance;
        }

        public override void Apply()
        {
            // Store the chosen color for refresh logic if needed
            //targetShape.DefaultMat = Value; // <- add this field to Shape for re-apply on refresh if you wish

            foreach (var s in targetShape.GetDependentShapesForDelete())
            {
                foreach (var r in s.GetComponentsInChildren<Renderer>())
                {
                    MaterialLibrary.Apply(r, Value); // This MUST use PropertyBlock and never swap material
                }
            }

            // Optionally force SelectableShape to refresh highlight/selection if that's needed
            var selectable = targetShape.GetComponent<SelectableShape>();
            if (selectable != null)
            {
                var sel = selectable.IsSelected();
                selectable.SetSelected(!sel); // flip
                selectable.SetSelected(sel); // restore
            }
        }


        public override float Height()
        {
            return 0;
        }

        public override void Update()
        {
            // (Optional) Set UI highlight/indicator for current color, if you want
        }
    }
}
