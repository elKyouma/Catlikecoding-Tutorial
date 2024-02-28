using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct UVSphere : IMeshGenetator
    {
        public int Resolution { get; set; }
        public int ResolutionU => Resolution * 4;
        public int ResolutionV => Resolution * 2;
        public int JobLength => ResolutionU + 1;
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1);
        public int IndexTriangleCount => 6 * ResolutionU * ResolutionV;

        public Bounds Bounds => new(new Vector3(0.5f, 0f, 0.5f), new Vector3(1f, 1f, 1f));

        public float DebugParam { get; set; }

        private int GetIndex(float x, float z) => (int)x + (int)z * (ResolutionU + 1);

        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams
        {
            Vertex tempVertex;

            float x = index;
            int ti = 2 * ResolutionV * (index - 1);

            //first
            tempVertex.position = up();
            tempVertex.normal = up();
            tempVertex.tangent = float4(cos(2 * PI * (x - 0.5f) / ResolutionU), 0f, sin(2 * PI * (x - 0.5f) / ResolutionU), -1f);
            tempVertex.uv = float2((x - 0.5f) / ResolutionU, 0f);
            streams.SetVertex(GetIndex(x, 0f), tempVertex);

            //last
            tempVertex.position = down();
            tempVertex.normal = down();
            tempVertex.tangent = float4(cos(2 * PI * (x - 0.5f) / ResolutionU), 0f, sin(2 * PI * (x - 0.5f) / ResolutionU), -1f);
            tempVertex.uv = float2((x + 0.5f) / ResolutionU, -1f);
            streams.SetVertex(GetIndex(x, ResolutionV), tempVertex);

            for (int i = 1; i < ResolutionV; i++, ti += 2)
            {
                float z = i;

                tempVertex.position = float3(cos(2 * PI * x / ResolutionU) * sin(PI * z / ResolutionV),
                                                                            cos(PI * z / ResolutionV),
                                            sin(2 * PI * x / ResolutionU) * sin(PI * z / ResolutionV));
                tempVertex.normal = normalize(tempVertex.position);
                tempVertex.tangent = float4(cross(float3(tempVertex.normal.x, 0f, tempVertex.normal.z), up()), -1f);

                tempVertex.uv = float2(x / ResolutionU, -z / ResolutionV);
                streams.SetVertex(GetIndex(x, z), tempVertex);

                if (x > 0.5f)
                {
                    streams.SetTriangle(ti, int3(GetIndex(x - 1, z - 1), GetIndex(x, z - 1), GetIndex(x - 1, z)));
                    streams.SetTriangle(ti + 1, int3(GetIndex(x - 1, z), GetIndex(x, z - 1), GetIndex(x, z)));
                }
            }
            if (x > 0.5f)
            {
                streams.SetTriangle(ti, int3(GetIndex(x - 1, ResolutionV - 1), GetIndex(x, ResolutionV - 1), GetIndex(x - 1, ResolutionV)));
                streams.SetTriangle(ti + 1, int3(GetIndex(x - 1, ResolutionV), GetIndex(x, ResolutionV - 1), GetIndex(x, ResolutionV)));
            }
        }
    }
}
