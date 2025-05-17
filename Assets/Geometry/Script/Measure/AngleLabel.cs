using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class AngleLabel : MonoBehaviour
    {
        public Shape PointA;
        public Shape PointB;
        public Shape PointC;
        private Camera mainCam;
        private TextMeshPro text;

        private void Start()
        {
            mainCam = Camera.main;

            var textGO = new GameObject("AngleText");
            textGO.transform.SetParent(transform);
            textGO.transform.localPosition = Vector3.zero;

            text = textGO.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 2;
            text.color = Color.cyan;
            text.enableCulling = false;
        }

        private void Update()
        {
            if (PointA == null || PointB == null || PointC == null) return;

            var a = PointA.transform.position;
            var b = PointB.transform.position;
            var c = PointC.transform.position;

            var ab = (a - b).normalized;
            var cb = (c - b).normalized;
            var angle = Vector3.Angle(ab, cb);

            text.text = angle.ToString("F1") + "°";
            transform.position = b;

            if (mainCam != null)
                text.transform.rotation = Quaternion.LookRotation(text.transform.position - mainCam.transform.position);
        }
    }
}