using ProceduralMeshes.Generators;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
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
        [SerializeField, Range(1, 30)]
        int resolution;
        [SerializeField]
        bool drawGizmos = true;
        enum MeshJobType
        {
            SquareGrid,
            SharedSquareGrid,
            SharedTriangleGrid,
            PointyHexagonGrid,
            FlatHexagonGrid
        }

        Vector3[] vertices;

        [SerializeField]
        MeshJobType meshJobType;

        MeshJobScheduleDelegate[] meshJobs =
        {
            MeshJob<SquareGrid, Streams.SingleStream>.ScheduleParallel,
            MeshJob<SquareGrid, Streams.MultiStream>.ScheduleParallel,
            MeshJob<SharedSquareGrid, Streams.SingleStream>.ScheduleParallel,
            MeshJob<SharedSquareGrid, Streams.MultiStream>.ScheduleParallel,
            MeshJob<SharedTriangleGrid, Streams.SingleStream>.ScheduleParallel,
            MeshJob<SharedTriangleGrid, Streams.MultiStream>.ScheduleParallel,
            MeshJob<PointyHexagonGrid, Streams.SingleStream>.ScheduleParallel,
            MeshJob<PointyHexagonGrid, Streams.MultiStream>.ScheduleParallel,
            MeshJob<FlatHexagonGrid, Streams.SingleStream>.ScheduleParallel,
            MeshJob<FlatHexagonGrid, Streams.MultiStream>.ScheduleParallel
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

            meshJobs[(int)meshJobType*2 + (int)streamType](mesh, meshData, resolution, default).Complete();

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh, MeshUpdateFlags.DontRecalculateBounds);
            GetComponent<MeshFilter>().mesh = mesh;
            
            vertices = mesh.vertices;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || vertices == null) return;

            foreach(Vector3 v in vertices)
            {
                Gizmos.DrawSphere(v * transform.lossyScale.x, 0.1f / resolution);
            }
        }
    }
}
