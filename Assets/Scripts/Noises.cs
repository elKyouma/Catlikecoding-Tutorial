using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;

using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using UnityEngine;
using static Noises;

public static partial class Noises
{
    public struct LatticeSpan4
    {
        public int4 p0, p1;
        public float4 g0, g1;
        public float4 t;
    }
    public interface INoise
    {
        float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency);
    }

    public interface ILattice
    {
        public LatticeSpan4 GetLatticeSpan(float4 coordinate, int frequency);
        public int4 ValidateSingleStep(int4 points, int frequency);
    }

    public struct LatticeNormal : ILattice
    {
        public LatticeSpan4 GetLatticeSpan(float4 coordinate, int frequency)
        {
            coordinate *= frequency;

            LatticeSpan4 span;
            span.p0 = (int4)floor(coordinate);
            span.p1 = span.p0 + 1;
            span.g0 = coordinate - span.p0;
            span.g1 = span.g0 - 1;
            span.t = coordinate - span.p0;
            span.t = span.t * span.t * span.t * (span.t * (span.t * 6f - 15f) + 10f);
            return span;
        }
        public int4 ValidateSingleStep(int4 points, int frequency) => points;
    }

    public struct LatticeTiled : ILattice
    {
        public LatticeSpan4 GetLatticeSpan(float4 coordinate, int frequency)
        {
            coordinate *= frequency;

            float4 points = floor(coordinate);
            LatticeSpan4 span;
            span.g0 = coordinate - points;
            span.g1 = span.g0 - 1;

            span.t = coordinate - points;
            span.t = span.t * span.t * span.t * (span.t * (span.t * 6f - 15f) + 10f);

            span.p0 = (int4)points - (int4)ceil(points / frequency) * frequency;
            span.p0 = select(span.p0 + frequency, span.p0, span.p0 >= 0);
            span.p1 = span.p0 + 1;
            span.p1 = select(span.p1, 0, span.p1 == frequency);
            return span;
        }

        public int4 ValidateSingleStep(int4 points, int frequency) =>
            select(select(points, 0, points == frequency), frequency - 1, points == -1);
    }

    public struct Lattice1D<GRADIENT, LATTICE> : INoise where GRADIENT : struct, IGradient where LATTICE : struct, ILattice
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(LATTICE);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            GRADIENT g = default;

            return lerp(g.Evaluate(hash.Eat(x.p0), x.g0), g.Evaluate(hash.Eat(x.p1), x.g1), x.t);
        }
    }

    public struct Lattice1DT<GRADIENT, LATTICE> : INoise where GRADIENT : struct, IGradient where LATTICE : struct, ILattice
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(LATTICE);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            LatticeSpan4 t = l.GetLatticeSpan(positions.c3 * (hash.FloatsB() + 0.5f), frequency);

            GRADIENT g = default;
            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1);
            return g.EvaluateAfterInterpolation(lerp(lerp(g.Evaluate(h0.Eat(t.p0), x.g0, t.g0), g.Evaluate(h0.Eat(t.p1), x.g0, t.g1), t.t),
                        lerp(g.Evaluate(h1.Eat(t.p0), x.g1, t.g0), g.Evaluate(h1.Eat(t.p1), x.g1, t.g1), t.t), x.t));
        }
    }

    public struct Lattice2D<GRADIENT, LATTICE> : INoise where GRADIENT : struct, IGradient where LATTICE : struct, ILattice
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(LATTICE);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            LatticeSpan4 z = l.GetLatticeSpan(positions.c2, frequency);
            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1);
            GRADIENT g = default;
            return g.EvaluateAfterInterpolation(lerp(lerp(g.Evaluate(h0.Eat(z.p0), x.g0, z.g0), g.Evaluate(h0.Eat(z.p1), x.g0, z.g1), z.t),
                        lerp(g.Evaluate(h1.Eat(z.p0), x.g1, z.g0), g.Evaluate(h1.Eat(z.p1), x.g1, z.g1), z.t), x.t));
        }
    }

    public struct Lattice2DT<GRADIENT, LATTICE> : INoise where GRADIENT : struct, IGradient where LATTICE : struct, ILattice
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(LATTICE);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            LatticeSpan4 z = l.GetLatticeSpan(positions.c2, frequency);
            LatticeSpan4 t = l.GetLatticeSpan(positions.c3 * (hash.FloatsB() + 0.5f), frequency);

            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1),
                            h00 = h0.Eat(z.p0), h01 = h0.Eat(z.p1),
                            h10 = h1.Eat(z.p0), h11 = h1.Eat(z.p1);

            GRADIENT g = default;

            return g.EvaluateAfterInterpolation(lerp(
                    lerp(lerp(g.Evaluate(h00.Eat(t.p0), x.g0, z.g0, t.g0), g.Evaluate(h00.Eat(t.p1), x.g0, z.g0, t.g1), t.t),
                        lerp(g.Evaluate(h01.Eat(t.p0), x.g0, z.g1, t.g0), g.Evaluate(h01.Eat(t.p1), x.g0, z.g1, t.g1), t.t), z.t),
                    lerp(lerp(g.Evaluate(h10.Eat(t.p0), x.g1, z.g0, t.g0), g.Evaluate(h10.Eat(t.p1), x.g1, z.g0, t.g1), t.t),
                        lerp(g.Evaluate(h11.Eat(t.p0), x.g1, z.g1, t.g0), g.Evaluate(h11.Eat(t.p1), x.g1, z.g1, t.g1), t.t), z.t),
                    x.t));
        }
    }

    public struct Lattice3D<GRADIENT, LATTICE> : INoise where GRADIENT : struct, IGradient where LATTICE : struct, ILattice
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(LATTICE);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            LatticeSpan4 y = l.GetLatticeSpan(positions.c1, frequency);
            LatticeSpan4 z = l.GetLatticeSpan(positions.c2, frequency);

            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1),
                            h00 = h0.Eat(y.p0), h01 = h0.Eat(y.p1),
                            h10 = h1.Eat(y.p0), h11 = h1.Eat(y.p1);

            GRADIENT g = default;

            return g.EvaluateAfterInterpolation(lerp(
                lerp
                (
                    lerp(g.Evaluate(h00.Eat(z.p0), x.g0, y.g0, z.g0), g.Evaluate(h00.Eat(z.p1), x.g0, y.g0, z.g1), z.t),
                    lerp(g.Evaluate(h01.Eat(z.p0), x.g0, y.g1, z.g0), g.Evaluate(h01.Eat(z.p1), x.g0, y.g1, z.g1), z.t), y.t
                ),
                lerp
                (
                    lerp(g.Evaluate(h10.Eat(z.p0), x.g1, y.g0, z.g0), g.Evaluate(h10.Eat(z.p1), x.g1, y.g0, z.g1), z.t),
                    lerp(g.Evaluate(h11.Eat(z.p0), x.g1, y.g1, z.g0), g.Evaluate(h11.Eat(z.p1), x.g1, y.g1, z.g1), z.t), y.t
                ),
                x.t));
        }
    }

    public struct Lattice3DT<GRADIENT, LATTICE> : INoise where GRADIENT : struct, IGradient where LATTICE : struct, ILattice
    {
        public float4 GetNoise4(float4x4 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(LATTICE);
            LatticeSpan4 x = l.GetLatticeSpan(positions.c0, frequency);
            LatticeSpan4 y = l.GetLatticeSpan(positions.c1, frequency);
            LatticeSpan4 z = l.GetLatticeSpan(positions.c2, frequency);
            LatticeSpan4 t = l.GetLatticeSpan(positions.c3 * (hash.FloatsB() + 0.5f), frequency);

            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1),
                            h00 = h0.Eat(y.p0), h01 = h0.Eat(y.p1),
                            h10 = h1.Eat(y.p0), h11 = h1.Eat(y.p1),
                                h000 = h00.Eat(z.p0), h001 = h00.Eat(z.p1),
                                h010 = h01.Eat(z.p0), h011 = h01.Eat(z.p1),
                                h100 = h10.Eat(z.p0), h101 = h10.Eat(z.p1),
                                h110 = h11.Eat(z.p0), h111 = h11.Eat(z.p1);

            GRADIENT g = default;

            return g.EvaluateAfterInterpolation(lerp(
                    lerp(
                        lerp(lerp(g.Evaluate(h000.Eat(t.p0), x.g0, y.g0, z.g0, t.g0), g.Evaluate(h000.Eat(t.p1), x.g0, y.g0, z.g0, t.g1), t.t),
                             lerp(g.Evaluate(h001.Eat(t.p0), x.g0, y.g0, z.g1, t.g0), g.Evaluate(h001.Eat(t.p1), x.g0, y.g0, z.g1, t.g1), t.t), z.t),
                        lerp(lerp(g.Evaluate(h010.Eat(t.p0), x.g0, y.g1, z.g0, t.g0), g.Evaluate(h010.Eat(t.p1), x.g0, y.g1, z.g0, t.g1), t.t),
                             lerp(g.Evaluate(h011.Eat(t.p0), x.g0, y.g1, z.g1, t.g0), g.Evaluate(h011.Eat(t.p1), x.g0, y.g1, z.g1, t.g1), t.t), z.t), y.t),
                    lerp(
                        lerp(lerp(g.Evaluate(h100.Eat(t.p0), x.g1, y.g0, z.g0, t.g0), g.Evaluate(h100.Eat(t.p1), x.g1, y.g0, z.g0, t.g1), t.t),
                             lerp(g.Evaluate(h101.Eat(t.p0), x.g1, y.g0, z.g1, t.g0), g.Evaluate(h101.Eat(t.p1), x.g1, y.g0, z.g1, t.g1), t.t), z.t),
                        lerp(lerp(g.Evaluate(h110.Eat(t.p0), x.g1, y.g1, z.g0, t.g0), g.Evaluate(h110.Eat(t.p1), x.g1, y.g1, z.g0, t.g1), t.t),
                             lerp(g.Evaluate(h111.Eat(t.p0), x.g1, y.g1, z.g1, t.g0), g.Evaluate(h111.Eat(t.p1), x.g1, y.g1, z.g1, t.g1), t.t), z.t), y.t),
                    x.t));
        }
    }
}
