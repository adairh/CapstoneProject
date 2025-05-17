using System;
using UnityEngine;

namespace SlimUI.ModernMenu
{
    [CreateAssetMenu(menuName = "ThemeSettings")]
    [Serializable]
    public class ThemedUIData : ScriptableObject
    {
        [Header("PRESETS")] public Custom1 custom1;

        public Custom2 custom2;
        public Custom3 custom3;

        [HideInInspector] public Color currentColor;

        [HideInInspector] public Color32 textColor;

        [Serializable]
        public class Custom1
        {
            [Header("Text")] public Color graphic1;

            public Color32 text1;
        }

        [Serializable]
        public class Custom2
        {
            [Header("Text")] public Color graphic2;

            public Color32 text2;
        }

        [Serializable]
        public class Custom3
        {
            [Header("Text")] public Color graphic3;

            public Color32 text3;
        }
    }
}