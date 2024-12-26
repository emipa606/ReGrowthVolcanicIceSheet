using System.Collections.Generic;
using ReGrowthCore;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RGW_VolcanicIceSheet;

public class RGW_VolcanicIceSheet_BiomeWorker : UniversalBiomeWorker
{
    public override float GetScore(Tile tile, int tileID)
    {
        var waterCovered = tile.WaterCovered;
        float result;
        if (waterCovered)
        {
            result = -100f;
        }
        else
        {
            var perlin = new Perlin(0.079999998211860657, 2.0, 1.0, 6, Rand.Int, QualityMode.High);
            var tileCenter = Find.WorldGrid.GetTileCenter(tileID);
            var value = perlin.GetValue(tileCenter);
            if (value > 1.5f && !AtCoats(tileID))
            {
                result = PermaIceScore(tile) + 0.01f;
            }
            else
            {
                result = -100f;
            }
        }

        return result;
    }

    public static float PermaIceScore(Tile tile)
    {
        return -20f + ((0f - tile.temperature) * 2f);
    }

    public static bool AtCoats(int tileID)
    {
        var list = new List<int>();
        Find.WorldGrid.GetTileNeighbors(tileID, list);
        foreach (var tileID2 in list)
        {
            try
            {
                if (Find.WorldGrid[tileID2].WaterCovered ||
                    Find.WorldGrid[tileID2].biome == BiomeDefOf.Ocean ||
                    Find.World.CoastDirectionAt(tileID) != Rot4.Invalid)
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }
        }

        return false;
    }
}