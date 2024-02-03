using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Noises;

public static partial class Noise
{

    public struct Simplex1D<G> : INoise where G : struct, IGradient
    {
        static float4 Kernel(SmallXXHash4 hash, float4 lx, float4x4 pos)
        {
            float4 x = pos.c0 - lx;
            return default(G).Evaluate(hash, x);
        }

        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            positions *= frequency;
            int4 x0 = (int4)floor(positions.c0);
            int4 x1 = x0 + 1;

            return default(G).EvaluateAfterInterpolation(Kernel(hash.Eat(x0), x0, positions) + Kernel(hash.Eat(x1), x1, positions));
        }
    }

    public struct Simplex2D<G> : INoise where G : struct, IGradient
    {
        static float4 Kernel(SmallXXHash4 hash, float4 lx, float4 lz, float4x4 pos)
        {
            float4 unskew = (lx + lz) * ((3f - sqrt(3f)) / 6f);
            float4 x = pos.c0 - (lx - unskew);
            float4 z = pos.c2 - (lz - unskew);
            return default(G).Evaluate(hash, x, z);
        }

        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            positions *= frequency / sqrt(3);
            float4 skew = (positions.c0 + positions.c2) * ((sqrt(3f) - 1f) / 2f);
            float4 sx = positions.c0 + skew, sz = positions.c2 + skew;
            int4 x0 = (int4)floor(sx);
            int4 z0 = (int4)floor(sz);
            int4 x1 = x0 + 1;
            int4 z1 = z0 + 1;

            SmallXXHash4 hashx0 = hash.Eat(x0), hashx1 = hash.Eat(x1);

            return default(G).EvaluateAfterInterpolation(Kernel(hashx0.Eat(z0), x0, z0, positions) + Kernel(hashx1.Eat(z1), x1, z1, positions) +
                    select(Kernel(hashx0.Eat(z1), x0, z1, positions), Kernel(hashx1.Eat(z0), x1, z0, positions), sx - x0 > sz - z0)) / 3f;
        }
    }

    public struct Simplex3D<G> : INoise where G : struct, IGradient
    {
        static float4 Kernel(SmallXXHash4 hash, float4 lx, float4 ly, float4 lz, float4x4 pos)
        {
            float4 unskew = (lx + ly + lz) / 6f;
            float4 x = pos.c0 - (lx - unskew);
            float4 y = pos.c1 - (ly - unskew);
            float4 z = pos.c2 - (lz - unskew);
            return default(G).Evaluate(hash, x, y, z);
        }

        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            positions *= frequency * 0.6f;
            float4 skew = (positions.c0 + positions.c1 + positions.c2) / 3f;
            float4 sx = positions.c0 + skew, sy = positions.c1 + skew, sz = positions.c2 + skew;

            int4 x0 = (int4)floor(sx);
            int4 y0 = (int4)floor(sy);
            int4 z0 = (int4)floor(sz);
            int4 x1 = x0 + 1;
            int4 y1 = y0 + 1;
            int4 z1 = z0 + 1;


            bool4
                xGy = sx - x0 > sy - y0,
                xGz = sx - x0 > sz - z0,
                yGz = sy - y0 > sz - z0;


            bool4
                xA = xGy & xGz,
                xB = xGy | (xGz & yGz),
                yA = !xGy & yGz,
                yB = !xGy | (xGz & yGz),
                zA = (xGy & !xGz) | (!xGy & !yGz),
                zB = !(xGz & yGz);

            int4
                xCA = select(x0, x1, xA),
                xCB = select(x0, x1, xB),
                yCA = select(y0, y1, yA),
                yCB = select(y0, y1, yB),
                zCA = select(z0, z1, zA),
                zCB = select(z0, z1, zB);

            SmallXXHash4 hashx0 = hash.Eat(x0), hashx1 = hash.Eat(x1), 
                                hA = select(hashx0, hashx1, xA),
                                hB = select(hashx0, hashx1, xB);

            //return default(G).EvaluateAfterInterpolation(
            //        Kernel(hashx0.Eat(y0).Eat(z0), x0, y0, z0, positions) +
            //        Kernel(hashx1.Eat(y1).Eat(z1), x1, y1, z1, positions) +
            //        Kernel(hA.Eat(yCA).Eat(zCA), xCA, yCA, zCA, positions) +
            //        Kernel(hB.Eat(yCB).Eat(zCB), xCB, yCB, zCB, positions)) / 4f;
            return default(G).EvaluateAfterInterpolation(
                    Kernel(hashx0.Eat(y0).Eat(z0), x0, y0, z0, positions) +
                    Kernel(hashx0.Eat(y0).Eat(z1), x0, y0, z1, positions) +
                    Kernel(hashx0.Eat(y1).Eat(z0), x0, y1, z0, positions) +
                    Kernel(hashx0.Eat(y1).Eat(z1), x0, y1, z1, positions) +
                    Kernel(hashx1.Eat(y0).Eat(z0), x1, y0, z0, positions) +
                    Kernel(hashx1.Eat(y0).Eat(z1), x1, y0, z1, positions) +
                    Kernel(hashx1.Eat(y1).Eat(z0), x1, y1, z0, positions) +
                    Kernel(hashx1.Eat(y1).Eat(z1), x1, y1, z1, positions)) / 4f;
        }
    }
}