// using TMPro;
// using UnityEngine;
//
// namespace Manipulator
// {
//     public class HologramLabel : Shape
//     {
//         private TextMeshPro textMesh;
//         private Constraint constraintSource;
//
//         public HologramLabel(Vector3 position, Constraint constraint, Shape parent = null)
//             : base(position, "Hologram", parent)
//         {
//             constraintSource = constraint;
//             GO = new GameObject(Name);
//             GO.transform.position = Position;
//         }
//
//         protected override void SetupGameObject()
//         {
//             textMesh = GO.AddComponent<TextMeshPro>();
//             textMesh.fontSize = 2;
//             textMesh.alignment = TextAlignmentOptions.Center;
//             textMesh.color = new Color(0.5f, 0.5f, 1f, 1f);
//             textMesh.outlineColor = new Color(0.3f, 1f, 1f, 1f);
//             textMesh.enableCulling = false;
//             textMesh.enableWordWrapping = false;
//
//             GO.AddComponent<Billboard>();
//             GO.AddComponent<HologramClickHandler>().SetShape(this);
//         }
//
//         public Constraint GetConstraint() => constraintSource;
//
//         public void Update()
//         {
//             if (constraintSource != null && textMesh != null)
//             {
//                 textMesh.text = constraintSource.GetLabelText();
//             }
//         }
//
//         public void SetText()
//         {
//             Update();
//         }
//         
//         public override void Drawing()
//         {
//             GO.transform.position = Position;
//             Update();
//         }
//         public override void UpdateHitbox() { }
//         public override GameObject[] Components() => new[] { GO };
//         protected override void InitializeSettings() { }
//     }
//     public class Billboard : MonoBehaviour
//     {
//         private void Update()
//         {
//             if (Camera.main != null)
//                 transform.forward = Camera.main.transform.forward;
//         }
//     }
// }
