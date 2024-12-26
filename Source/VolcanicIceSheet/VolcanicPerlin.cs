using Verse;
using Verse.Noise;

namespace RGW_VolcanicIceSheet;

[StaticConstructorOnStartup]
public static class VolcanicPerlin
{
    [Unsaved] public static ModuleBase noiseElevation;

    private static readonly FloatRange ElevationRange = new FloatRange(650f, 750f);

    static VolcanicPerlin()
    {
        SetupNoise();
    }

    private static float FreqMultiplier => 1f;

    private static void SetupNoise()
    {
        var freqMultiplier = FreqMultiplier;
        ModuleBase moduleBase = new Perlin(0.09f * freqMultiplier, 2.0, 0.40000000596046448, 6,
            Rand.Range(0, int.MaxValue), QualityMode.High);
        ModuleBase moduleBase2 = new RidgedMultifractal(0.025f * freqMultiplier, 2.0, 6,
            Rand.Range(0, int.MaxValue), QualityMode.High);
        moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
        moduleBase2 = new ScaleBias(0.5, 0.5, moduleBase2);
        noiseElevation = new Multiply(moduleBase, moduleBase2);
        var rhs = new InverseLerp(noiseElevation, ElevationRange.max, ElevationRange.min);
        noiseElevation = new Multiply(noiseElevation, rhs);
        NoiseDebugUI.StorePlanetNoise(noiseElevation, "noiseVolcanic");
    }
}