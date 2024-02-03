using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DefaultMeshGeneration : MonoBehaviour
{
    private void OnEnable()
    {
        Mesh mesh = new()
        {
            name = "ProceduralMesh",
            vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up, Vector3.right + Vector3.up },
            triangles = new int[] { 0, 2, 1, 2, 3, 1 },
            normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back },
            uv = new Vector2[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.up + Vector2.right }
            
        };

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
