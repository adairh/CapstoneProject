using UnityEngine;

namespace Manipulator
{
    public class EditableShape : MonoBehaviour
    {
        public ISetting[] Settings { get; set; }

        private void Start()
        {
            ISetting[] defaultSetting =
            {
                //new ColorSetting(Color.red, shape),
                new NameSetting(ToString())
            };


            Settings = defaultSetting;
        }


        public void ApplySettings()
        {
            foreach (var setting in Settings) // ✅ Corrected loop
                setting.Apply(); // ✅ Apply each setting to the GameObject
        }
    }
}