using System;
using UnityEngine;

namespace SlimUI.ModernMenu
{
    [ExecuteInEditMode]
    [Serializable]
    public class ThemedUI : MonoBehaviour
    {
        public ThemedUIData themeController;

        public virtual void Awake()
        {
            OnSkinUI();
        }

        public virtual void Update()
        {
            OnSkinUI();
        }

        protected virtual void OnSkinUI()
        {
        }
    }
}