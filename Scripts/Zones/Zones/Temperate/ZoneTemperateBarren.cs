﻿using AtomicTorch.CBND.CoreMod.Systems.PvE;
using AtomicTorch.CBND.GameApi;

namespace AtomicTorch.CBND.CoreMod.Zones
{
  public class ZoneTemperateBarren : ProtoZoneDefault
  {
    [NotLocalizable]
    public override string Name => "Temperate - Barren";

    protected override void PrepareZone(ZoneScripts scripts)
    {
      if (!PveSystem.ServerIsPvE)
      {
        // spawn resource deposits on PvP servers
        scripts
            .Add(GetScript<SpawnDepositOilSeep>());
      }

      // plants
      scripts
          .Add(GetScript<SpawnTreesBarren>())
          .Add(GetScript<SpawnBushesBarren>())
          .Add(GetScript<SpawnShrubs>());

      // minerals
      scripts
          .Add(GetScript<SpawnResourcePragmium>())
          .Add(GetScript<SpawnResourceCopper>().Configure(densityMultiplier: 0.10))
          .Add(GetScript<SpawnResourceIron>().Configure(densityMultiplier: 0.10))
          .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.10))
          .Add(GetScript<SpawnResourceSaltpeter>().Configure(densityMultiplier: 0.10))
          .Add(GetScript<SpawnResourceSulfur>().Configure(densityMultiplier: 0.10));

      // mobs
      scripts
          .Add(GetScript<SpawnMobsCloakedLizard>().Configure(densityMultiplier: 0.5))
          .Add(GetScript<SpawnMobDesertPrincess>())
          .Add(GetScript<SpawnMobsHoneyBadger>())
          .Add(GetScript<SpawnMobsAngryPangolin>())
          //.Add(GetScript<SpawnMobsPragmiumBear>())
          //.Add(GetScript<SpawnMobsPragmiumBearKing>())
          .Add(GetScript<SpawnMobsSnakeBrown>())
          .Add(GetScript<SpawnMobsHyena>())
          .Add(GetScript<SpawnMobsThumper>());
    }
  }
}