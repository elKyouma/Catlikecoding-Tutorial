using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;

using float3x4 = Unity.Mathematics.float3x4;
using Unity.Collections;
using System;

using static MathExtension;
using static Noises;
using static Noise;

public class NoiseMaker : Visualisation
{

    [SerializeField]
    private SpaceTRS domain;
    [SerializeField]
    private Noises.NoiseSettings settings;

    private ComputeBuffer noiseBuffer;
    private NativeArray<float> noise;
    static readonly int noiseId = Shader.PropertyToID("_Noise");

    [SerializeField, Range(1, 3)]
    private int dimensions = 1;

    [SerializeField, Range(0, 4)]
    private float timeScale;

    [SerializeField]
    private bool tiled = false;
    private enum NoiseType
    {
        VALUE,
        PERLIN,
        TURBULANCE_VAULE,
        TURBULANCE_PERLIN,
        SIMPLEX_VALUE, 
        TURBULANCE_SIMPLEX,
        VORONOI_WORLEYF1,
        VORONOI_WORLEYF2,
        VORONOI_WORLEYF2MINUSF1,
        VORONOI_WORLEYF2PLUSF1,
        VORONOI_CHEBYSHEVF1,
        VORONOI_CHEBYSHEVF2,
        VORONOI_CHEBYSHEVF2MINUSF1,
        VORONOI_CHEBYSHEVF2PLUSF1
    }

    [SerializeField]
    private NoiseType noiseType;

    private static readonly ScheduleDelegate[,] noiseJobs = {
        {
            Job<Lattice1DT<Value, LatticeNormal>>.ScheduleParallel,
            Job<Lattice1DT<Value, LatticeTiled>>.ScheduleParallel,
            Job<Lattice2DT<Value, LatticeNormal>>.ScheduleParallel,
            Job<Lattice2DT<Value, LatticeTiled>>.ScheduleParallel,
            Job<Lattice3DT<Value, LatticeNormal>>.ScheduleParallel,
            Job<Lattice3DT<Value, LatticeTiled>>.ScheduleParallel
        },
        {
            Job<Lattice1DT<Perlin, LatticeNormal>>.ScheduleParallel,
            Job<Lattice1DT<Perlin, LatticeTiled>>.ScheduleParallel,
            Job<Lattice2DT<Perlin, LatticeNormal>>.ScheduleParallel,
            Job<Lattice2DT<Perlin, LatticeTiled>>.ScheduleParallel,
            Job<Lattice3DT<Perlin, LatticeNormal>>.ScheduleParallel,
            Job<Lattice3DT<Perlin, LatticeTiled>>.ScheduleParallel
        },
        {
            Job<Lattice1DT<Turbulance<Value>, LatticeNormal>>.ScheduleParallel,
            Job<Lattice1DT<Turbulance<Value>, LatticeTiled>>.ScheduleParallel,
            Job<Lattice2DT<Turbulance<Value>, LatticeNormal>>.ScheduleParallel,
            Job<Lattice2DT<Turbulance<Value>, LatticeTiled>>.ScheduleParallel,
            Job<Lattice3DT<Turbulance<Value>, LatticeNormal>>.ScheduleParallel,
            Job<Lattice3DT<Turbulance<Value>, LatticeTiled>>.ScheduleParallel
        },
        {
            Job<Lattice1DT<Turbulance<Perlin>, LatticeNormal>>.ScheduleParallel,
            Job<Lattice1DT<Turbulance<Perlin>, LatticeTiled>>.ScheduleParallel,
            Job<Lattice2DT<Turbulance<Perlin>, LatticeNormal>>.ScheduleParallel,
            Job<Lattice2DT<Turbulance<Perlin>, LatticeTiled>>.ScheduleParallel,
            Job<Lattice3DT<Turbulance<Perlin>, LatticeNormal>>.ScheduleParallel,
            Job<Lattice3DT<Turbulance<Perlin>, LatticeTiled>>.ScheduleParallel
        },
        {
            Job<Simplex1D<Kernel>>.ScheduleParallel,
            Job<Simplex1D<Kernel>>.ScheduleParallel,
            Job<Simplex2D<Kernel>>.ScheduleParallel,
            Job<Simplex2D<Kernel>>.ScheduleParallel,
            Job<Simplex3D<Kernel>>.ScheduleParallel,
            Job<Simplex3D<Kernel>>.ScheduleParallel
        },
        {
            Job<Simplex1D<Turbulance<Kernel>>>.ScheduleParallel,
            Job<Simplex1D<Turbulance<Kernel>>>.ScheduleParallel,
            Job<Simplex2D<Turbulance<Kernel>>>.ScheduleParallel,
            Job<Simplex2D<Turbulance<Kernel>>>.ScheduleParallel,
            Job<Simplex3D<Turbulance<Kernel>>>.ScheduleParallel,
            Job<Simplex3D<Turbulance<Kernel>>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F1,         Worley>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F1,         Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F1,         Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F1,         Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F1,         Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F1,         Worley>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F2,         Worley>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F2,         Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F2,         Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F2,         Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F2,         Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F2,         Worley>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F2minusF1,   Worley>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F2minusF1,   Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F2minusF1,   Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F2minusF1,   Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F2minusF1,   Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F2minusF1,   Worley>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F2plusF1,    Worley>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F2plusF1,    Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F2plusF1,    Worley>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F2plusF1,    Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F2plusF1,    Worley>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F2plusF1,    Worley>>.ScheduleParallel
        },
                {
            Job<Voronoi1D<LatticeNormal,    F1,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F1,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F1,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F1,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F1,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F1,         Chebyshev>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F2,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F2,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F2,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F2,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F2,         Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F2,         Chebyshev>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F2minusF1,   Chebyshev>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F2minusF1,   Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F2minusF1,   Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F2minusF1,   Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F2minusF1,   Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F2minusF1,   Chebyshev>>.ScheduleParallel
        },
        {
            Job<Voronoi1D<LatticeNormal,    F2plusF1,    Chebyshev>>.ScheduleParallel,
            Job<Voronoi1D<LatticeTiled,     F2plusF1,    Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeNormal,    F2plusF1,    Chebyshev>>.ScheduleParallel,
            Job<Voronoi2D<LatticeTiled,     F2plusF1,    Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeNormal,    F2plusF1,    Chebyshev>>.ScheduleParallel,
            Job<Voronoi3D<LatticeTiled,     F2plusF1,    Chebyshev>>.ScheduleParallel
        }
    };


    protected override void EnableVisualization(int resolution)
    {
        noiseBuffer = new ComputeBuffer(resolution * resolution, sizeof(float));
        noise = new NativeArray<float>(resolution * resolution, Allocator.Persistent);
        material.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle)
    {
        handle = noiseJobs[(int)noiseType, (dimensions - 1) * 2 + (tiled ? 1 : 0)](positions, noise.Reinterpret<float4>(4), resolution, domain.Matrix, settings, timeScale, handle);
        handle.Complete();

        noiseBuffer.SetData(noise);
        if (timeScale > 0)
            isDirty = true;
    }

    protected override void DisableVisualization()
    {
        noise.Dispose();
        noiseBuffer?.Release();
        noiseBuffer = null;
    }
}
