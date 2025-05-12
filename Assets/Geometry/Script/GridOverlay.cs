using UnityEngine;

namespace Manipulator
{

    [ExecuteAlways]
    public class GridOverlay : MonoBehaviour
    {
        public float spacing = 1f;
        public int halfGridCount = 10;
        public Color lineColor = new Color(1f, 1f, 1f, 0.1f); // mờ mờ

        public Vector3 axis1 = Vector3.right;
        public Vector3 axis2 = Vector3.forward;

        private Material lineMaterial;

        void OnEnable()
        {
            // Material cho GL.LINES (Unlit, transparent)
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }

        void OnRenderObject()
        {
            if (!lineMaterial) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);
            GL.Color(lineColor);

            for (int i = -halfGridCount; i <= halfGridCount; i++)
            {
                Vector3 offset = i * spacing * axis1;
                GL.Vertex(-halfGridCount * spacing * axis2 + offset);
                GL.Vertex(halfGridCount * spacing * axis2 + offset);
            }

            for (int j = -halfGridCount; j <= halfGridCount; j++)
            {
                Vector3 offset = j * spacing * axis2;
                GL.Vertex(-halfGridCount * spacing * axis1 + offset);
                GL.Vertex(halfGridCount * spacing * axis1 + offset);
            }

            GL.End();
            GL.PopMatrix();
        }
    }

}