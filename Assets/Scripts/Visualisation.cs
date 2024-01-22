using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public abstract class Visualisation : MonoBehaviour
{
    static readonly private int configId = Shader.PropertyToID("_Config"),
                                    positionsId = Shader.PropertyToID("_Positions"),
                                    normalsId = Shader.PropertyToID("_Normals");

    private ComputeBuffer positionsBuffer;
    private ComputeBuffer normalsBuffer;
    private NativeArray<float3> positions;
    private NativeArray<float3> normals;

    private enum Shape
    {
        Torus,
        UVSphere,
        Sphere,
        Plane,
        Cone
    }

    private static readonly Shapes.ScheduleDelegate[] shapeJobs = {
        Shapes.Job<Shapes.Torus>.ScheduleParallel,
        Shapes.Job<Shapes.UVSphere>.ScheduleParallel,
        Shapes.Job<Shapes.Sphere>.ScheduleParallel,
        Shapes.Job<Shapes.Plane>.ScheduleParallel,
        Shapes.Job<Shapes.Cone>.ScheduleParallel
    };

    [SerializeField]
    private Shape shape;

    [SerializeField]
    protected Material material;
    [SerializeField]
    private Mesh mesh;

    [SerializeField, Range(1, 400)]
    private int resolution;
    [SerializeField, Range(-0.5f, 0.5f)]
    private float displacement;
    [SerializeField, Range(0.5f, 10f)]
    private float sizeMultiplier = 1f;
    private int ValidRes => resolution * 2;

    protected bool isDirty = false;

    void OnEnable()
    {
        isDirty = true;

        positionsBuffer = new ComputeBuffer(ValidRes * ValidRes, sizeof(float) * 3);
        normalsBuffer = new ComputeBuffer(ValidRes * ValidRes, sizeof(float) * 3);
        positions = new NativeArray<float3>(ValidRes * ValidRes, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        normals = new NativeArray<float3>(ValidRes * ValidRes, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetBuffer(normalsId, normalsBuffer);
        material.SetVector(configId, new Vector4(ValidRes, 1f / ValidRes, displacement, sizeMultiplier));

        EnableVisualization(ValidRes);
    }

    void OnDisable()
    {
        positions.Dispose();
        normals.Dispose();

        positionsBuffer?.Release();
        normalsBuffer?.Release();

        positionsBuffer = null;
        normalsBuffer = null;

        DisableVisualization();
    }

    void OnValidate()
    {
        if (positionsBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    void Update()
    {
        if (isDirty || transform.hasChanged)
        {
            isDirty = false;
            transform.hasChanged = false;

            var handle = shapeJobs[(int)shape](positions.Reinterpret<float3x4>(3 * 4),
                                                normals.Reinterpret<float3x4>(3 * 4), 
                                                ValidRes, 
                                                transform.localToWorldMatrix,
                                                default);
            
            UpdateVisualization(positions.Reinterpret<float3x4>(3 * 4), ValidRes, handle);

            positionsBuffer.SetData(positions);
            normalsBuffer.SetData(normals);
        }

        RenderParams rp = new(material)
        {
            worldBounds = new Bounds(transform.position, transform.lossyScale + Vector3.one * displacement / ValidRes)
        };
        Graphics.RenderMeshPrimitives(rp, mesh, 0, ValidRes * ValidRes);
    }

    protected abstract void UpdateVisualization( NativeArray<float3x4> positions, int resolution, JobHandle handle);
    protected abstract void EnableVisualization( int resolution);
    protected abstract void DisableVisualization();
}
