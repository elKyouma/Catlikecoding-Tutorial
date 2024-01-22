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

    public struct Kernel : IGradient
    {
        public static float4 Interpolation(float4 x, float4 y, float4 z, float4 w)
        {
            x = abs(x);
            y = abs(y);
            z = abs(z);

            return max(8* pow(0.5f - x * x - y * y - z * z - w * w, 3), 0f);
        }

        public float4 Evaluate(SmallXXHash4 hash, float4 x) => (hash.FloatsA() * 2 - 1) * Interpolation(x, 0, 0, 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) => (hash.FloatsA() * 2 - 1) * Interpolation(x, y, 0, 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) => (hash.FloatsA() * 2 - 1) * Interpolation(x, y, z, 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w) => (hash.FloatsA() * 2 - 1) * Interpolation(x, y, z, w);

        public float4 EvaluateAfterInterpolation(float4 value) => value;
    }




    public struct Perlin : IGradient
    {
        public float4 Evaluate(SmallXXHash4 hash, float4 x) => (1f + hash.FloatsA()) * select(-x, x, (((uint4)hash >> 8) & 1) == 0);
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y)
        {
            float4 gx = hash.FloatsA() * 2f - 1f;
            float4 gy = 0.5f - abs(gx);
            gx -= floor(gx + 0.5f);
            return (gx * x + gy * y) * (2f / 0.53528f);
        }
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z)
        {
            float4 gx = hash.FloatsA() * 2f - 1f, gy = hash.FloatsD() * 2f - 1f;
            float4 gz = 1f - abs(gx) - abs(gy);
            float4 offset = max(-gz, 0f);
            gx += select(-offset, offset, gx < 0f);
            gy += select(-offset, offset, gy < 0f);
            return (gx * x + gy * y + gz * z) * (1f / 0.56290f);
        }
        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z, float4 w)
        {
            float4 gx = hash.FloatsA() * 2f - 1f, gy = hash.FloatsD() * 2f - 1f, gz = hash.FloatsC() * 2f - 1f;
            float4 gw = 1f - abs(gx) - abs(gy) - abs(gz);
            float4 offset = max(-gw, 0f);
            gx += select(-offset, offset, gx < 0f);
            gy += select(-offset, offset, gy < 0f);
            gz += select(-offset, offset, gz < 0f);
            return (gx * x + gy * y + gz * z + gw * w) * (1f / 0.56290f);
        }

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
}
