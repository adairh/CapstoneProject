using TMPro;
using UnityEngine;

namespace Manipulator
{
    public class DistanceLabel : MonoBehaviour
    {
        public Shape PointA;
        public Shape PointB;
        private Camera mainCam;
        private TextMeshPro text;

        private void Start()
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

        private void Update()
        {
            if (PointA == null || PointB == null) return;

            var posA = PointA.transform.position;
            var posB = PointB.transform.position;
            var mid = (posA + posB) / 2;

            transform.position = mid;

            var dist = Vector3.Distance(posA, posB);
            text.text = dist.ToString("F2");

            if (mainCam != null)
                text.transform.rotation = Quaternion.LookRotation(text.transform.position - mainCam.transform.position);
        }
    }
}