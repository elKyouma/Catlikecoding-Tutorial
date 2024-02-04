using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace ProceduralMeshes.Streams
{
    public struct SingleStream : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Stream0
        {
            public float3 position;
            public float3 normal;
            public float4 tangent;
            public float2 uv;
        }

        [NativeDisableContainerSafetyRestriction]
        NativeArray<Stream0> stream0;
        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> triangles;
        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4);
            vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<int>().Reinterpret<TriangleUInt16>(4);

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
        public void SetVertex(int index, Vertex data) => stream0[index] = new Stream0{
                position = data.position,
                normal = data.normal,
                tangent = data.tangent,
                uv = data.uv
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
    }
}