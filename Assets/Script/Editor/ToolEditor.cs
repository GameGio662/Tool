using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class ToolEditor : EditorWindow
{
    [MenuItem("GIO/ToolEditor")]
    public static void OpenWindow() => GetWindow<ToolEditor>();

    public float gridSize = 1f;

    private GameObject stanzaSelezionata;

    SerializedObject so;
    GameObject[] Stanze;

    [SerializeField] bool[] selected;

    private void OnEnable()
    {
        so = new SerializedObject(this);
        SceneView.duringSceneGui += OnSceneGUI;
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
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        so.Update();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        ButtonPreview();
        HandleInput();
        DrawPrefabOnCursor(stanzaSelezionata);
    }

    private void HandleInput()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control)
        {
            if (e.keyCode == KeyCode.Q)
            {
                RotatePrefab(-90f); // Ruota di -90° quando premi Ctrl+Q
            }
            else if (e.keyCode == KeyCode.E)
            {
                RotatePrefab(90f); // Ruota di 90° quando premi Ctrl+E
            }
        }
        else if (e.type == EventType.MouseDown && e.button == 0)
        {
            InstantiatePrefab();
        }
    }

    private void InstantiatePrefab()
    {
        if (stanzaSelezionata != null)
        {
            Undo.RecordObject(this, "Istanza prefab");

            // Applica la rotazione al prefab prima di istanziarlo
            RotatePrefab(0f);

            GameObject istanzaPrefab = PrefabUtility.InstantiatePrefab(stanzaSelezionata) as GameObject;
            if (istanzaPrefab != null)
            {
                Vector3 worldPosition = GetWorldPosition(Event.current.mousePosition);
                istanzaPrefab.transform.position = worldPosition;
                Undo.RegisterCreatedObjectUndo(istanzaPrefab, "Istanza prefab");
                Debug.Log("Prefab istanziato");
            }
        }
    }

    private void RotatePrefab(float angle)
    {
        if (stanzaSelezionata != null)
        {
            Undo.RecordObject(stanzaSelezionata.transform, "Rotazione prefab");
            stanzaSelezionata.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void DrawSelectedObj(GameObject obj)
    {
        if (obj == null)
            return;

        EditorGUILayout.Space();
        GUILayout.Label("Selected Object", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
        EditorGUI.EndDisabledGroup();
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private void ButtonPreview()
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
                stanzaSelezionata = prefab;
                Debug.Log(stanzaSelezionata);
            }

            rect.y += rect.height + 5;
        }
        Handles.EndGUI();
    }

    private void DrawPrefabOnCursor(GameObject prefab)
    {
        if (prefab != null && Event.current.type == EventType.Repaint)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 cursorPosition = hit.point;
                Quaternion cursorRotation = Quaternion.LookRotation(hit.normal);

                GameObject previewObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (previewObject != null)
                {
                    // Offset per evitare di intersecare il terreno
                    cursorPosition += hit.normal * 0.5f;

                    previewObject.transform.position = cursorPosition;
                    previewObject.transform.rotation = cursorRotation;
                    previewObject.transform.localScale = prefab.transform.localScale;
                    DestroyImmediate(previewObject);
                }
            }
        }
    }
}