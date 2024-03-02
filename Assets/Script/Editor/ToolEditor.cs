using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class ToolEditor : EditorWindow
{
    [MenuItem("GIO/ToolEditor")] public static void OpenWindow() => GetWindow<ToolEditor>();

    SerializedObject so;
    GameObject[] Stanze;

    [SerializeField] bool[] selected;


    private void OnEnable()
    {
        so = new SerializedObject(this);
        SceneView.duringSceneGui += DuringSceneGUI;
        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Prefab" });
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        Stanze = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();

        if (selected == null || selected.Length != Stanze.Length)
        {
            selected = new bool[Stanze.Length];
        }

    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }
    private void OnGUI()
    {
        so.Update();

    }

    void DuringSceneGUI(SceneView sv)
    {
        ButtonPreview();
    }

    void ButtonPreview()
    {
        Handles.BeginGUI();

        Rect rect = new Rect(10, 10, 100, 100);


        for (int i = 0; i < Stanze.Length; i++)
        {
            GameObject prefab = Stanze[i];

            Texture texture = AssetPreview.GetAssetPreview(prefab);
            EditorGUI.BeginChangeCheck();

            selected[i] = GUI.Button(rect, texture);

            if (selected[i])
            {
                Debug.Log("Stupido");
            }

            rect.y += rect.height + 5;
        }
        Handles.EndGUI();
    }

}
