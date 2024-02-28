using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SharedTriangleGrid : IMeshGenetator
    {
        public int Resolution { get; set; }
        public int JobLength => Resolution + 1;
        public int VertexCount => (Resolution + 1) * (Resolution + 1);
        public int IndexTriangleCount => 6 * Resolution * Resolution;

        public Bounds Bounds => new(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));

        public float DebugParam { get; set; }

        private int GetIndex(float x, float z) => (int)x + (int)z * (Resolution + 1);
        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams
        {
            Vertex tempVertex = new() { normal = up(), tangent = float4(1f, 0f, 0f, -1f) };

            float zOffset = 0.5f - sqrt(3)/4f;
            float z = index * sqrt(3)/2;
            int ti = 2 * Resolution * (index - 1) - 2;
            
            float offsetPos = (index & 1) == 1 ? 0.25f : -0.25f;
            offsetPos /= Resolution;
            float offsetUV = offsetPos;
            offsetPos += 0.5f;

            for (int i = 0; i < Resolution + 1; i++, ti += 2)
            {
                float x = i;

                tempVertex.position = float3(x / Resolution - offsetPos, 0f, z / Resolution - 0.5f + zOffset);
                tempVertex.uv = float2(x / Resolution - offsetUV, z / Resolution + zOffset);
                streams.SetVertex(GetIndex(x, index), tempVertex);

                if (z == 0 || x == 0) continue;

                if ((index & 1) == 0)
                {
                    streams.SetTriangle(ti, int3(GetIndex(x - 1, index - 1), GetIndex(x - 1, index), GetIndex(x, index - 1)));
                    streams.SetTriangle(ti + 1, int3(GetIndex(x - 1, index), GetIndex(x, index), GetIndex(x, index - 1)));
                }
                else
                {
                    streams.SetTriangle(ti, int3(GetIndex(x - 1, index - 1), GetIndex(x - 1, index), GetIndex(x, index)));
                    streams.SetTriangle(ti + 1, int3(GetIndex(x - 1, index - 1), GetIndex(x, index), GetIndex(x, index - 1)));
                }
            }
        }
    }
}