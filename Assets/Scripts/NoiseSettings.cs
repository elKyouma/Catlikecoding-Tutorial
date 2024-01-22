using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Noises
{
    [System.Serializable]
    public struct NoiseSettings
    {
        [SerializeField]
        int seed;
        [SerializeField, Min(1)]
        int frequency;
        [SerializeField, Range(1, 5)]
        int octaves;
        [SerializeField, Range(2, 5)]
        int lacunarity;
        [SerializeField, Range(0, 1)]
        float persistance;

        public int Seed { get => seed; }
        public int Frequency { get => frequency; }
        public int Octaves { get => octaves; }
        public int Lacunarity { get => lacunarity; }
        public float Persistance { get => persistance; }
    }
}