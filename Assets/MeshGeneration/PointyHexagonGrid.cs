using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct PointyHexagonGrid : IMeshGenetator
    {
        private static readonly int VertsPerHex = 7;
        public int Resolution { get; set; }
        public int JobLength => Resolution;
        public int VertexCount => VertsPerHex * Resolution * Resolution;
        public int IndexTriangleCount => 3 * 6 * Resolution * Resolution;

        private static readonly float[] pointsZ = {
            0f,
            0.5f, 0.25f, -0.25f,
            -0.5f, -0.25f, 0.25f
        };

        private static readonly float[] pointsX = {
            0f,
            0f, 0.25f * 1.73205081f, 0.25f * 1.73205081f,
            0f, -0.25f * 1.73205081f, -0.25f * 1.73205081f
        };

        public Bounds Bounds => new(new Vector3(0.5f, 0f, 0.5f), new Vector3(1f, 1f, 1f));

        public void Execute<Streams>(int zIndex, Streams streams) where Streams : struct, IMeshStreams
        {
            Vertex tempVertex;
            tempVertex.normal = back();
            tempVertex.tangent = float4(1f, 0f, 0f, -1f);

            int vi = zIndex * 7 * Resolution;
            int ti = zIndex * 6 * Resolution;

            for (int xIndex = 0; xIndex < Resolution; xIndex++, ti += 6)
            {
                streams.SetTriangle(ti + 0, vi + int3(0, 1, 2));
                streams.SetTriangle(ti + 1, vi + int3(0, 2, 3));
                streams.SetTriangle(ti + 2, vi + int3(0, 3, 4));
                streams.SetTriangle(ti + 3, vi + int3(0, 4, 5));
                streams.SetTriangle(ti + 4, vi + int3(0, 5, 6));
                streams.SetTriangle(ti + 5, vi + int3(0, 6, 1));

                for (int i = 0; i < VertsPerHex; i++, vi++)
                {
                    float hexRadius = (Resolution - 1f) / Resolution;
                    float3 gridOffset = float3(hexRadius * sqrt(3) * 0.25f, 0f, hexRadius / 3f);
                    float evenZOffset = (zIndex & 1) == 1 ? 0.5f / Resolution : 0f;
                    float3 centerPosition = float3((float)xIndex / Resolution * sqrt(3) * 0.5f, 0f, zIndex * 2f / 3f / Resolution);
                    float3 hexPointsOffset = float3(pointsX[i] / Resolution, 0f, pointsZ[i] / Resolution);

                    tempVertex.position = centerPosition;
                    tempVertex.position += hexPointsOffset;
                    tempVertex.position -= gridOffset;
                    tempVertex.position.x += evenZOffset;
                    tempVertex.uv = float2(tempVertex.position.x - 0.5f, tempVertex.position.z - 0.5f);
                    streams.SetVertex(vi, tempVertex);
                }

                //for (int i = 1; i < VertexCount - 1; i++, ti++)
                //    streams.SetTriangle(ti, int3(vi - 7, vi - 7 + i, vi - 6 + i));

                //streams.SetTriangle(ti, int3(vi - 7, vi - 1, vi - 6));
                //ti++;
            }
        }
    }
}