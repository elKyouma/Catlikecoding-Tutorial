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

            return Kernel(hash.Eat(x0), x0, positions) + Kernel(hash.Eat(x1), x1, positions);
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

            return (Kernel(hashx0.Eat(z0), x0, z0, positions) + Kernel(hashx0.Eat(z1), x0, z1, positions) +
                    Kernel(hashx1.Eat(z0), x1, z0, positions) + Kernel(hashx1.Eat(z1), x1, z1, positions)) / 4;
        }
    }

    public struct Simplex3D<G> : INoise where G : struct, IGradient
    {
        static float4 Kernel(SmallXXHash4 hash, float4 lx, float4 ly, float4 lz, float4x4 pos)
        {
            float4 x = pos.c0 - lx;
            float4 y = pos.c1 - ly;
            float4 z = pos.c2 - lz;
            return default(G).Evaluate(hash, x, y, z);
        }

        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            positions *= frequency;
            int4 x0 = (int4)floor(positions.c0);
            int4 y0 = (int4)floor(positions.c1);
            int4 z0 = (int4)floor(positions.c2);
            int4 x1 = x0 + 1;
            int4 y1 = y0 + 1;
            int4 z1 = z0 + 1;

            SmallXXHash4 hashx0 = hash.Eat(x0), hashx1 = hash.Eat(x1),
                            hashx0y0 = hashx0.Eat(y0), hashx0y1 = hashx0.Eat(y1),
                            hashx1y0 = hashx1.Eat(y0), hashx1y1 = hashx1.Eat(y1);

            return  Kernel(hashx0y0.Eat(z0), x0, y0, z0, positions) + Kernel(hashx0y0.Eat(z1), x0, y0, z1, positions) +
                    Kernel(hashx0y1.Eat(z0), x0, y1, z0, positions) + Kernel(hashx0y1.Eat(z1), x0, y1, z1, positions) +
                    Kernel(hashx1y0.Eat(z0), x1, y0, z0, positions) + Kernel(hashx1y0.Eat(z1), x1, y0, z1, positions) +
                    Kernel(hashx1y1.Eat(z0), x1, y1, z0, positions) + Kernel(hashx1y1.Eat(z1), x1, y1, z1, positions);
        }
    }
}