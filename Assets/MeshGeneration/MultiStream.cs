using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.CompilerServices;

namespace ProceduralMeshes.Streams
{
    public struct MultiStream : IMeshStreams
    {
        [NativeDisableContainerSafetyRestriction] NativeArray<float3> positions;
        [NativeDisableContainerSafetyRestriction] NativeArray<float3> normals;
        [NativeDisableContainerSafetyRestriction] NativeArray<float4> tangents;
        [NativeDisableContainerSafetyRestriction] NativeArray<float2> uvs;
        [NativeDisableContainerSafetyRestriction] NativeArray<TriangleUInt16> triangles;
        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 2);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 3);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
            
            positions = meshData.GetVertexData<float3>(0);
            normals =   meshData.GetVertexData<float3>(1);
            tangents =  meshData.GetVertexData<float4>(2);
            uvs =       meshData.GetVertexData<float2>(3);
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount, MeshTopology.Triangles)
            {
                vertexCount = vertexCount,
                bounds = bounds
            },
            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices
            );

            vertexAttributes.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, Vertex data)
        {
            positions[index] = data.position;
            normals[index] = data.normal;
            tangents[index] = data.tangent;
            uvs[index] = data.uv;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
    }
}
