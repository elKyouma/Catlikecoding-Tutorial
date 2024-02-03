using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
public static partial class Noises
{
    public interface IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w);

        public float4 EvaluateAfterInterpolation(float4 value);
    }

    public struct Value : IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => hash.FloatsA() * 2 - 1;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => hash.FloatsA() * 2 - 1;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => hash.FloatsA() * 2 - 1;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w) => hash.FloatsA() * 2 - 1;

        public float4 EvaluateAfterInterpolation(float4 value) => value;
    }

    public struct Kernel<GRADIENT> : IGradient where GRADIENT : struct, IGradient
    {
        public static float4 Interpolation(float4 x, float4 y, float4 z, float4 w)
        {
            x = abs(x);
            y = abs(y);
            z = abs(z);

            return max(8 * pow(0.5f - x * x - y * y - z * z - w * w, 3), 0f);
        }

        public float4 Evaluate(SmallXXHash4 hash, float4 x) => default(GRADIENT).Evaluate(hash, x) * Interpolation(x, 0, 0, 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => default(GRADIENT).Evaluate(hash, x, y) * Interpolation(x, y, 0, 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => default(GRADIENT).Evaluate(hash, x, y, 0) * Interpolation(x, y, z, 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w) => default(GRADIENT).Evaluate(hash, x, y, z, w) * Interpolation(x, y, z, w);

        public float4 EvaluateAfterInterpolation(float4 value) => default(GRADIENT).EvaluateAfterInterpolation(value);
    }




    public struct Perlin : IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => BaseGradients.Line(hash, x) / 2;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => BaseGradients.Square(hash, x, y) * (2f / 0.53528f);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => BaseGradients.Octahedron(hash, x, y, z) * (1f / 0.56290f);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w) => BaseGradients.Orthoplex(hash, x, y, z, w) * (1f / 0.56290f);

        public float4 EvaluateAfterInterpolation(float4 value) => value;
    }
    public struct Turbulance<GRADIENT> : IGradient where GRADIENT : struct, IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => default(GRADIENT).Evaluate(hash, x);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => default(GRADIENT).Evaluate(hash, x, y);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => default(GRADIENT).Evaluate(hash, x, y, z);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w) => default(GRADIENT).Evaluate(hash, x, y, z, w);

        public float4 EvaluateAfterInterpolation(float4 value) => abs(default(GRADIENT).EvaluateAfterInterpolation(value));
    }

    public struct Simplex : IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => BaseGradients.Line(hash, x) *  32f / 27f;
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => BaseGradients.Circle(hash, x, y) * (5.832f / sqrt(2f));
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => BaseGradients.Sphere(hash, x, y, z) * (1024f / (125f * sqrt(3f)));
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w) => BaseGradients.HyperSphere(hash, x, y, z, w) * (1f / 0.56290f);

        public float4 EvaluateAfterInterpolation(float4 value) => value;
    }

    public static class BaseGradients
    {
        public static float4 Line(SmallXXHash4 hash, float4 x) => (hash.FloatsA() * 2 - 1f) * x;
        public static float4x2 SquareVectrors(SmallXXHash4 hash)
        {
            float4x2 g;
            g.c0 = hash.FloatsA() * 2f - 1f;
            g.c1 = 0.5f - abs(g.c0);
            g.c0 -= floor(g.c0 + 0.5f);
            return g;
        }

        public static float4x3 OctahedronVectors(SmallXXHash4 hash)
        {
            float4x3 g;
            g.c0 = hash.FloatsA() * 2f - 1f;
            g.c1 = hash.FloatsD() * 2f - 1f;
            g.c2 = 1f - abs(g.c0) - abs(g.c1);
            float4 offset = max(-g.c2, 0f);
            g.c0 += select(-offset, offset, g.c0 < 0f);
            g.c1 += select(-offset, offset, g.c1 < 0f);
            return g;
        }

        public static float4x4 OrthoplexVectors(SmallXXHash4 hash)
        {
            float4x4 g;
            g.c0 = hash.FloatsA() * 2f - 1f;
            g.c1 = hash.FloatsD() * 2f - 1f;
            g.c2 = hash.FloatsC() * 2f - 1f;
            g.c3 = 1f - abs(g.c0) - abs(g.c1) - abs(g.c2);
            float4 offset = max(-g.c3, 0f);
            g.c0 += select(-offset, offset, g.c0 < 0f);
            g.c1 += select(-offset, offset, g.c1 < 0f);
            g.c2 += select(-offset, offset, g.c2 < 0f);
            return g;
        }


        public static float4 Square(SmallXXHash4 hash, float4 x, float4 y)
        {
            float4x2 g = SquareVectrors(hash);
            return (g.c0 * x + g.c1 * y);
        }

        public static float4 Circle(SmallXXHash4 hash, float4 x, float4 y)
        {
            float4x2 g = SquareVectrors(hash);
            return (g.c0 * x + g.c1 * y) * rsqrt(g.c0 * g.c0 + g.c1 * g.c1);
        }

        public static float4 Octahedron(SmallXXHash4 hash, float4 x, float4 y, float4 z)
        {
            float4x3 g = OctahedronVectors(hash);
            return (g.c0 * x + g.c1 * y + g.c2 * z);
        }

        public static float4 Sphere(SmallXXHash4 hash, float4 x, float4 y, float4 z)
        {
            float4x3 g = OctahedronVectors(hash);
            return (g.c0 * x + g.c1 * y + g.c2 * z) * rsqrt(g.c0 * g.c0 + g.c1 * g.c1 + g.c2 * g.c2);
        }

        public static float4 Orthoplex(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 t)
        {
            float4x4 g = OrthoplexVectors(hash);
            return (g.c0 * x + g.c1 * y + g.c2 * z + g.c3 * t);
        }

        public static float4 HyperSphere(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 t)
        {
            float4x4 g = OrthoplexVectors(hash);
            return (g.c0 * x + g.c1 * y + g.c2 * z + g.c3 * t) * rsqrt(g.c0 * g.c0 + g.c1 * g.c1 + g.c2 * g.c2 + g.c3 * g.c3);
        }
    }
}
