using UnityEngine;
using System.Collections.Generic;

namespace Manipulator
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvas")]
        [SerializeField] private Transform canvas;

        [Header("UI Prefabs")]
        [Tooltip("Drag in any UI prefab you want to register at startup")]
        [SerializeField] private List<GameObject> uiPrefabsList;

        // Internal lookup of name → prefab
        private Dictionary<string, GameObject> uiPrefabs = new Dictionary<string, GameObject>();

        /// <summary>
        /// Read-only access to registered prefabs.
        /// </summary>
        public IReadOnlyDictionary<string, GameObject> UIPrefabs => uiPrefabs;

        private void Awake()
        {
            // --- singleton boilerplate ---
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // load any prefabs assigned in inspector
            LoadUIComponentsFromList();
        }

        /// <summary>
        /// Registers a prefab under a given key (use its name or your own).
        /// </summary>
        public void RegisterUIComponent(string key, GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"UIManager: Attempted to register null prefab under key '{key}'");
                return;
            }
            if (!uiPrefabs.ContainsKey(key))
                uiPrefabs[key] = prefab;
            else
                Debug.LogWarning($"UIManager: Key '{key}' is already registered");
        }

        /// <summary>
        /// Returns the raw prefab registered under that key (or null).
        /// </summary>
        public GameObject GetUIComponent(string key)
        {
            uiPrefabs.TryGetValue(key, out var prefab);
            return prefab;
        }

        /// <summary>
        /// Instantiates the prefab under that key as a child of the main canvas.
        /// Returns the instance or null if missing.
        /// </summary>
        public GameObject InstantiateUIComponent(string key)
        {
            var prefab = GetUIComponent(key);
            if (prefab == null)
            {
                Debug.LogWarning($"UIManager: No prefab registered under '{key}'");
                return null;
            }
            if (canvas == null)
            {
                Debug.LogWarning("UIManager: Canvas Transform is null, cannot instantiate UI");
                return null;
            }
            return Instantiate(prefab, canvas);
        }

        /// <summary>
        /// For other systems to grab the canvas Transform reference.
        /// </summary>
        public Transform GetCanvasTransform() => canvas;

        private void LoadUIComponentsFromList()
        {
            if (uiPrefabsList == null) return;
            foreach (var prefab in uiPrefabsList)
                if (prefab != null)
                    RegisterUIComponent(prefab.name, prefab);
        }
    }
}
