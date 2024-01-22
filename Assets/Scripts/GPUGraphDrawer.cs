using UnityEngine;
using UnityEngine.Rendering;

public class GPUGraphDrawer : MonoBehaviour
{

    [SerializeField]
    Material material;
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    ComputeShader mathLibrary;

    const int maxResolution = 200;

    [SerializeField, Range(0, 20)]
    float speed = 1; 
    [SerializeField, Range(0, 6)]
    float figureSize = 1;
    [SerializeField, Range(0, 20)]
    int functionSize = 1;
    [SerializeField, Range(0, 1)]
    float amp = 0.5f;
    [SerializeField, Range(0, 10)]
    float dump = 1;

    [SerializeField, Range(1, maxResolution)]
    int resolution = 10;
    [SerializeField, Range(0, 5)]
    int functionIndex = 1;


    enum Figure2D
    {
        LINE = 6,
        CIRCLE
    }

    enum Figure3D
    {
        PLANE = 8,
        SPHERE,
        TORUS
    }

    [SerializeField]
    Figure2D figure2d;

    [SerializeField]
    Figure3D figure3d;

    const int index3D = 3;

    int prevFunctionIndex;

    private ComputeBuffer posBuffer;
    private ComputeBuffer normalBuffer;
    static readonly int
            positionsId = Shader.PropertyToID("_Positions"),
            normalsId = Shader.PropertyToID("_Normals"),
            resolutionId = Shader.PropertyToID("_Resolution"),
            stepId = Shader.PropertyToID("_Step"),
            figureSizeId = Shader.PropertyToID("_FigureSize"),
            figureSize2Id = Shader.PropertyToID("_FigureSize2"),
            functionSizeId = Shader.PropertyToID("_FunctionSize"),
            speedId = Shader.PropertyToID("_Speed"),
            dumpId = Shader.PropertyToID("_Dump"),
            amp0Id = Shader.PropertyToID("_Amp0"),
            amp1Id = Shader.PropertyToID("_Amp1"),
            amp2Id = Shader.PropertyToID("_Amp2"),
            timeId = Shader.PropertyToID("_Time");

    // Start is called before the first frame update
    private void OnEnable()
    {
        posBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * sizeof(float));
        normalBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * sizeof(float));
        prevFunctionIndex = functionIndex;
        SetCamera();

        figure2d = Figure2D.LINE;
        figure3d = Figure3D.PLANE;
    }

    void OnDisable()
    {
        posBuffer.Release();
        posBuffer = null;

        normalBuffer.Release();
        normalBuffer = null;
    }

    private void SetCamera()
    {
        if (!Application.isPlaying) return;

        if (functionIndex >= index3D)
            CameraManager.Instance?.TurnOn2DView();
        else
            CameraManager.Instance?.TurnOn3DView();
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2 * Mathf.PI / resolution;
        
        mathLibrary.SetInt(resolutionId, resolution);
        mathLibrary.SetFloat(stepId, step);
        mathLibrary.SetFloat(figureSizeId, figureSize / 2 / Mathf.PI);
        mathLibrary.SetFloat(figureSize2Id, figureSize * 3 / Mathf.PI);
        mathLibrary.SetInt(functionSizeId, functionSize);
        mathLibrary.SetFloat(dumpId, dump);
        mathLibrary.SetFloat(timeId, Time.time);
        
        mathLibrary.SetFloat(amp0Id, amp);
        mathLibrary.SetFloat(amp1Id, amp * 4.6f);
        mathLibrary.SetFloat(amp2Id, amp * 8.2f);

        mathLibrary.SetFloat(speedId, speed);
        mathLibrary.SetBuffer(functionIndex, positionsId, posBuffer);
        mathLibrary.SetBuffer(functionIndex, normalsId, normalBuffer);

        int figureindex = functionIndex > index3D ? (int)figure2d : (int)figure3d;

        mathLibrary.SetBuffer(figureindex, positionsId, posBuffer);
        mathLibrary.SetBuffer(figureindex, normalsId, normalBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        mathLibrary.Dispatch(figureindex, groups, groups, 1);
        mathLibrary.Dispatch(functionIndex, groups, groups, 1);

        material.SetFloat(stepId, figureSize * 0.9f/resolution);
        material.SetBuffer(positionsId, posBuffer);


        var bounds = new Bounds(Vector3.zero, Vector3.one * (5f + 2f / resolution));
        RenderParams renderParam = new(material)
        {
            worldBounds = bounds,
        };

        Graphics.RenderMeshPrimitives(renderParam, mesh, 0, resolution * resolution);
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateFunctionOnGPU();

        if(prevFunctionIndex != functionIndex)
        {
            prevFunctionIndex = functionIndex;
            SetCamera();
        }
    }
}
