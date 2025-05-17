using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class InspectorLockToggle
{
    private static EditorWindow _mouseOverWindow;

    [MenuItem("Stuff/Select Inspector under mouse cursor (use hotkey) #&q")]
    private static void SelectLockableInspector()
    {
        if (EditorWindow.mouseOverWindow.GetType().Name == "InspectorWindow")
        {
            _mouseOverWindow = EditorWindow.mouseOverWindow;
            var type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            var findObjectsOfTypeAll = Resources.FindObjectsOfTypeAll(type);
            var indexOf = findObjectsOfTypeAll.ToList().IndexOf(_mouseOverWindow);
            EditorPrefs.SetInt("LockableInspectorIndex", indexOf);
        }
    }

    [MenuItem("Stuff/Toggle Lock &q")]
    private static void ToggleInspectorLock()
    {
        if (_mouseOverWindow == null)
        {
            if (!EditorPrefs.HasKey("LockableInspectorIndex"))
                EditorPrefs.SetInt("LockableInspectorIndex", 0);
            var i = EditorPrefs.GetInt("LockableInspectorIndex");

            var type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            var findObjectsOfTypeAll = Resources.FindObjectsOfTypeAll(type);
            _mouseOverWindow = (EditorWindow)findObjectsOfTypeAll[i];
        }

        if (_mouseOverWindow != null && _mouseOverWindow.GetType().Name == "InspectorWindow")
        {
            var type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
            var propertyInfo = type.GetProperty("isLocked");
            var value = (bool)propertyInfo.GetValue(_mouseOverWindow, null);
            propertyInfo.SetValue(_mouseOverWindow, !value, null);
            _mouseOverWindow.Repaint();
        }
    }

    [MenuItem("Stuff/Clear Console #&c")]
    private static void ClearConsole()
    {
        var type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditorInternal.LogEntries");
        type.GetMethod("Clear").Invoke(null, null);
    }
}