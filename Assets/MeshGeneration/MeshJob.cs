using ProceduralMeshes.Generators;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralMeshes
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct MeshJob<Generator, Streams> : IJobFor
        where Generator : struct, IMeshGenetator
        where Streams : struct, IMeshStreams
    {
        Generator generator;
        
        [WriteOnly]
        public Streams streams;

        public void Execute(int index) => generator.Execute(index, streams);

        public static JobHandle ScheduleParallel(Mesh mesh, Mesh.MeshData meshData, int resolution, JobHandle dependency)
        {
            var job = new MeshJob<Generator, Streams>();
            job.generator.Resolution = resolution;
            job.streams.Setup(meshData, mesh.bounds = job.generator.Bounds, job.generator.VertexCount, job.generator.IndexTriangleCount);
            return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
        }
    }
}
