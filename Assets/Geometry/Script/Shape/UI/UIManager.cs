using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Transform canvas;

    [SerializeField] private List<GameObject> uiPrefabsList; // List of UI prefabs set via editor

    private Dictionary<string, GameObject> uiPrefabs = new Dictionary<string, GameObject>(); // Storage for UI components

    public Dictionary<string, GameObject> UIPrefabs
    {
        get { return uiPrefabs;}
    }
    
    public Transform GetCanvasTransform()
    {
        return canvas;
    }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
 

        LoadUIComponentsFromList();
    }

    /// <summary>
    /// Registers a prebuilt UI component for reuse.
    /// </summary>
    public void RegisterUIComponent(string key, GameObject prefab)
    {
        if (!uiPrefabs.ContainsKey(key))
        {
            uiPrefabs[key] = prefab;
        }
        else
        {
            //Debug.LogWarning($"UI component with key {key} is already registered.");
        }
    }

    /// <summary>
    /// Retrieves a stored UI component by key.
    /// </summary>
    public GameObject GetUIComponent(string key)
    {
        if (uiPrefabs.TryGetValue(key, out GameObject prefab))
        {
            return prefab; // Return the original prefab, not an instantiated one
        }
        //Debug.LogWarning($"UI component with key {key} not found.");
        return null;
    }


    /// <summary>
    /// Loads UI components from the assigned list in the editor.
    /// </summary>
    private void LoadUIComponentsFromList()
    {
        foreach (GameObject prefab in uiPrefabsList)
        {
            if (prefab != null)
            {
                RegisterUIComponent(prefab.name, prefab);
            }
        }
    } 
}
