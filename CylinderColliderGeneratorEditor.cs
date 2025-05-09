#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(CylinderColliderGenerator))]
public class CylinderColliderGeneratorEditor : Editor
{
    private string _meshName = "CylinderCollider";
    private string _savePath = "";
    private bool _savePathModified = false;
    private SerializedProperty _heightProp, _radiusProp, _sidesProp;

    private void OnEnable()
    {
        _heightProp = serializedObject.FindProperty("height");
        _radiusProp = serializedObject.FindProperty("radius");
        _sidesProp = serializedObject.FindProperty("sides");
        UpdateDefaultSavePath();
    }

    public override void OnInspectorGUI()
    {
        var generator = (CylinderColliderGenerator)target;

        serializedObject.Update();

        float prevHeight = _heightProp.floatValue;
        float prevRadius = _radiusProp.floatValue;
        int prevSides = _sidesProp.intValue;

        EditorGUILayout.PropertyField(_heightProp);
        EditorGUILayout.PropertyField(_radiusProp);
        EditorGUILayout.PropertyField(_sidesProp);

        serializedObject.ApplyModifiedProperties();

        bool valuesChanged = prevHeight != _heightProp.floatValue ||
                           prevRadius != _radiusProp.floatValue ||
                           prevSides != _sidesProp.intValue;

        if (valuesChanged)
        {
            GeneratePreviewMesh(generator);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Asset Settings", EditorStyles.boldLabel);

        _meshName = EditorGUILayout.TextField("Mesh Name", _meshName);

        EditorGUI.BeginChangeCheck();
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);
        if (EditorGUI.EndChangeCheck())
        {
            _savePathModified = true;
        }

        if (GUILayout.Button("Reset to Default Path"))
        {
            _savePathModified = false;
            UpdateDefaultSavePath();
        }

        if (!_savePathModified)
        {
            UpdateDefaultSavePath();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Save as New Mesh Asset"))
        {
            SaveCurrentMesh(generator);
        }
    }

    private void UpdateDefaultSavePath()
    {
        string newPath = GetCurrentProjectFolder();
        if (!_savePathModified)
        {
            _savePath = newPath;
        }
    }

    private string GetCurrentProjectFolder()
    {
        string folderPath = "Assets/SavedCustomColliders";

        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    folderPath = path;
                }
                else
                {
                    folderPath = System.IO.Path.GetDirectoryName(path);
                }
            }
        }

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/SavedCustomColliders"))
            {
                AssetDatabase.CreateFolder("Assets", "SavedCustomColliders");
            }
            folderPath = "Assets/SavedCustomColliders";
        }

        return folderPath + "/";
    }

    private void GeneratePreviewMesh(CylinderColliderGenerator generator)
    {
        Mesh previewMesh = CreateCylinderMesh(
            generator.height,
            generator.radius,
            generator.sides
        );
        generator.UpdateCollider(previewMesh);
    }

    private void SaveCurrentMesh(CylinderColliderGenerator generator)
    {
        Mesh meshToSave = CreateCylinderMesh(
            generator.height,
            generator.radius,
            generator.sides
        );
        SaveMeshAsAsset(meshToSave);
        generator.UpdateCollider(meshToSave);
    }

    private Mesh CreateCylinderMesh(float height, float radius, int sides)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Generate side vertices
        for (int i = 0; i < sides; i++)
        {
            float angle = i * 2 * Mathf.PI / sides;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices.Add(new Vector3(x, height / 2, z));  // Top
            vertices.Add(new Vector3(x, -height / 2, z)); // Bottom
        }

        // Add center vertices
        int topCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0, height / 2, 0));
        int bottomCenterIndex = vertices.Count;
        vertices.Add(new Vector3(0, -height / 2, 0));

        // Generate side triangles
        for (int i = 0; i < sides; i++)
        {
            int currentTop = i * 2;
            int currentBottom = i * 2 + 1;
            int nextTop = ((i + 1) % sides) * 2;
            int nextBottom = ((i + 1) % sides) * 2 + 1;

            triangles.Add(currentTop);
            triangles.Add(currentBottom);
            triangles.Add(nextTop);

            triangles.Add(nextTop);
            triangles.Add(currentBottom);
            triangles.Add(nextBottom);
        }

        // Generate top cap
        for (int i = 0; i < sides; i++)
        {
            int current = i * 2;
            int next = ((i + 1) % sides) * 2;

            triangles.Add(topCenterIndex);
            triangles.Add(current);
            triangles.Add(next);
        }

        // Generate bottom cap
        for (int i = 0; i < sides; i++)
        {
            int current = i * 2 + 1;
            int next = ((i + 1) % sides) * 2 + 1;

            triangles.Add(bottomCenterIndex);
            triangles.Add(next);
            triangles.Add(current);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    private void SaveMeshAsAsset(Mesh mesh)
    {
        if (string.IsNullOrEmpty(_meshName))
        {
            _meshName = "CylinderCollider";
        }

        string finalPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{_savePath}{_meshName}.asset"
        );

        AssetDatabase.CreateAsset(mesh, finalPath);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = mesh;
        Debug.Log($"Saved collider mesh to: {finalPath}");
    }
}
#endif