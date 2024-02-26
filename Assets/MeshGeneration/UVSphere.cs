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
        public int JobLength => ResolutionV + 1;
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1);
        public int IndexTriangleCount => 6 * ResolutionU * ResolutionV;

        public Bounds Bounds => new(new Vector3(0.5f, 0f, 0.5f), new Vector3(1f, 1f, 1f));

        private int GetIndex(float x, float z) => (int)x + (int)z * (ResolutionU + 1);
        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams
        {
            Vertex tempVertex;

            float z = index;
            int ti = 2 * ResolutionU * (index - 1) - 2;
            for (int i = 0; i < ResolutionU + 1; i++, ti += 2)
            {
                float x = i;

                tempVertex.position = float3(cos(2 * PI * x / ResolutionU) * sin(PI * z / ResolutionV),
                                                                            -cos(PI * z / ResolutionV),
                                            sin(2 * PI * x / ResolutionU) * sin(PI * z / ResolutionV));
                tempVertex.normal = normalize(tempVertex.position);
                tempVertex.tangent = float4(cross(float3(tempVertex.normal.x, 0f, tempVertex.normal.z), up()), -1f);
                tempVertex.uv = float2((x / ResolutionU), z / ResolutionV);
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
