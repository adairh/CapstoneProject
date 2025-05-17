using UnityEngine;

namespace Manipulator
{
    public class HolographicShapeDisplay : MonoBehaviour
    {
        private VisibilitySetting setting;

        private void LateUpdate()
        {
            if (Camera.main == null) return;

            var camPos = Camera.main.transform.position;
            var direction = camPos - transform.position;

            direction.y = 0; // giữ cho biển không nghiêng theo trục dọc

            transform.rotation = Quaternion.LookRotation(-direction);
        }

        private void OnMouseDown()
        {
            if (setting == null)
            {
                Debug.LogWarning("[Hologram] No setting bound yet.");
                return;
            }

            Debug.LogWarning("[Hologram] OK.");
            setting.Value = true;
            setting.Apply();
            Destroy(gameObject.transform.parent.gameObject); // 👈 phá hủy root của prefab luôn
        }

        public void BindToSetting(VisibilitySetting s)
        {
            setting = s;
        }
    }
}