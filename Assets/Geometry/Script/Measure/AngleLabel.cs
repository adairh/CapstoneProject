
using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class AngleLabel : MonoBehaviour
    {
        public Shape PointA;
        public Shape PointB;
        public Shape PointC;
        private TextMeshPro text;
        private Camera mainCam;

        void Start()
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

        void Update()
        {
            if (PointA == null || PointB == null || PointC == null) return;

            Vector3 a = PointA.transform.position;
            Vector3 b = PointB.transform.position;
            Vector3 c = PointC.transform.position;

            Vector3 ab = (a - b).normalized;
            Vector3 cb = (c - b).normalized;
            float angle = Vector3.Angle(ab, cb);

            text.text = angle.ToString("F1") + "°";
            transform.position = b;

            if (mainCam != null)
            {
                text.transform.rotation = Quaternion.LookRotation(text.transform.position - mainCam.transform.position);
            }
        }
    }
}
