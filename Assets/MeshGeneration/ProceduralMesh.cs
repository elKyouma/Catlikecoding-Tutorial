using ProceduralMeshes.Generators;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace ProceduralMeshes
{
    public class ProceduralMesh : MonoBehaviour
    {
        enum StreamType
        {
            SingleStream,
            MultiStream
        }

        [SerializeField]
        StreamType streamType;
        [SerializeField, Range(1, 10)]
        int resolution;

        enum MeshJobType
        {
            SquareGrid
        }

        [SerializeField]
        MeshJobType meshJobType;

        MeshJobScheduleDelegate[] meshJobs =
        {
             MeshJob<SquareGrid, Streams.SingleStream>.ScheduleParallel,
             MeshJob<SquareGrid, Streams.MultiStream>.ScheduleParallel
        };
        bool dirty;
        private void Awake()
        {
            dirty = true;
        }

        private void OnValidate()
        {
            dirty = true;
        }

        private void Update()
        {
            if (dirty)
                RenderMesh();
        }

        void RenderMesh()
        {
            dirty = false;
            Mesh mesh = new()
            {
                name = "ProceduralMesh"
            };

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            meshJobs[(int)meshJobType + (int)streamType](mesh, meshData, resolution, default).Complete();

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh, MeshUpdateFlags.DontRecalculateBounds);
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}