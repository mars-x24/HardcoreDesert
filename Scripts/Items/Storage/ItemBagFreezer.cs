

namespace AtomicTorch.CBND.CoreMod.Items.Storage
{
  using AtomicTorch.CBND.GameApi.Data.Items;
  using AtomicTorch.CBND.GameApi.Resources;

  public class ItemBagFreezer : ProtoItemStorageFridge
  {
    public override byte SlotsCount => 16;

    public override byte SlotsEnergyCount => 1;

    public override double GroundIconScale => 1.0;

    public override double EnergyConsumptionPerSecond => 0.2;

    public override double FreshnessDurationMultiplier => 20;

    public override string Name => "Portable freezer";

    public static TextureResource IconOff = new TextureResource("Content/Textures/Items/Storage/ItemBagFreezer.png");
    public static TextureResource Icon25 = new TextureResource("Content/Textures/Items/Storage/ItemBagFreezer25.png");
    public static TextureResource Icon50 = new TextureResource("Content/Textures/Items/Storage/ItemBagFreezer50.png");
    public static TextureResource Icon75 = new TextureResource("Content/Textures/Items/Storage/ItemBagFreezer75.png");
    public static TextureResource Icon100 = new TextureResource("Content/Textures/Items/Storage/ItemBagFreezer100.png");

    public override ITextureResource ClientGetIcon(IItem item)
    {
      var publicState = GetPublicState(item);

      ITextureResource iconLevel = IconOff;

      if (publicState.IsOn)
      {
        if (publicState.PowerPourcent <= 0)
          iconLevel = this.Icon;

        else if (publicState.PowerPourcent <= 0.25)
          iconLevel = Icon25;

        else if (publicState.PowerPourcent <= 0.50)
          iconLevel = Icon50;

        else if (publicState.PowerPourcent <= 0.75)
          iconLevel = Icon75;

        else
          iconLevel = Icon100;
      }

      return iconLevel;
    }

  }
}