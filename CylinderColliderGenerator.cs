using UnityEngine;

[ExecuteInEditMode]
public class CylinderColliderGenerator : MonoBehaviour
{
    [Header("Cylinder Settings")]
    public float height = 2f;
    public float radius = 0.5f;
    [Range(3, 64)] public int sides = 16;

    private MeshCollider _meshCollider;
    private Mesh _currentMesh;

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

        // Clear previous mesh if it wasn't saved as asset
        if (_currentMesh != null && _currentMesh != mesh)
        {
            DestroyImmediate(_currentMesh);
        }

        _currentMesh = mesh;
        _meshCollider.sharedMesh = _currentMesh;
        _meshCollider.convex = true;
    }

    private void OnValidate()
    {
        height = Mathf.Max(0.1f, height);
        radius = Mathf.Max(0.1f, radius);
        sides = Mathf.Clamp(sides, 3, 64);
    }
}