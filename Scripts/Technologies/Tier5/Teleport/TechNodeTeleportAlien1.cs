﻿using AtomicTorch.CBND.CoreMod.CraftRecipes;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Teleport
{
  public class TechNodeTeleportAlien1 : TechNode<TechGroupTeleportT5>
  {
    protected override void PrepareTechNode(Config config)
    {
      config.Effects
            .AddRecipe<RecipeTeleportAlien1>();

      config.SetRequiredNode<TechNodeTeleportAlien3>();
    }
  }
}