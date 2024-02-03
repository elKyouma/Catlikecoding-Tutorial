using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SquareGrid : IMeshGenetator
    {
        public int JobLength => 1;
        public int VertexCount => 4;
        public int IndexTriangleCount => 6;

        public Bounds Bounds => new (new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams 
        {
            Vertex tempVertex = new() { normal = back(), tangent = float4(1f, 0f, 0f, -1f) };
            
            tempVertex.position = 0f;
            tempVertex.uv = 0f;
            streams.SetVertex(0, tempVertex);

            tempVertex.position = right();
            tempVertex.uv = float2(1f, 0f);
            streams.SetVertex(1, tempVertex);

            tempVertex.position = up();
            tempVertex.uv = float2(0f, 1f);
            streams.SetVertex(2, tempVertex);
            
            tempVertex.position = right() + up();
            tempVertex.uv = 1f;
            streams.SetVertex(3, tempVertex);

            streams.SetTriangle(0, int3(0, 2, 1));
            streams.SetTriangle(1, int3(2, 3, 1));
        }
    }
}