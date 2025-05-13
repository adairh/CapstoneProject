
using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class DistanceLabel : MonoBehaviour
    {
        public Shape PointA;
        public Shape PointB;
        private TextMeshPro text;
        private Camera mainCam;

        void Start()
        {
            mainCam = Camera.main;

            var textGO = new GameObject("DistanceText");
            textGO.transform.SetParent(transform);
            textGO.transform.localPosition = Vector3.zero;

            text = textGO.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 2;
            text.color = Color.yellow;
            text.enableCulling = false;
        }

        void Update()
        {
            if (PointA == null || PointB == null) return;

            Vector3 posA = PointA.transform.position;
            Vector3 posB = PointB.transform.position;
            Vector3 mid = (posA + posB) / 2;

            transform.position = mid;

            float dist = Vector3.Distance(posA, posB);
            text.text = dist.ToString("F2");

            if (mainCam != null)
            {
                text.transform.rotation = Quaternion.LookRotation(text.transform.position - mainCam.transform.position);
            }
        }
    }
}
