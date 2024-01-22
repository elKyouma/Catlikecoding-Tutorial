using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GraphDrawer : MonoBehaviour
{
    public enum Type
    {
        _2D,
        _3D
    }

    enum Functions2D
    {
        WAVE,
        RIPPLE,
        MULTI_WAVE_FUNCTION
    }

    enum Functions3D
    {
        WAVE,
        RIPPLE,
        MULTI_WAVE_FUNCTION
    }

    enum FigureType2D
    {
        CIRCLE,
        LINE
    }

    enum FigureType3D
    {
        CIRCLE,
        SPHERE,
        TORUS,
        PLANE
    }

    public Type CurrentView { get; private set; }

    public delegate void CurrentViewChangedCallback(Type newType);

    public event CurrentViewChangedCallback OnCurrentViewChangedCallback;

    [SerializeField, HideInInspector]
    Functions2D functionType2d;
    [SerializeField, HideInInspector]
    Functions3D functionType3d;
    [SerializeField, HideInInspector]
    FigureType2D figureType2d;
    [SerializeField, HideInInspector]
    FigureType3D figureType3d;

    MathLibrary.Function2D currentFunction2D;
    MathLibrary.Function3D currentFunction3D;
    MathLibrary.Figure2D currentFigure2D;
    MathLibrary.Figure3D currentFigure3D;

    [SerializeField]
    private Vector3 offset = Vector3.zero;
    [SerializeField]
    private float inputScale = 1;
    [SerializeField]
    private float outputScale = 1;
    [SerializeField]
    private float figureScale = 1;

    [SerializeField]
    private float dump = 10;
    [SerializeField]
    private uint density = 20;

    [SerializeField]
    private float transitionDuration = 2.0f;

    private List<GameObject> drawingObjects;

    private float duration = 0.0f;

    private void OnEnable()
    {
        OnCurrentViewChangedCallback += SetCamera;
        OnCurrentViewChangedCallback += SetDrawableType;
    }

    private void OnDisable()
    {
        OnCurrentViewChangedCallback -= SetCamera;
        OnCurrentViewChangedCallback -= SetDrawableType;
    }

    public void Interpolate() => duration = 0;

    void Start()
    {
        SetCamera(CurrentView);
        BuildGraph();
    }
    private void CreateGraphPoints()
    {
        GameObject point = Resources.Load<GameObject>("Cube");


        if (CurrentView == Type._2D)
        {
            int amount = (int)density;
            for (int i = 0; i < amount; i++)
                drawingObjects.Add(Instantiate(point, transform));
        }
        else
        {
            int amount = (int)(density * density);
            for (int i = 0; i < amount; i++)
                drawingObjects.Add(Instantiate(point, transform));
        }
    }

    private void SetDrawableType(Type newView)
    {
        switch (newView)
        {
            case Type._2D:
                SetFunctionType2D();
                SetFigureType2D();
                break;
            case Type._3D:
                SetFunctionType3D();
                SetFigureType3D();
                break;
        }
    }

    private void SetFunctionType2D()
    {
        MathLibrary.Size = 12f;
        MathLibrary.Dump = dump;
        switch (functionType2d)
        {
            case Functions2D.WAVE:
                MathLibrary.Speed = 4;
                currentFunction2D = MathLibrary.WaveFunction;
                break;
            case Functions2D.RIPPLE:
                MathLibrary.Speed = 1f;
                currentFunction2D = MathLibrary.Ripple;
                break;
            case Functions2D.MULTI_WAVE_FUNCTION:
                MathLibrary.Params = new List<float> { 2, 5, 7 };
                MathLibrary.Speed = 20f;
                currentFunction2D = MathLibrary.MultiWaveFunction;
                break;
        }
    }

    private void SetFunctionType3D()
    {
        MathLibrary.Size = 2;
        MathLibrary.Dump = dump;
        switch (functionType3d)
        {
            case Functions3D.WAVE:
                MathLibrary.Speed = 4f;
                currentFunction3D = MathLibrary.WaveFunction;
                break;
            case Functions3D.RIPPLE:
                MathLibrary.Speed = 1f;
                currentFunction3D = MathLibrary.Ripple;
                break;
            case Functions3D.MULTI_WAVE_FUNCTION:
                MathLibrary.Params = new List<float> { 1, 0.7f, 3 };
                MathLibrary.Speed = 4f;
                currentFunction3D = MathLibrary.MultiWaveFunction;
                break;
        }
    }

    private void SetFigureType2D()
    {
        switch (figureType2d)
        {
            case FigureType2D.CIRCLE:
                currentFigure2D = MathLibrary.Circle;
                break;
            case FigureType2D.LINE:
                currentFigure2D = MathLibrary.Line;
                break;
        }
    }

    private void SetFigureType3D()
    {
        switch (figureType3d)
        {
            case FigureType3D.CIRCLE:
                currentFigure3D = MathLibrary.Circle3D;
                break;
            case FigureType3D.SPHERE:
                currentFigure3D = MathLibrary.Sphere;
                break;
            case FigureType3D.TORUS:
                MathLibrary.Params = new List<float> { 4f, 1f };
                currentFigure3D = MathLibrary.Torus;
                break;
            case FigureType3D.PLANE:
                currentFigure3D = MathLibrary.Plane;
                break;
        }

    }

    public void ChangeView(Type newView)
    {
        if (CurrentView == newView) return;

        OnCurrentViewChangedCallback?.Invoke(newView);
        duration = 0;
        CurrentView = newView;
        SetDrawableType(CurrentView);
    }

    private void OnValidate()
    {
        SetDrawableType(CurrentView);
    }

    private void SetCamera(Type newView)
    {
        if (!Application.isPlaying) return;

        switch (newView)
        {
            case Type._2D:
                CameraManager.Instance?.TurnOn2DView();
                break;
            case Type._3D:
                CameraManager.Instance?.TurnOn3DView();
                break;
        }
    }

    public void BuildGraph()
    {
        SetDrawableType(CurrentView);
        ResetGraph();
        UpdateDrawable(0);
    }

    private void Update()
    {
        duration += Time.deltaTime;

        MathLibrary.Size = figureScale;
        UpdateDrawable(Time.time);
    }

    private void UpdateDrawable(float time)
    {
        switch (CurrentView)
        {
            case Type._2D: UpdateGraph2D(time); break;
            case Type._3D: UpdateGraph3D(time); break;
        }
    }

    private void UpdateGraph2D(float time)
    {
        int amount = drawingObjects.Count;

        for (int i = 0; i < amount; i++)
        {
            float x = (((float)i / amount - 0.5f) * inputScale - offset.x) * 2 * Mathf.PI;
            var ray = currentFigure2D(amount, time);
            Vector2 p = ray.origin * figureScale + ray.direction * currentFunction2D(x, time) * outputScale;

            if (duration < transitionDuration)
                drawingObjects[i].transform.localPosition = Vector3.Lerp(drawingObjects[i].transform.localPosition, p, Mathf.SmoothStep(0, 1, duration / transitionDuration));
            else
                drawingObjects[i].transform.localPosition = p;
        }
    }

    private void UpdateGraph3D(float time)
    {
        int amount = (int)Mathf.Sqrt(drawingObjects.Count);

        for (int iz = 0; iz < amount; iz++)
        {
            for (int ix = 0; ix < amount; ix++)
            {
                float x = (((float)ix / amount - 0.5f) * inputScale - offset.x) * 2 * Mathf.PI;
                float z = (((float)iz / amount - 0.5f) * inputScale - offset.z) * 2 * Mathf.PI;
                var ray = currentFigure3D(amount, amount, time);

                Vector3 p = ray.origin * figureScale + ray.direction * currentFunction3D(x, z, time) * outputScale;

                if (duration < transitionDuration)
                    drawingObjects[ix + iz * amount].transform.localPosition = Vector3.Lerp(drawingObjects[ix + iz * amount].transform.localPosition,
                                                                                                p,
                                                                                                Mathf.SmoothStep(0, 1, duration / transitionDuration));
                else
                    drawingObjects[ix + iz * amount].transform.localPosition = p;

            }
        }

    }

    public void ResetGraph()
    {
        drawingObjects ??= new List<GameObject>();

        drawingObjects.Clear();
        if (CurrentView == Type._2D)
            drawingObjects.Capacity = (int)density;
        else
            drawingObjects.Capacity = (int)(density * density);

        var children = GetComponentsInChildren<Transform>();
        int size = children.Length;
        for (int i = size - 1; i >= 1; i--)
        {
            if (Application.isEditor)
                DestroyImmediate(children[i].gameObject);
            else
                Destroy(children[i].gameObject);
        }

        CreateGraphPoints();
    }

    private void OnDrawGizmos()
    {
        if (CurrentView == Type._2D)
            Gizmos.DrawWireCube(transform.position, new Vector3(figureScale, figureScale, 0.01f));
        else
            Gizmos.DrawWireCube(transform.position, new Vector3(figureScale, figureScale, figureScale));
    }
}
