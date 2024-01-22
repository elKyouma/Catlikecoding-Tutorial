using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.UIElements;

using static Unity.Mathematics.math;
using float3x4 = Unity.Mathematics.float3x4;
using float3x3 = Unity.Mathematics.float3x3;
using float3 = Unity.Mathematics.float3;

public class Fractal : MonoBehaviour
{
    [Range(1, 8)]
    public int depth;

    public Mesh mesh;
    public Material material;

    struct FractalPart
    {
        public float3 direction;
        public float3 worldPos;
        public float scale;
    }

    public ComputeBuffer[] fractal;
    NativeArray<float3x4>[] matrices;
    NativeArray<FractalPart>[] parts;

    static readonly int matricesId = Shader.PropertyToID("_Matrices"), baseColorId = Shader.PropertyToID("_BaseColor");
    private void OnEnable()
    {
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float3x4>[depth];
        fractal = new ComputeBuffer[depth];

        for (int i = 0, length = 1; i < depth; i++, length *= 20)
        {
            fractal[i] = new ComputeBuffer(length, 12 * sizeof(float));
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        UpdatePos();
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<FractalPart> parents;


        [ReadOnly]
        public readonly static float3[] directions = {
            float3( -1f, -1f, -1f ),
            float3( -1f, -1f, 0f ),
            float3( -1f, -1f, 1f ),

            float3( -1f, 0f, -1f ),
            float3( -1f, 0f, 1f ),

            float3( -1f, 1f, -1f ),
            float3( -1f, 1f, 0f ),
            float3( -1f, 1f, 1f ),


            float3( 0f, -1f, -1f ),
            float3( 0f, -1f, 1f ),
            float3( 0f, 1f, -1f ),
            float3( 0f, 1f, 1f ),

            float3( 1f, -1f, -1f ),
            float3( 1f, -1f, 0f ),
            float3( 1f, -1f, 1f ),

            float3( 1f, 0f, -1f ),
            float3( 1f, 0f, 1f ),

            float3( 1f, 1f, -1f ),
            float3( 1f, 1f, -0f ),
            float3( 1f, 1f, 1f )
        };

        [WriteOnly]
        public NativeArray<FractalPart> parts;
        [WriteOnly]
        public NativeArray<float3x4> matrices;

        [ReadOnly]
        public Quaternion rotation;

        public void Execute(int index)
        {
            FractalPart parent = parents[index/20];
            float3 direction = directions[index%20];

            var part = new FractalPart { worldPos = parent.worldPos + direction * parent.scale / 3, direction = direction, scale = parent.scale / 3 };

            float3x3 r = float3x3(rotation) * part.scale;
            matrices[index] = float3x4(r.c0, r.c1, r.c2, rotation * part.worldPos);
            parts[index] = part;
        }
    }

    private void UpdatePos()
    {
        float3x3 r = float3x3(Quaternion.identity);
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, parts[0][0].worldPos);
        var part = new FractalPart { direction = float3.zero, worldPos = float3.zero, scale = transform.localScale.x };
        parts[0][0] = part;

        JobHandle jobHandle = default;
        for (int i = 1; i < depth; i++)
        {
            var job = new UpdateFractalLevelJob
            {
                matrices = matrices[i],
                parts = parts[i],
                parents = parts[i - 1],
                rotation = transform.rotation
            };

            jobHandle = job.Schedule(parts[i].Length, 20, jobHandle);
            jobHandle.Complete();

            matrices[i] = job.matrices;
            parts[i] = job.parts;
        }
    }

    private void Update()
    {
        UpdatePos();

        fractal[^1].SetData(matrices[^1]);
        material.SetBuffer(matricesId, fractal[^1]);
        material.SetColor(baseColorId, Color.red);

        RenderParams param = new(material);
        
        Graphics.RenderMeshPrimitives(param, mesh, 0, fractal[^1].count);
    }

    private void OnDisable()
    {
        for (int i = 0; i < depth; i++)
        {
            fractal[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }

        fractal = null;
        parts = null;
        matrices = null;
    }
}
