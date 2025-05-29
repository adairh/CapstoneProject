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
        private static readonly Dictionary<MaterialType, Material> _materials = new();

        private static readonly Dictionary<MaterialType, Color> _colors = new()
        {
            { MaterialType.Default, Color.red },
            { MaterialType.Highlight, new Color(1f, 0.8f, 0f) },
            { MaterialType.Drag, new Color(0.2f, 0.6f, 1f) },
            { MaterialType.Select, new Color(0f, 0.5f, 0.3f) },
            { MaterialType.Hover, new Color(0.8f, 0.5f, 0.3f) },
            { MaterialType.Plane, new Color(1f, 1f, 1f, 0.5f) },
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

        public static Material Get(MaterialType type, float alpha = 1f)
        {
            if (!_materials.TryGetValue(type, out var mat))
            {
                mat = CreateLitMaterial(_colors[type]);
                mat.name = $"Mat_{type}";
                _materials[type] = mat;
            }

            if (mat == null) return null;
            var baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            baseColor.a = alpha;
            mat.SetColor("_BaseColor", baseColor);

            // Double-sided
            mat.SetInt("_CullMode", 0);
            mat.EnableKeyword("_DOUBLESIDED_ON");
            mat.doubleSidedGI = true;

            // Transparent mode
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            return mat;
        }

        private static Material CreateLitMaterial(Color baseColor)
        {
            const string urpLitShader = "Universal Render Pipeline/Lit";
            var shader = Shader.Find(urpLitShader);
            if (shader == null)
            {
                Debug.LogError($"Không tìm thấy shader: {urpLitShader}");
                return new Material(Shader.Find("Standard"));
            }

            var mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            return mat;
        }

        public static Material MakeDoubleSidedTransparent(Material baseMat, Color color, float alpha = 0.08f)
        {
            var mat = new Material(baseMat.shader);
            mat.CopyPropertiesFromMaterial(baseMat);

            mat.SetColor("_BaseColor", new Color(color.r, color.g, color.b, alpha));
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_CullMode", 0);
            mat.EnableKeyword("_DOUBLESIDED_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            return mat;
        }

        
        public static Color GetColorForType(MaterialType type)
        {
            return _colors.TryGetValue(type, out var c) ? c : Color.white;
        }

        

        public static Material GetPolygonMat(Color? colorOverride = null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");

            var mat = new Material(shader);
            Color color = colorOverride ?? new Color(0.4f, 0.8f, 1f, 0.08f);

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;

            if (mat.HasProperty("_CullMode"))
                mat.SetInt("_CullMode", 0);
            if (mat.HasProperty("_Cull"))
                mat.SetInt("_Cull", (int)CullMode.Off);
            if (mat.HasProperty("_RenderFace")) mat.SetInt("_RenderFace", 0);

            mat.EnableKeyword("_DOUBLESIDED_ON");
            mat.doubleSidedGI = true;

            if (mat.HasProperty("_Surface"))
                mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            if (mat.HasProperty("_ZWrite"))
                mat.SetInt("_ZWrite", 0);
            if (mat.HasProperty("_SrcBlend"))
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend"))
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

            return mat;
        }
        
        
        public static Material GetPolygonMeshMaterial(Color color, float alpha = 0.08f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader);

            Color finalColor = color;
            finalColor.a = alpha;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", finalColor);
            else mat.color = finalColor;

            // Set transparent rendering
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
            if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Double-sided settings
            if (mat.HasProperty("_CullMode")) mat.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Off);
            if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            if (mat.HasProperty("_RenderFace")) mat.SetInt("_RenderFace", 0); // Both
            mat.EnableKeyword("_DOUBLESIDED_ON");
            mat.doubleSidedGI = true;

            return mat;
        }

    }
}
