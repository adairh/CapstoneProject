using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LocalizedTextEditor : EditorWindow
{
    public LocalizationData localizationData;
    public string key;
    private string filename = "No file specified";
    private string filePath;
    private Vector2 scrollPosition;
    private string value;

    private void OnGUI()
    {
        if (localizationData != null)
        {
            EditorGUI.HelpBox(new Rect(5, 5, 400, 40), "Now editing: " + filename, MessageType.Info);
            //EditorGUI.HelpBox(filename, EditorStyles.boldLabel, EditorStyles.helpBox);
            GUILayout.Space(55);
            key = EditorGUILayout.TextField("Key: ", key);
            value = EditorGUILayout.TextField("Value: ", value);

            if (GUILayout.Button("Add", GUILayout.Height(30)))
            {
                var exists = false;
                foreach (var item in localizationData.items)
                    if (item.key == key)
                        exists = true;

                if (!exists)
                {
                    var newItem = new LocalizationItem();
                    newItem.key = key;
                    newItem.value = value;

                    var temp = new List<LocalizationItem>();

                    foreach (var item in localizationData.items)
                        temp.Add(item);

                    temp.Add(newItem);

                    localizationData.items = temp.ToArray();
                }
                else
                {
                    EditorUtility.DisplayDialog("Key already exists. Please edit it in the hierarchy", "OK?", "Ok",
                        "Ok?");
                }
            }

            EditorGUILayout.LabelField("Raw data view", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            var serializedObject = new SerializedObject(this);
            var serializedProperty = serializedObject.FindProperty("localizationData");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();

            GUILayout.EndScrollView();


            if (GUILayout.Button("Save localization data", GUILayout.Height(30))) SaveGameData();

            GUILayout.Space(20);
        }

        if (GUILayout.Button("Load localization data")) LoadGameData();

        if (GUILayout.Button("Create new localization data")) CreateNewData();

        GUILayout.Space(10);
    }

    [MenuItem("Window/Localization Editor")]
    private static void Init()
    {
        GetWindow(typeof(LocalizedTextEditor)).Show();
    }

    private void LoadGameData()
    {
        filePath = EditorUtility.OpenFilePanel("Select localization data file", Application.streamingAssetsPath,
            "json");
        key = "";
        value = "";
        if (!string.IsNullOrEmpty(filePath))
        {
            var dataAsJson = File.ReadAllText(filePath);
            filename = Path.GetFileName(filePath);
            localizationData = JsonUtility.FromJson<LocalizationData>(dataAsJson);
        }
    }

    private void SaveGameData()
    {
        //filePath = EditorUtility.SaveFilePanel("Save localization data file", Application.streamingAssetsPath, "", "json");

        if (!string.IsNullOrEmpty(filePath))
        {
            var dataAsJson = JsonUtility.ToJson(localizationData);
            File.WriteAllText(filePath, dataAsJson);
        }
    }

    private void CreateNewData()
    {
        localizationData = new LocalizationData();
    }
}