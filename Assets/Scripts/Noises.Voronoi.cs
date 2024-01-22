using Unity.Mathematics;
using float4x4 = Unity.Mathematics.float4x4;
using static Unity.Mathematics.math;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

public static partial class Noises
{
    public interface IVoronoiFunction
    {
        float4 Evaluate(float4x2 minima);
    }

    public struct F1 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 minima) => minima.c0;
    }

    public struct F2 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 minima) => minima.c1;
    }

    public struct F2minusF1 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 minima) => minima.c1 - minima.c0;
    }

    public struct F2plusF1 : IVoronoiFunction
    {
        public float4 Evaluate(float4x2 minima) => (minima.c1 + minima.c0) / 2;
    }

    public interface IVoronoiDistance
    {
        float4 GetDistance(float4 x);
        float4 GetDistance(float4 x, float4 y);
        float4 GetDistance(float4 x, float4 y, float4 z);

        float4x2 Finalize(float4x2 mins);
        float4x2 Finalize2D(float4x2 mins);
        float4x2 Finalize3D(float4x2 mins);
    }

    public struct Worley : IVoronoiDistance
    {
        public float4x2 Finalize(float4x2 mins) => mins;

        public float4x2 Finalize2D(float4x2 mins)
        {
            mins.c0 = sqrt(mins.c0);
            mins.c1 = sqrt(mins.c1);
            return mins;
        }

        public float4x2 Finalize3D(float4x2 mins)
        {
            mins.c0 = sqrt(mins.c0);
            mins.c1 = sqrt(mins.c1);
            return mins;
        }

        public float4 GetDistance(float4 x) => abs(x);
        public float4 GetDistance(float4 x, float4 y) => x * x + y * y;
        public float4 GetDistance(float4 x, float4 y, float4 z) => x * x + y * y + z * z;
    }

    public struct Chebyshev : IVoronoiDistance
    {
        public float4x2 Finalize(float4x2 mins) => mins;
        public float4x2 Finalize2D(float4x2 mins) => mins;
        public float4x2 Finalize3D(float4x2 mins) => mins;

        public float4 GetDistance(float4 x) => abs(x);
        public float4 GetDistance(float4 x, float4 y) => max(abs(x), abs(y));
        public float4 GetDistance(float4 x, float4 y, float4 z) => max(abs(x), max(abs(y), abs(z)));
    }

    public struct Voronoi1D<L, F, D> : INoise where L : struct, ILattice where F : struct, IVoronoiFunction where D : struct, IVoronoiDistance
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            var d = default(D);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            float4x2 minimum = 1f;

            for (int u = -1; u <= 1; u++)
            {
                SmallXXHash4 h = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
                float4 newDist = d.GetDistance(h.FloatsA() + u - x.g0);

                minimum.c1 = select(minimum.c1, minimum.c0, minimum.c0 > newDist);
                minimum.c1 = select(minimum.c1, newDist, minimum.c1 > newDist & minimum.c0 < newDist);
                minimum.c0 = select(minimum.c0, newDist, minimum.c0 > newDist);
                newDist = d.GetDistance(h.FloatsB() + u - x.g0);

                minimum.c1 = select(minimum.c1, minimum.c0, minimum.c0 > newDist);
                minimum.c1 = select(minimum.c1, newDist, minimum.c1 > newDist & minimum.c0 < newDist);
                minimum.c0 = select(minimum.c0, newDist, minimum.c0 > newDist);
            }

            return default(F).Evaluate(d.Finalize(minimum));
        }
    }

    public struct Voronoi2D<L, F, D> : INoise where L : struct, ILattice where F : struct, IVoronoiFunction where D : struct, IVoronoiDistance
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            var d = default(D);
            LatticeSpan4
                x = l.GetLatticeSpan(positions.c0, frequency),
                z = l.GetLatticeSpan(positions.c2, frequency);

            float4x2 minimum = 1f;

            for (int u = -1; u <= 1; u++)
            {
                var hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 h = hx.Eat(l.ValidateSingleStep(z.p0 + v, frequency));
                    float4 newDist = d.GetDistance(h.FloatsA() + u - x.g0, h.FloatsB() + v - z.g0);

                    minimum.c1 = select(minimum.c1, minimum.c0, minimum.c0 > newDist);
                    minimum.c1 = select(minimum.c1, newDist, minimum.c1 > newDist & minimum.c0 < newDist);
                    minimum.c0 = select(minimum.c0, newDist, minimum.c0 > newDist);

                    newDist = d.GetDistance(h.FloatsC() + u - x.g0, h.FloatsD() + v - z.g0);

                    minimum.c1 = select(minimum.c1, minimum.c0, minimum.c0 > newDist);
                    minimum.c1 = select(minimum.c1, newDist, minimum.c1 > newDist & minimum.c0 < newDist);
                    minimum.c0 = select(minimum.c0, newDist, minimum.c0 > newDist);
                }
            }

            return default(F).Evaluate(d.Finalize2D(minimum));
        }
    }

    public struct Voronoi3D<L, F, D> : INoise where L : struct, ILattice where F : struct, IVoronoiFunction where D : struct, IVoronoiDistance
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            var d = default(D);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            LatticeSpan4 y = l.GetLatticeSpan(positions.c1, frequency);
            LatticeSpan4 z = l.GetLatticeSpan(positions.c2, frequency);

            float4x2 minimum = 1f;
            for (int u = -1; u <= 1; u++)
            {
                var hx = hash.Eat(l.ValidateSingleStep(x.p0 + u, frequency));
                for (int v = -1; v <= 1; v++)
                {
                    SmallXXHash4 hz = hx.Eat(l.ValidateSingleStep(z.p0 + v, frequency));
                    for (int w = -1; w <= 1; w++)
                    {
                        SmallXXHash4 h = hz.Eat(l.ValidateSingleStep(y.p0 + w, frequency));

                        float4 newDist = d.GetDistance(h.GetBitsF(5, 0) + u - x.g0, h.GetBitsF(5, 5) + w - y.g0, h.GetBitsF(5, 10) + v - z.g0);

                        minimum.c1 = select(minimum.c1, minimum.c0, minimum.c0 > newDist);
                        minimum.c1 = select(minimum.c1, newDist, minimum.c1 > newDist & minimum.c0 < newDist);
                        minimum.c0 = select(minimum.c0, newDist, minimum.c0 > newDist);
                        newDist = d.GetDistance(h.GetBitsF(5, 15) + u - x.g0, h.GetBitsF(5, 20) + w - y.g0, h.GetBitsF(5, 25) + v - z.g0);

                        minimum.c1 = select(minimum.c1, minimum.c0, minimum.c0 > newDist);
                        minimum.c1 = select(minimum.c1, newDist, minimum.c1 > newDist & minimum.c0 < newDist);
                        minimum.c0 = select(minimum.c0, newDist, minimum.c0 > newDist);
                    }
                }
            }

            return default(F).Evaluate(d.Finalize3D(minimum));
        }
    }
}
