// using UnityEngine;
//
// namespace Manipulator
// {
//     public class HologramClickHandler : MonoBehaviour
//     {
//         private Shape shape;
//         private Camera cam;
//         private SpawnPanel panelSpawner;
//         private const float clickThreshold = 0.15f; // khoảng cách nhỏ trên màn hình
//
//         public void SetShape(Shape shape)
//         {
//             this.shape = shape;
//         }
//
//         private void Start()
//         {
//             cam = Camera.main;
//             panelSpawner = new SpawnPanel();
//         }
//
//         private void Update()
//         {
//             if (Input.GetMouseButtonDown(1)) // Right-click
//             {
//                 if (cam == null || shape == null) return;
//
//                 Vector3 screenPos = cam.WorldToScreenPoint(shape.transform.position);
//                 Vector2 mouse = Input.mousePosition;
//                 float dist = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), mouse);
//
//                 // Normalized distance by screen size
//                 float screenSize = Mathf.Min(Screen.width, Screen.height);
//                 float normalizedDist = dist / screenSize;
//
//                 if (normalizedDist < clickThreshold)
//                 {
//                     Debug.Log("[HologramClickHandler] Right-clicked hologram label!");
//                     panelSpawner.SpawnPanelAtTop(shape);
//                 }
//             }
//         }
//     }
// }