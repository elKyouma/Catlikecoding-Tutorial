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
        [SerializeField, Range(0.2f, 12f)]
        private float gizmoSize = 1f;
        enum MeshJobType
        {
            SquareGrid,
            SharedSquareGrid,
            SharedTriangleGrid,
            PointyHexagonGrid,
            FlatHexagonGrid,
            UVSphere,
            Cylinder
        }

        Vector3[] vertices;
        Vector3[] normals;
        Vector4[] tangents;

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
            MeshJob<FlatHexagonGrid, Streams.MultiStream>.ScheduleParallel,            
            MeshJob<UVSphere, Streams.SingleStream>.ScheduleParallel,
            MeshJob<UVSphere, Streams.MultiStream>.ScheduleParallel,
            MeshJob<Cylinder, Streams.SingleStream>.ScheduleParallel,
            MeshJob<Cylinder, Streams.MultiStream>.ScheduleParallel
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
            normals = mesh.normals;
            tangents = mesh.tangents;
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || vertices == null) return;

            Transform t = transform;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = t.TransformPoint(vertices[i]);
                Vector3 n = t.TransformDirection(normals[i]);
                Vector4 tan = t.TransformDirection(tangents[i]);

                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(v, gizmoSize * 0.1f / resolution);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(v, gizmoSize * 0.5f * n / resolution);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(v, gizmoSize * 0.5f * tan / resolution);
            }
        }
    }
}
