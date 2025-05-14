using System;
using UnityEngine;

[ExecuteInEditMode]
public class CylinderColliderGenerator : MonoBehaviour
{
    [Header("Cylinder Settings")]
    public float height = 2f;
    public float radius = 0.5f;
    [Range(3, 64)] public int sides = 16;
    public Vector3 center = Vector3.zero;
    public Vector3 rotation = Vector3.zero;

    [SerializeField] private MeshCollider _meshCollider;
    [SerializeField] private Mesh _currentMesh;

    private void OnValidate()
    {
        height = Mathf.Max(0.1f, height);
        radius = Mathf.Max(0.1f, radius);
        sides = Mathf.Clamp(sides, 3, 64);

        if (!Application.isPlaying)
        {
            UpdateCollider(CreateCylinderMesh());
        }
    }

    public void UpdateCollider(Mesh mesh)
    {
        if (_meshCollider == null)
        {
            _meshCollider = GetComponent<MeshCollider>();
            if (_meshCollider == null)
            {
                _meshCollider = gameObject.AddComponent<MeshCollider>();
            }
        }

        if (_currentMesh != null && _currentMesh != mesh)
        {
            DestroyImmediate(_currentMesh);
        }

        _currentMesh = mesh;
        _meshCollider.sharedMesh = _currentMesh;
        _meshCollider.convex = true;
    }

    public Mesh CreateCylinderMesh()
    {
        Mesh mesh = new();
        int side_double = 2 * sides;
        int vertexCount = side_double + 2;
        int triangleCount = 12 * sides;

        // Use arrays directly (Unity's Mesh API requires managed arrays)
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Quaternion rot = Quaternion.Euler(rotation);
        float angleStep = 2 * Mathf.PI / sides;

        // Create vertices
        for (int i = 0; i < sides; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[2 * i] = rot * new Vector3(x, height / 2, z) + center;
            vertices[2 * i + 1] = rot * new Vector3(x, -height / 2, z) + center;
        }

        // Add center points
        int topCenterIndex = side_double;
        int bottomCenterIndex = side_double + 1;
        vertices[topCenterIndex] = rot * new Vector3(0, height / 2, 0) + center;
        vertices[bottomCenterIndex] = rot * new Vector3(0, -height / 2, 0) + center;

        // Use Span for efficient array operations
        Span<int> triangleSpan = triangles.AsSpan();

        // Create side triangles
        for (int i = 0; i < sides; i++)
        {
            int i_double = 2 * i;
            int offset = i * 6;
            int currentTop = i_double;
            int currentBottom = i_double + 1;
            int nextTop = 2 * ((i + 1) % sides);
            int nextBottom = nextTop + 1;

            var slice = triangleSpan.Slice(offset, 6);
            slice[0] = currentTop;
            slice[1] = currentBottom;
            slice[2] = nextTop;
            slice[3] = nextTop;
            slice[4] = currentBottom;
            slice[5] = nextBottom;
        }

        // Create top cap
        int topOffset = 6 * sides;
        for (int i = 0; i < sides; i++)
        {
            int offset = topOffset + i * 3;
            triangleSpan[offset] = topCenterIndex;
            triangleSpan[offset + 1] = 2 * i;
            triangleSpan[offset + 2] = 2 * ((i + 1) % sides);
        }

        // Create bottom cap
        int bottomOffset = 9 * sides;
        for (int i = 0; i < sides; i++)
        {
            int offset = bottomOffset + i * 3;
            triangleSpan[offset] = bottomCenterIndex;
            triangleSpan[offset + 1] = 2 * ((i + 1) % sides) + 1;
            triangleSpan[offset + 2] = 2 * i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
