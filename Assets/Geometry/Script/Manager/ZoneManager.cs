using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manipulator
{  

    public class ZoneManager : MonoBehaviour
    {
        [Header("Prefabs for Foundations")]
        public GameObject gridPrefab;
        public GameObject axisPrefab;
        // Add more foundation prefabs as needed

        public float zoneSize = 20f;
        public int currentZoneIndex = 0;

        private Dictionary<int, List<GameObject>> zoneFoundations = new();

        public void GoToZoneSmooth(int zoneIndex, float duration = 1.0f)
        {
            // Calculate center for this zone
            Vector3 zoneCenter = new Vector3(zoneIndex * zoneSize, 0, 0);

            // Hide current foundations
            if (zoneFoundations.TryGetValue(currentZoneIndex, out var oldList))
                foreach (var obj in oldList) obj.SetActive(false);

            currentZoneIndex = zoneIndex;

            // Move camera smoothly to this zone
            if (CameraController.Instance != null)
                CameraController.Instance.MoveToZoneSmooth(zoneCenter, duration);

            // (Optional) Update camera bounds
            if (CameraController.Instance != null)
            {
                float half = zoneSize / 2f;
                CameraController.Instance.useBounds = true;
                CameraController.Instance.minBounds = zoneCenter + new Vector3(-half, -1, -half);
                CameraController.Instance.maxBounds = zoneCenter + new Vector3(half, 10, half);
            }

            // Spawn or enable foundation objects
            if (!zoneFoundations.TryGetValue(currentZoneIndex, out var foundationList))
            {
                foundationList = new List<GameObject>();
                if (gridPrefab != null)
                    foundationList.Add(Instantiate(gridPrefab, zoneCenter, Quaternion.identity));
                if (axisPrefab != null)
                    foundationList.Add(Instantiate(axisPrefab, zoneCenter, Quaternion.identity));
                // Add more foundations here
                zoneFoundations[currentZoneIndex] = foundationList;
            }
            else
            {
                foreach (var obj in foundationList) obj.SetActive(true);
            }
        }

        public void GoToNextZone(float duration = 1.0f)
        {
            GoToZoneSmooth(currentZoneIndex + 1, duration);
        }

        public void GoToPreviousZone(float duration = 1.0f)
        {
            if (currentZoneIndex > 0)
                GoToZoneSmooth(currentZoneIndex - 1, duration);
        }
    }


}