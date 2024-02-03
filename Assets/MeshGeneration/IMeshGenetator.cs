using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMeshes
{
    public interface IMeshGenetator
    {
        public int JobLength => 0;
        public int VertexCount => 0;
        public int IndexTriangleCount => 0;
        Bounds Bounds { get; }
        public void Execute<Streams>(int index, Streams streams) where Streams : struct, IMeshStreams{ }
    }
}
