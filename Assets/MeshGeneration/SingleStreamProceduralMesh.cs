using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SingleStreamProceduralMesh : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float3 position;
        public float3 normal;
        public half4 tangent;
        public half2 uv;
    }
    private void OnEnable()
    {
        const int VERTEX_ATTRIBUTE_COUNT = 4;
        const int VERTEX_COUNT = 4;
        const int INDEX_TRIANGLE_COUNT = 6;

        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);

        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(VERTEX_ATTRIBUTE_COUNT, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

        meshData.SetVertexBufferParams(VERTEX_COUNT, vertexAttributes);
        meshData.SetIndexBufferParams(INDEX_TRIANGLE_COUNT, IndexFormat.UInt16);

        Mesh mesh = new()
        {
            name = "ProceduralMesh"
        };

        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

        Vertex tempVertex = new Vertex {
            tangent = half4(half(1f), half(0f), half(0f), half(-1f)),
            normal = back()
        };

        tempVertex.position = 0f;
        tempVertex.uv = half2(0f);
        vertices[0] = tempVertex;

        tempVertex.position = right();
        tempVertex.uv = half2(half(1f), half(0f));
        vertices[1] = tempVertex;

        tempVertex.position = up();
        tempVertex.uv = half2(half(0f), half(1f));
        vertices[2] = tempVertex;

        tempVertex.position = right() + up();
        tempVertex.uv = half2(half(1f));
        vertices[3] = tempVertex;

        NativeArray<ushort> indices = meshData.GetIndexData<ushort>();
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;
        indices[3] = 2;
        indices[4] = 3;
        indices[5] = 1;


        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, INDEX_TRIANGLE_COUNT, MeshTopology.Triangles)
        {
            vertexCount = VERTEX_COUNT,
            bounds = bounds
        },
        MeshUpdateFlags.DontRecalculateBounds
        );
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh, MeshUpdateFlags.DontRecalculateBounds);
        GetComponent<MeshFilter>().mesh = mesh;

        vertexAttributes.Dispose();
    }
}
