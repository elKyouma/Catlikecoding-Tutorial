using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralMeshes
{
    public struct Vertex
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 uv;
    }
}