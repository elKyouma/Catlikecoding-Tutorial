using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public static partial class Noises
{
    public delegate JobHandle ScheduleDelegate(
    NativeArray<float3x4> positions, NativeArray<float4> noise,
    int resolution, float3x4 trs, NoiseSettings settings, float timeScale, JobHandle dependency
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

    public struct Job<NOISE> : IJobFor where NOISE : struct, INoise
    {
        [WriteOnly]
        public NativeArray<float4> noise;

        [ReadOnly]
        float4 time;
        [ReadOnly]
        public float3x4 trs;
        [ReadOnly]
        public NativeArray<float3x4> positions;
        [ReadOnly]
        public NoiseSettings settings;

        public void Execute(int index)
        {
            SmallXXHash4 hash = SmallXXHash4.Seed(settings.Seed);

            float4x3 transformedPos = transpose(MathExtension.Transform(trs, transpose(positions[index])));
            int freq = settings.Frequency;
            float persistance = 1;
            float sumAmp = 1;
            float4 sum = 0f;

            for (int i = 0; i < settings.Octaves; i++, freq*=settings.Lacunarity, persistance *= settings.Persistance, sumAmp += persistance)
                sum += default(NOISE).GetNoise4(float4x4(transformedPos.c0, transformedPos.c1, transformedPos.c2, time), hash + i, freq) * persistance;

            noise[index] = sum/sumAmp;
        }

        public static JobHandle ScheduleParallel(NativeArray<float3x4> positions, NativeArray<float4> noise, int resolution, float3x4 trs, NoiseSettings settings, float timeScale, JobHandle dependency)
        {
            return new Job<NOISE>
            {
                noise = noise,
                positions = positions,
                trs = trs,
                settings = settings,
                time = float4(Time.time * timeScale)
            }.ScheduleParallel(resolution * resolution / 4, resolution / 4, dependency);
        }
    }
}
