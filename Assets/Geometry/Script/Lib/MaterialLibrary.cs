using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{
    /// <summary>
    /// Liệt kê các loại Material mà bạn sẽ dùng xuyên suốt:
    /// Default, Highlight, Drag, Select, v.v…
    /// </summary>
    public enum MaterialType
    {
        Default,
        Highlight,
        Drag,
        Select,
        Hover,
        Plane
        // TODO: thêm nếu cần
    }

    /// <summary>
    /// Cung cấp sẵn các Material URP/Lit với BaseColor cấu hình sẵn.
    /// </summary>
    public static class MaterialLibrary
    {
        // Map giữ các material đã khởi tạo (lazy)
        private static readonly Dictionary<MaterialType, Material> _materials = new();

        // Màu mặc định cho từng MaterialType (có thể tùy chỉnh)
        private static readonly Dictionary<MaterialType, Color> _colors = new()
        {
            { MaterialType.Default,   Color.red },
            { MaterialType.Highlight, new Color(1f, 0.8f, 0f) },  // vàng nhạt
            { MaterialType.Drag,      new Color(0.2f, 0.6f, 1f) }, // xanh dương
            { MaterialType.Select,    new Color(0f, 1f, 0f) },     // xanh lá
            { MaterialType.Hover,    Color.cyan },     // xanh lá
            { MaterialType.Plane,     new Color(1f, 1f, 1f, 0.5f)  },     // xanh lá
        };

        /// <summary>
        /// Lấy Material tương ứng; sẽ tự tạo lần đầu và cache lại.
        /// </summary>
        public static Material Get(MaterialType type)
        {
            if (!_materials.TryGetValue(type, out var mat))
            {
                mat = CreateLitMaterial(_colors[type]);
                mat.name = $"Mat_{type}";
                _materials[type] = mat;
            }
            return mat;
        }

        /// <summary>
        /// Tạo Material URP/Lit và gán Base Color.
        /// </summary>
        private static Material CreateLitMaterial(Color baseColor)
        {
            // Shader path của URP Lit
            const string urpLitShader = "Universal Render Pipeline/Lit";
            var shader = Shader.Find(urpLitShader);
            if (shader == null)
            {
                Debug.LogError($"Không tìm thấy shader: {urpLitShader}");
                return new Material(Shader.Find("Standard"));
            }

            var mat = new Material(shader);
            // với URP Lit, property chính là _BaseColor
            mat.SetColor("_BaseColor", baseColor);
            return mat;
        }
    }
}
