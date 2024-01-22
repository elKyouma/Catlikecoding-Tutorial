using uint4 = Unity.Mathematics.uint4;
using int4 = Unity.Mathematics.int4;
using Unity.Mathematics;

public readonly struct SmallXXHash4
{

    const uint primeB = 0b10000101111010111100101001110111;
    const uint primeC = 0b11000010101100101010111000111101;
    const uint primeD = 0b00100111110101001110101100101111;
    const uint primeE = 0b00010110010101100110011110110001;

    readonly uint4 accumulator;


    public static implicit operator SmallXXHash4(uint4 accumulator) => new(accumulator);

    public static SmallXXHash4 operator +(SmallXXHash4 h, int v) => h.accumulator + (uint)v;

    public SmallXXHash4(uint4 accumulator)
    {
        this.accumulator = accumulator;
    }

    public uint4 GetBits(int count, int shift) => ((uint4)this >> shift) & ((uint4)math.pow(2, count) - 1);
    public uint4 BytesA() => (uint4)this & 255;
    public uint4 BytesB() => (uint4)this >> 8 & 255;
    public uint4 BytesC() => (uint4)this >> 16 & 255;
    public uint4 BytesD() => (uint4)this >> 24 & 255;

    public float4 GetBitsF(int count, int shift) => (float4)(((uint4)this >> shift) & ((uint4)math.pow(2, count) - 1)) / ((uint4)math.pow(2, count) - 1);
    public float4 FloatsA() => (float4)BytesA() / 255f;
    public float4 FloatsB() => (float4)BytesA() / 255f;
    public float4 FloatsC() => (float4)BytesA() / 255f;
    public float4 FloatsD() => (float4)BytesA() / 255f;


    public static implicit operator uint4(SmallXXHash4 hash)
    {
        uint4 avalanche = hash.accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= primeB;
        avalanche ^= avalanche >> 13;
        avalanche *= primeC;
        avalanche ^= avalanche >> 16;
        return avalanche;
    }

    public static SmallXXHash4 Seed(int4 seed) => (uint4)seed + primeE;
    public SmallXXHash4 Eat(int4 data) => RotateLeft(accumulator + (uint4)data * primeC, 17) * primeD;
    public static uint4 RotateLeft(uint4 data, int steps) => (data << steps) | (data >> 32 - steps);

}
