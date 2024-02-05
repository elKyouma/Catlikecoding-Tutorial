using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SquareGrid : IMeshGenetator
    {
        public int Resolution { get; set; }
        public int JobLength => Resolution;
        public int VertexCount => 4 * Resolution * Resolution;
        public int IndexTriangleCount => 6 * Resolution * Resolution;

        public Bounds Bounds => new(new Vector3(0.5f, 0f, 0.5f), new Vector3(1f, 1f, 1f));


        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams
        {
            Vertex tempVertex = new() { normal = up(), tangent = float4(1f, 0f, 0f, -1f) };

            float z = index;
            int vi = 4 * Resolution * index, ti = 2 * Resolution * index;
            for (int i = 0; i < Resolution; i++, vi+=4, ti +=2)
            {
                float x = i;

                float3 relativePosition = float3(x / Resolution - 0.5f, 0f, z / Resolution - 0.5f);
                tempVertex.position = relativePosition;
                tempVertex.uv = 0f;
                streams.SetVertex(vi, tempVertex);

                tempVertex.position = relativePosition + right() / Resolution;
                tempVertex.uv = float2(1f, 0f);
                streams.SetVertex(vi + 1, tempVertex);

                tempVertex.position = relativePosition + forward() / Resolution;
                tempVertex.uv = float2(0f, 1f);
                streams.SetVertex(vi + 2, tempVertex);

                tempVertex.position = relativePosition + right() / Resolution + forward() / Resolution;
                tempVertex.uv = 1f;
                streams.SetVertex(vi + 3, tempVertex);

                streams.SetTriangle(ti, int3(vi, vi + 2, vi + 1));
                streams.SetTriangle(ti + 1, int3(vi + 2, vi + 3, vi + 1));
            }
        }
    }
}