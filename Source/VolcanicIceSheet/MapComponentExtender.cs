using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VolcanicIceSheet;

public class MapComponentExtender(Map map) : MapComponent(map)
{
    private readonly HashSet<HashSet<IntVec3>> groups = [];

    private readonly HashSet<IntVec3> terrains = [];
    public bool verifyFirstTime = true;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref verifyFirstTime, "verifyFirstTime", true, true);
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        if (!verifyFirstTime)
        {
            return;
        }

        RemoveSmallLavaGroups();
        RemoveSmallLavaRocksGroups();
        verifyFirstTime = false;
    }

    public void ProcessCell(IntVec3 initialCell, IntVec3 adjCell)
    {
        if (!terrains.Contains(adjCell))
        {
            return;
        }

        foreach (var group in groups)
        {
            if (!group.Contains(initialCell))
            {
                continue;
            }

            if (group.Contains(adjCell))
            {
                return;
            }

            foreach (var group2 in groups)
            {
                if (group == group2 || !group2.Contains(adjCell))
                {
                    continue;
                }

                if (group.Count > group2.Count)
                {
                    group.AddRange(group2);
                    group2.Clear();
                }
                else
                {
                    group2.AddRange(group);
                    group.Clear();
                }

                return;
            }

            group.Add(adjCell);
            return;
        }

        var hashSet = new HashSet<IntVec3> { initialCell };
        groups.Add(hashSet);
    }

    public void RemoveSmallLavaGroups()
    {
        terrains.Clear();
        groups.Clear();
        foreach (var item in map.AllCells.Where(x => x.GetTerrain(map) == RGW_DefOf.RG_Lava))
        {
            terrains.Add(item);
        }

        foreach (var terrain in terrains)
        {
            for (var i = 0; i < 8; i++)
            {
                ProcessCell(terrain, terrain + GenAdj.AdjacentCells[i]);
            }
        }

        var list = new List<IntVec3>();
        foreach (var group in groups)
        {
            if (group.Count >= 30)
            {
                continue;
            }

            foreach (var item2 in group)
            {
                map.terrainGrid.SetTerrain(item2, RGW_DefOf.RG_LavaRock);
                list.Add(item2);
            }
        }

        var hashSet = new HashSet<IntVec3>();
        foreach (var group2 in groups)
        {
            foreach (var item3 in group2)
            {
                hashSet.Add(item3);
            }
        }

        foreach (var terrain2 in terrains)
        {
            if (hashSet.Contains(terrain2))
            {
                continue;
            }

            map.terrainGrid.SetTerrain(terrain2, RGW_DefOf.RG_LavaRock);
            list.Add(terrain2);
        }

        SpawnSnowIfNeeded(map, list);
    }

    public void RemoveSmallLavaRocksGroups()
    {
        terrains.Clear();
        groups.Clear();
        foreach (var item in map.AllCells.Where(x => x.GetTerrain(map) == RGW_DefOf.RG_LavaRock))
        {
            terrains.Add(item);
        }

        foreach (var terrain in terrains)
        {
            for (var i = 0; i < 8; i++)
            {
                ProcessCell(terrain, terrain + GenAdj.AdjacentCells[i]);
            }
        }

        var list = new List<IntVec3>();
        foreach (var group in groups)
        {
            if (group.Count >= 30)
            {
                continue;
            }

            foreach (var item2 in group)
            {
                map.terrainGrid.SetTerrain(item2, RGW_DefOf.SoilRich);
                list.Add(item2);
            }
        }

        var hashSet = new HashSet<IntVec3>();
        foreach (var group2 in groups)
        {
            foreach (var item3 in group2)
            {
                hashSet.Add(item3);
            }
        }

        foreach (var terrain2 in terrains)
        {
            if (hashSet.Contains(terrain2))
            {
                continue;
            }

            map.terrainGrid.SetTerrain(terrain2, RGW_DefOf.SoilRich);
            list.Add(terrain2);
        }

        SpawnSnowIfNeeded(map, list);
    }

    public void SpawnSnowIfNeeded(Map localMap, List<IntVec3> cells)
    {
        var num = 0;
        for (var i = (int)(GenLocalDate.Twelfth(localMap) - 2); i <= (int)GenLocalDate.Twelfth(localMap); i++)
        {
            var num2 = i;
            if (num2 < 0)
            {
                num2 += 12;
            }

            var twelfth = (Twelfth)num2;
            if (GenTemperature.AverageTemperatureAtTileForTwelfth(localMap.Tile, twelfth) < 0f)
            {
                num++;
            }
        }

        var num3 = 0f;
        switch (num)
        {
            case 0:
                return;
            case 1:
                num3 = 0.3f;
                break;
            case 2:
                num3 = 0.7f;
                break;
            case 3:
                num3 = 1f;
                break;
        }

        if (localMap.mapTemperature.SeasonalTemp > 0f)
        {
            num3 *= 0.4f;
        }

        if (num3 < 0.3)
        {
            return;
        }

        foreach (var cell in cells)
        {
            if (!cell.Roofed(localMap))
            {
                localMap.steadyEnvironmentEffects.AddFallenSnowAt(cell, num3);
            }
        }
    }
}