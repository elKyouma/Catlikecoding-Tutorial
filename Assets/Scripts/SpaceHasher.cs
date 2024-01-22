using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;

using static Unity.Mathematics.math;
using float3x4 = Unity.Mathematics.float3x4;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;
using Unity.Collections;
using System;

using static MathExtension;

public class SpaceHasher : Visualisation
{
    [SerializeField]
    private SpaceTRS domain;

    [SerializeField]
    private int seed;

    private ComputeBuffer hashBuffer;
    private NativeArray<uint> hashes;
    static readonly int hashesId = Shader.PropertyToID("_Hashes");

    protected override void EnableVisualization(int resolution)
    {
        hashBuffer = new ComputeBuffer(resolution * resolution, sizeof(uint));
        hashes = new NativeArray<uint>(resolution * resolution, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        material.SetBuffer(hashesId, hashBuffer);
    }

    protected override void UpdateVisualization( NativeArray<float3x4> positions, int resolution, JobHandle handle)
    { 
        new HashJob
        {
            positions = positions,
            hashes = hashes.Reinterpret<uint4>(4),
            hash = new SmallXXHash4(SmallXXHash4.Seed(seed)),
            domainTRS = domain.Matrix
        }.ScheduleParallel(resolution * resolution / 4, resolution / 4, handle).Complete();

        hashBuffer.SetData(hashes);
    }

    protected override void DisableVisualization()
    {
        hashes.Dispose();
        hashBuffer?.Release();
        hashBuffer = null;
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {
        [ReadOnly] public SmallXXHash4 hash;
        [ReadOnly] public NativeArray<float3x4> positions;
        [ReadOnly] public float3x4 domainTRS;

        [WriteOnly]
        public NativeArray<uint4> hashes;
        public void Execute(int index)
        {
            float3x4 transformedPos = Transform(domainTRS, index);

            float4x3 p = transpose(transformedPos);

            int4 u = (int4)floor(p.c0);
            int4 v = (int4)floor(p.c1);
            int4 w = (int4)floor(p.c2);

            hashes[index] = hash.Eat(u).Eat(v).Eat(w);
        }
    }
}
