using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

using static UnityEngine.Mathf;
public static class MathLibrary
{
    public delegate float Function2D(float u, float t);
    public delegate float Function3D(float u, float v, float t);
    public delegate float MutliParamFunction(float u, float t, params float[] param);

    public delegate Ray2D Figure2D(int elements, float t);
    public delegate Ray Figure3D(int xElements, int zElements, float t);

    public static float MapValue(float value, float min, float max, float width) => ((value - min) / (max - min)) * (width);
    public static float MapValue(float value, float min, float max, float newMin, float newMax) => MapValue(value, min, max, newMax - newMin) + newMin;
    public static float WaveFunction(float u, float t) => Sin(Speed * u + t % (2 * PI));

    public static float Ripple(float u, float t)
    {
        float d = Abs(u);
        float y = Sin(2f * Speed * PI * (d - t));

        return y / (1f + Dump * d);
    }

    public static float MultiWaveFunction(float u, float t)
    {
        if (Params == null)
            Debug.LogError("Lack of multipliers");

        float y = 0;
        float mults = 1;
        foreach (float multiplier in Params)
        {
            y += multiplier * Sin(u + Speed * t % (2 * PI));
            mults *= multiplier;
        }
        return y/mults;
    }

    public static float MultiWaveFunction(float u, float t, params float[] multipliers)
    {
        float y = 0;
        float mults = 1;

        foreach (float multiplier in multipliers)
        {
            y += multiplier * Sin(u + Speed * t % (2 * PI));
            mults *= multiplier;
        }

        return y/mults;
    }

    public static float WaveFunction(float u, float v, float t)
    {
        float y = Sin(Speed * (u + v) + t % (2 * PI));
        y += Sin(Speed * u + t % (2 * PI));
        y += Sin(Speed * v + t % (2 * PI));

        return y/3;
    }
    public static float Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        float y = Sin(2f * Speed * PI * (d - t));
        y /= (1f + Dump * d);

        return y;
    }

    public static float MultiWaveFunction(float u, float v, float t)
    {
        if (Params == null)
            Debug.LogError("Lack of multipliers");

        float y = 0;
        float mults = 1;

        foreach (float multiplier in Params)
        {
            y += multiplier * Sin(u + Speed * t % (2 * PI));
            y += multiplier * Sin(v + Speed * t % (2 * PI));
            y += multiplier * Sin((u + v) + Speed * t % (2 * PI));
            mults *= 3*multiplier;
        }

        return y/mults;
    }

    public static float MultiWaveFunction(float u, float v, float t, params float[] multipliers)
    {
        float y = 0;
        float mults = 1;
        foreach (float multiplier in multipliers)
        {
            y += multiplier * Sin(u + Speed * t % (2 * PI));
            y += multiplier * Sin(v + Speed * t % (2 * PI));
            y += multiplier * Sin((u + v) + Speed * t % (2 * PI));
            mults *= 3*multiplier;
        }

        return y/mults;
    }

    private static void Counter2D(int elements)
    {
        uCount++;
        if (uCount == elements)
            uCount = 0;
    }

    public static Ray2D Line(int elements, float t)
    {
        Counter2D(elements);
        return new Ray2D(new Vector2((float)uCount / elements - 1.0f / 2, 0) * Size, Vector2.up);
    }

    public static Ray Plane(int xElements, int zElements, float t)
    {
        Counter3D(xElements, zElements);
        return new Ray(new Vector3((float)uCount / xElements - 1.0f / 2, 0, (float)vCount / zElements - 1.0f / 2) * Size, Vector3.up);
    }
    public static Ray2D Circle(int elements, float t)
    {
        Counter2D(elements);

        Vector2 r = new Vector2(Cos(uCount * 2 * PI / elements), Sin(uCount * 2 * PI / elements)) * Size;
        return new Ray2D(r, r.normalized);
    }

    private static void Counter3D(int xElements, int zElements)
    {
        uCount++;
        if (uCount == xElements)
        {
            uCount = 0;
            vCount++;
            if (vCount == zElements)
                vCount = 0;
        }
    }
    public static Ray Circle3D(int xElements, int zElements, float t)
    {
        Counter3D(xElements, zElements);
        Vector3 r = new Vector3(Cos(uCount * 2 * PI / xElements), vCount, Sin(uCount * 2 * PI / xElements)) * Size;
        return new Ray(r, r.normalized);
    }

    public static Ray Sphere(int xElements, int zElements, float t)
    {
        Counter3D(xElements, zElements);

        float u = uCount * 2 * PI / xElements;
        float v = vCount * 2 * PI / zElements;

        float r = Sin(v);
        float x = r * Cos(u);
        float y = Cos(v);
        float z = r * Sin(u);

        Vector3 R = new Vector3(x, y, z) * Size;
        return new Ray(R, R.normalized);
    }

    public static Ray Torus(int xElements, int zElements, float t)
    {
        Counter3D(xElements, zElements);

        float u = uCount * 2 * PI / xElements;
        float v = vCount * 2 * PI / zElements;

        if (Params.Count < 2)
            Debug.Log("Lack of params r1 and r2 for torus");

        float r1 = Params[0];
        float r2 = Params[1];

        float r = r1 + r2 * Sin(v);
        float x = r * Cos(u);
        float y = Cos(v);
        float z = r * Sin(u);

        Vector3 R = new Vector3(x, y, z) * Size;
        Vector3 N = (new Vector3(x, y, z) - new Vector3(x, y, 0) * (r1 + r2/2)).normalized;

        return new Ray(R, N);
    }

    private static int uCount = 0;
    private static int vCount = 0;
    public static float Size { get; set; }
    public static float Dump { get; set; }
    public static float Speed { get; set; }
    public static List<float> Params { get; set; }
}
