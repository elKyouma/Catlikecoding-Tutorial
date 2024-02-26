using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct Cylinder : IMeshGenetator
    {
        public int Resolution { get; set; }
        public int JobLength => Resolution + 1;
        public int VertexCount => (Resolution + 1) * (Resolution + 1);
        public int IndexTriangleCount => 6 * Resolution * Resolution;

        public Bounds Bounds => new(new Vector3(0.5f, 0f, 0.5f), new Vector3(1f, 1f, 1f));

        private int GetIndex(float x, float z) => (int)x + (int)z * (Resolution + 1);
        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams
        {
            Vertex tempVertex;

            float z = index;
            int ti = 2 * Resolution * (index - 1) - 2;
            for (int i = 0; i < Resolution + 1; i++, ti += 2)
            {
                float x = i;

                tempVertex.position = float3(cos(2 * PI * x / Resolution), sin(2 * PI * z / Resolution), sin(2 * PI * x / Resolution));
                tempVertex.normal = normalize(float3(tempVertex.position.x, 0.0f, tempVertex.position.z));
                tempVertex.tangent = float4(cross(tempVertex.normal, up()),-1f);
                tempVertex.uv = float2(x / Resolution, z / Resolution);
                streams.SetVertex(GetIndex(x, z), tempVertex);

                if (z > 0 && x > 0)
                {
                    streams.SetTriangle(ti, int3(GetIndex(x - 1, z - 1), GetIndex(x - 1, z), GetIndex(x, z - 1)));
                    streams.SetTriangle(ti + 1, int3(GetIndex(x - 1, z), GetIndex(x, z), GetIndex(x, z - 1)));
                }
            }
        }
    }
}
