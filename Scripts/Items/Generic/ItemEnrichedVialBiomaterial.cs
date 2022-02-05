namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
  using AtomicTorch.CBND.CoreMod.Technologies;
  using AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics;
  using AtomicTorch.CBND.GameApi.Scripting;

  public class ItemEnrichedVialBiomaterial : ProtoItemGeneric
  {
    public override string Description => "Enriched biomaterial vial.";

    public override ushort MaxItemsPerStack => ItemStackSize.Big;

    public override string Name => "Enriched biomaterial vial";

  }
}