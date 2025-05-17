using UnityEngine;
using UnityEngine.Rendering;

namespace Manipulator
{
    [ExecuteAlways]
    public class GridOverlay : MonoBehaviour
    {
        public float spacing = 1f;
        public int halfGridCount = 10;
        public Color lineColor = new(1f, 1f, 1f, 0.1f); // mờ mờ

        public Vector3 axis1 = Vector3.right;
        public Vector3 axis2 = Vector3.forward;

        private Material lineMaterial;

        private void OnEnable()
        {
            // Material cho GL.LINES (Unlit, transparent)
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        private void OnRenderObject()
        {
            if (!lineMaterial) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);
            GL.Color(lineColor);

            for (var i = -halfGridCount; i <= halfGridCount; i++)
            {
                var offset = i * spacing * axis1;
                GL.Vertex(-halfGridCount * spacing * axis2 + offset);
                GL.Vertex(halfGridCount * spacing * axis2 + offset);
            }

            for (var j = -halfGridCount; j <= halfGridCount; j++)
            {
                var offset = j * spacing * axis2;
                GL.Vertex(-halfGridCount * spacing * axis1 + offset);
                GL.Vertex(halfGridCount * spacing * axis1 + offset);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}