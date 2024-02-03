using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using Unity.Mathematics;

using static Unity.Mathematics.math;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MultiStreamProceduralMesh : MonoBehaviour
{
    private void OnEnable()
    {
        const int VERTEX_ATTRIBUTE_COUNT = 4;
        const int VERTEX_COUNT = 4;
        const int INDEX_TRIANGLE_COUNT = 6;

        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);

        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(VERTEX_ATTRIBUTE_COUNT, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4, 2);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2, 3);

        meshData.SetVertexBufferParams(VERTEX_COUNT, vertexAttributes);
        meshData.SetIndexBufferParams(INDEX_TRIANGLE_COUNT, IndexFormat.UInt16);

        Mesh mesh = new()
        {
            name = "ProceduralMesh"
        };
        NativeArray<float3> positions = meshData.GetVertexData<float3>(0);
        positions[0] = 0f;
        positions[1] = right();
        positions[2] = up();
        positions[3] = right() + up();
        
        NativeArray<float3> normals = meshData.GetVertexData<float3>(1);
        normals[0] = normals[1] = normals[2] = normals[3] = back();
        NativeArray<half4> tangents = meshData.GetVertexData<half4>(2);
        tangents[0] = tangents[1] = tangents[2] = tangents[3] = half4(half(1f), half(0f), half(0f), half(-1f));
        
        NativeArray<half2> uvs = meshData.GetVertexData<half2>(3);
        uvs[0] = half2(0f);
        uvs[1] = half2(half(1f), half(0f));
        uvs[2] = half2(half(0f), half(1f));
        uvs[3] = half2(1f);

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
