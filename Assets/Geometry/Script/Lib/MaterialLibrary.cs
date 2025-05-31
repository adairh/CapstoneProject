using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Manipulator
{
    public enum MaterialType
    {
        Default,
        Highlight,
        Drag,
        Select,
        Hover,
        Plane,
        Red,
        Green,
        Blue,
        Yellow,
        White,
        Black,
        Cyan,
        Magenta,
        Gray
    }
 
    public static class MaterialLibrary
    {
        /// <summary>
        /// Gets the universal (singleton) material for all mesh rendering.
        /// </summary>
        public static Material Get(MaterialType type = MaterialType.Default)
        {
            return ManipulationManager.Instance != null
                ? ManipulationManager.Instance.universalMat
                : null;
        }

        public static void Apply(Renderer renderer, MaterialType type, float? alphaOverride = null)
        {
            var mat = Get();
            if (mat == null) { Debug.LogError("Universal Material not assigned!"); return; }
            renderer.sharedMaterial = mat;

            var color = GetColorForType(type);
            if (alphaOverride.HasValue) color.a = alphaOverride.Value;
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
        }
 

        // Optionally, keep your color map:
        private static readonly System.Collections.Generic.Dictionary<MaterialType, Color> _colors = new()
        {
            { MaterialType.Default, new Color(0.1f, 0.3f, 3f, 1f) },
            { MaterialType.Highlight, new Color(1f, 0.8f, 0f, 1f) },
            { MaterialType.Drag, new Color(0.2f, 0.6f, 1f, 1f) },
            { MaterialType.Select, new Color(0f, 0.5f, 0.3f, 1f) },
            { MaterialType.Hover, new Color(0.8f, 0.5f, 0.3f, 1f) },
            { MaterialType.Plane, new Color(1f, 1f, 1f, 0.08f) },
            { MaterialType.Red, Color.red },
            { MaterialType.Green, Color.green },
            { MaterialType.Blue, Color.blue },
            { MaterialType.Yellow, Color.yellow },
            { MaterialType.White, Color.white },
            { MaterialType.Black, Color.black },
            { MaterialType.Cyan, Color.cyan },
            { MaterialType.Magenta, Color.magenta },
            { MaterialType.Gray, Color.gray }
        };

        public static Color GetColorForType(MaterialType type)
            => _colors.TryGetValue(type, out var c) ? c : Color.white;
    }
}
