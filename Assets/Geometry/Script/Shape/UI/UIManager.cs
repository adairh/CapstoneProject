using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using An_An;

namespace Manipulator
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private Transform canvas; // Main Canvas transform for UI instantiation
        [SerializeField] private GameObject canvasOnboarding; // Onboarding screen with ShowHome button
        [SerializeField] private GameObject canvasHome; // Home page for hosting/joining rooms
        [SerializeField] private List<GameObject> uiPrefabsList; // List of UI prefabs set via editor

        /*public GameObject canvasPlaygame;
        public GameObject popupJoinRoom;
        public GameObject popupCreateRoom;
        public GameObject darkOverlay;
        public TMP_InputField joinRoomIDInput;
        public TMP_InputField joinPasswordInput;
        public TMP_InputField createRoomIDInput;
        public TMP_InputField createPasswordInput;*/

        private Dictionary<string, GameObject> uiPrefabs = new Dictionary<string, GameObject>(); // Storage for UI components

        public Dictionary<string, GameObject> UIPrefabs
        {
            get { return uiPrefabs; }
        }

        public Transform GetCanvasTransform()
        {
            return canvas;
        }

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Validate serialized fields
            /*if (canvas == null) Debug.LogError("Main canvas Transform is not assigned in UIManager!");
            if (canvasOnboarding == null) Debug.LogError("canvasOnboarding is not assigned in UIManager!");
            if (canvasHome == null) Debug.LogError("canvasHome is not assigned in UIManager!");*/
            if (uiPrefabsList == null || uiPrefabsList.Count == 0)
                Debug.LogWarning("uiPrefabsList is empty or not assigned in UIManager!");

            LoadUIComponentsFromList();
        }

        private void Start()
        {
            // Initialize canvas states
            /*if (canvasOnboarding != null)
            {
                canvasOnboarding.SetActive(true);
                Debug.Log("canvasOnboarding activated");
            }
            else
            {
                Debug.LogError("Cannot activate canvasOnboarding: Not assigned!");
            }*/

            /*if (canvasHome != null)
            {
                canvasHome.SetActive(false);
                Debug.Log("canvasHome deactivated");
            }
            else
            {
                Debug.LogError("Cannot deactivate canvasHome: Not assigned!");
            }*/
        }

        public void ShowHome()
        {
            /*if (canvasOnboarding != null)
            {
                canvasOnboarding.SetActive(false);
                Debug.Log("canvasOnboarding deactivated");
            }
            else
            {
                Debug.LogError("Cannot deactivate canvasOnboarding: Not assigned!");
            }*/

            /*if (canvasHome != null)
            {
                canvasHome.SetActive(true);
                Debug.Log("canvasHome activated");
            }
            else
            {
                Debug.LogError("Cannot activate canvasHome: Not assigned!");
            }*/

            /*if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
                Debug.Log("canvas activated");
            }
            else
            {
                Debug.LogError("Cannot activate canvas: Not assigned!");
            }*/
        }

        public void ShowOnboarding()
        {
            /*if (canvasHome != null)
            {
                canvasHome.SetActive(false);
                Debug.Log("canvasHome deactivated");
            }
            else
            {
                Debug.LogError("Cannot deactivate canvasHome: Not assigned!");
            }

            if (canvasOnboarding != null)
            {
                canvasOnboarding.SetActive(true);
                Debug.Log("canvasOnboarding activated");
            }
            else
            {
                Debug.LogError("Cannot activate canvasOnboarding: Not assigned!");
            }*/
        }

        public void RegisterUIComponent(string key, GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Attempted to register null prefab with key {key}");
                return;
            }

            if (!uiPrefabs.ContainsKey(key))
            {
                uiPrefabs[key] = prefab;
                Debug.Log($"Registered UI component: {key}");
            }
            else
            {
                Debug.LogWarning($"UI component with key {key} is already registered.");
            }
        }

        public GameObject GetUIComponent(string key)
        {
            if (uiPrefabs.TryGetValue(key, out GameObject prefab))
            {
                return prefab; // Return the original prefab
            }

            Debug.LogWarning($"UI component with key {key} not found.");
            return null;
        }

        public GameObject InstantiateUIComponent(string key)
        {
            GameObject prefab = GetUIComponent(key);
            if (prefab != null && canvas != null)
            {
                GameObject instance = Instantiate(prefab, canvas);
                Debug.Log($"Instantiated UI component: {key}");
                return instance;
            }

            Debug.LogWarning($"Failed to instantiate UI component: {key} (prefab or canvas missing)");
            return null;
        }

        private void LoadUIComponentsFromList()
        {
            if (uiPrefabsList == null || uiPrefabsList.Count == 0)
            {
                Debug.LogWarning("uiPrefabsList is empty or not assigned in UIManager!");
                return;
            }

            foreach (GameObject prefab in uiPrefabsList)
            {
                if (prefab != null)
                {
                    RegisterUIComponent(prefab.name, prefab);
                }
                else
                {
                    Debug.LogWarning("Null prefab found in uiPrefabsList!");
                }
            }
        }
    }
}