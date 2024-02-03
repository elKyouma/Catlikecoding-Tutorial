using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralMeshes
{
    public interface IMeshStreams
    {
        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount);
        public void SetVertex(int index, Vertex data);
        public void SetTriangle(int index, int3 triangle);
    }
}
