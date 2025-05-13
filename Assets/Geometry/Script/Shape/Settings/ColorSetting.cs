using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
                GameObject btn = new GameObject("ColorBtn", typeof(Image), typeof(Button));
                btn.transform.SetParent(container, false);

                var img = btn.GetComponent<Image>();
                img.color = MaterialLibrary.Get(matType).GetColor("_BaseColor");

                var button = btn.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    Value = matType;
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
            foreach (var s in targetShape.GetDependentShapesForDelete())
            {
                var newMat = new Material(MaterialLibrary.Get(Value));
                s.DefaultMat = newMat;

                foreach (var r in s.GetComponentsInChildren<Renderer>())
                {
                    r.material = newMat;
                }
            }
            // Force SelectableShape to refresh its material
            var selectable = targetShape.GetComponent<SelectableShape>();
            if (selectable != null)
            {
                bool sel = selectable.IsSelected();
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
            // Optional: dựa vào renderer.material == X để gán Value tương ứng nếu cần
        }
    }

}
