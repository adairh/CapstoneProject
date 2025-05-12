using System;
using UnityEngine;

namespace Manipulator
{

    public class EditableShape : MonoBehaviour
    {

        public ISetting[] Settings { get; set; }

        void Start()
        {
            ISetting[] defaultSetting =
            {
                //new ColorSetting(Color.red, shape),
                new NameSetting(this.ToString())
            };


            Settings = defaultSetting;
        }


        public void ApplySettings()
        {
            foreach (ISetting setting in Settings) // ✅ Corrected loop
            {
                setting.Apply(); // ✅ Apply each setting to the GameObject
            }
        }


    }
}