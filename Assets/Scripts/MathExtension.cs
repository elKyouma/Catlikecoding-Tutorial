using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;
using float3x4 = Unity.Mathematics.float3x4;
using float4x3 = Unity.Mathematics.float4x3;
using float3 = Unity.Mathematics.float3;
using quaternion = Unity.Mathematics.quaternion;
using float4x4 = Unity.Mathematics.float4x4;
public static class MathExtension
{
    [System.Serializable]
    public struct SpaceTRS
    {
        public float3 translation, rotation, scale;
        public float3x4 Matrix
        {
            get
            {
                float4x4 m = float4x4.TRS(
                    translation, quaternion.EulerZXY(radians(rotation)), scale
                );
                return float3x4(m.c0.xyz, m.c1.xyz, m.c2.xyz, m.c3.xyz);
            }
        }
    }

    public static float3x4 Transform(float3x4 trs, float4x3 p)
    {
        return float3x4(mul(trs, float4(p.c0.x, p.c1.x, p.c2.x, 1f)),
                                    mul(trs, float4(p.c0.y, p.c1.y, p.c2.y, 1f)),
                                    mul(trs, float4(p.c0.z, p.c1.z, p.c2.z, 1f)),
                                    mul(trs, float4(p.c0.w, p.c1.w, p.c2.w, 1f)));
    }
}
