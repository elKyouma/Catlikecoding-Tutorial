using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;

using static Unity.Mathematics.math;
using float3x4 = Unity.Mathematics.float3x4;
using float4x4 = Unity.Mathematics.float4x4;
using Unity.Collections;
using UnityEngine;

public struct Point4
{
    public float4x3 position, normal;
}

public interface IShape
{
    Point4 GetPoint4(int index, int resolutotion, float invResolution);
}

public static class Shapes
{

    public delegate JobHandle ScheduleDelegate(
        NativeArray<float3x4> positions, NativeArray<float3x4> normals,
        int resolution, float4x4 trs, JobHandle dependency
    );

    public static float4x2 GetUV(int index, int resolution, float invResolution)
    {
        float4 i4 = 4f * index + float4(0f, 1f, 2f, 3f);

        float4x2 uv;
        uv.c1 = floor(invResolution * i4 + 0.00001f);
        uv.c0 = invResolution * (i4 - resolution * uv.c1 + 0.5f);
        uv.c1 = invResolution * (uv.c1 + 0.5f);

        return uv;
    }

    public struct Plane : IShape
    {
        public Point4 GetPoint4(int index, int resolutotion, float invResolution)
        {
            float4x2 uv = GetUV(index, resolutotion, invResolution);
            return new Point4
            {
                position = float4x3(uv.c0 - 0.5f, 0f, uv.c1 - 0.5f),
                normal = float4x3(0f, 1f, 0f)
            };
        }
    }

    public struct Cone : IShape
    {
        public Point4 GetPoint4(int index, int resolutotion, float invResolution)
        {
            float sR = 0.5f;

            float4x2 uv = GetUV(index, resolutotion, invResolution);
            float4 r = sR * uv.c1;

            Point4 p;
            p.position = float4x3(r * sin(2 * PI * uv.c0), uv.c1 - 0.5f, r * cos(2 * PI * uv.c0));
            p.normal = p.position * 2;

            return p;
        }
    }
    public struct UVSphere : IShape
    {
        public Point4 GetPoint4(int index, int resolutotion, float invResolution)
        {
            float4x2 uv = GetUV(index, resolutotion, invResolution);
            float sR = 0.5f;
            float4 r = sR * sin(PI * uv.c1);

            Point4 p;
            p.position = float4x3(r * cos(2 * PI * uv.c0), sR * cos(PI * uv.c1), r * sin(2 * PI * uv.c0));
            p.normal = p.position * 2;

            return p;
        }
    }

    public struct Torus : IShape
    {
        public Point4 GetPoint4(int index, int resolutotion, float invResolution)
        {
            const float r1 = 0.5f; // inner circle radius
            const float r2 = 0.25f; // thickness

            float4x2 uv = GetUV(index, resolutotion, invResolution);

            Point4 p;
            p.position = float4x3(
                (r1 + r2 * (1 + sin(uv.c1 * 2 * PI))) * cos(uv.c0 * 2 * PI),
                r2 * cos(uv.c1 * 2 * PI),
                (r1 + r2 * (1 + sin(uv.c1 * 2 * PI))) * sin(uv.c0 * 2 * PI));
            p.normal = p.position - float4x3((r1 + r2) * cos(uv.c0 * 2 * PI), 0f, (r1 + r2) * sin(uv.c0 * 2 * PI));
            return p;
        }
    }

    public struct Sphere : IShape
    {
        public Point4 GetPoint4(int index, int resolutotion, float invResolution)
        {
            float4x2 uv = GetUV(index, resolutotion, invResolution);

            float4 xPos = select(uv.c0 - 0.5f, select(uv.c0, uv.c0 - 1f, uv.c0 > 0.5f), abs(uv.c0 - 0.5f) + abs(uv.c1 - 0.5f) > 0.5f);
            float4 zPos = select(uv.c1 - 0.5f, select(uv.c1, uv.c1 - 1f, uv.c1 > 0.5f), abs(uv.c0 - 0.5f) + abs(uv.c1 - 0.5f) > 0.5f);


            Point4 p;
            p.position = float4x3(xPos, abs(uv.c0 - 0.5f) + abs(uv.c1 - 0.5f) - 0.5f, zPos);
            float4 scale = 0.5f * rsqrt(p.position.c0 * p.position.c0 + 
                                        p.position.c1 * p.position.c1 + 
                                        p.position.c2 * p.position.c2);

            p.position.c0 *= scale;
            p.position.c1 *= scale;
            p.position.c2 *= scale;
            p.normal = p.position;
            return p;
        }
    }


    public struct Job<SHAPE> : IJobFor where SHAPE : struct, IShape
    {
        [ReadOnly]
        public int resolution;
        [ReadOnly]
        public float invResolution;

        [ReadOnly]
        public float3x4 posTrs;
        [ReadOnly]
        public float3x4 normalTrs;

        [WriteOnly]
        public NativeArray<float3x4> positions;
        [WriteOnly]
        public NativeArray<float3x4> normals;


        private void ApplyTrs(int index, Point4 p)
        {
            positions[index] = MathExtension.Transform(posTrs, p.position);
            normals[index] = MathExtension.Transform(normalTrs, p.normal);
        }
        public void Execute(int index)
        {
            ApplyTrs(index, default(SHAPE).GetPoint4(index, resolution, invResolution));
        }

        public static JobHandle ScheduleParallel(NativeArray<float3x4> positions, NativeArray<float3x4> normals, int resolution, float4x4 trs, JobHandle dependency)
        {
            float4x4 tim = transpose(inverse(trs));

            return new Job<SHAPE>
            {
                positions = positions,
                normals = normals,
                resolution = resolution,
                invResolution = 1f / resolution,
                posTrs = float3x4(trs.c0.xyz, trs.c1.xyz, trs.c2.xyz, trs.c3.xyz),
                normalTrs = float3x4(tim.c0.xyz, tim.c1.xyz, tim.c2.xyz, tim.c3.xyz)
            }.ScheduleParallel(resolution * resolution / 4, resolution / 4, dependency);
        }
    }
}
