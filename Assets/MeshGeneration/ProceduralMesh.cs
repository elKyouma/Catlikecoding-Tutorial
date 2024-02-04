using ProceduralMeshes.Generators;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

        private void Awake()
        {
            Mesh mesh = new()
            {
                name = "ProceduralMesh"
            };

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            if(streamType == StreamType.SingleStream)
                MeshJob<SquareGrid, Streams.SingleStream>.ScheduleParallel(mesh, meshData, default).Complete();
            else
                MeshJob<SquareGrid, Streams.MultiStream>.ScheduleParallel(mesh, meshData, default).Complete();

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh, MeshUpdateFlags.DontRecalculateBounds);
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
