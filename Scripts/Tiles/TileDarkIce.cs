using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
using AtomicTorch.CBND.GameApi.Resources;
using System.Windows.Media;

namespace AtomicTorch.CBND.CoreMod.Tiles
{
  public class TileDarkIce : ProtoTileWater, IProtoNoAmbientLight
  {
    public override byte BlendOrder => 205;

    public override IProtoTileWater BridgeProtoTile => null;

    public override bool CanCollect => false;

    public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Mud;

    public override bool IsFishingAllowed => false;

    public override string Name => "Dark Ice";

    public override TextureResource UnderwaterGroundTextureAtlas { get; }
      = new("Terrain/DarkIce/TileDarkIce1.jpg",
            isTransparent: false);

    public override TextureAtlasResource CliffAtlas { get; }
    = new("Terrain/Cliffs/TerrainCliffsSnow.png",
        columns: 6,
        rows: 4,
        isTransparent: true);

    public override string WorldMapTexturePath
            => "Map/Ice.png";

    protected override ITextureResource TextureWaterWorldPlaceholder { get; }
        = new TextureResource("Terrain/DarkIce/TileDarkIcePlaceholder",
                              isTransparent: false);

    protected override float WaterAmplitude => 0.02f;

    protected override Color WaterColor => Color.FromArgb(255, 255, 255, 255);

    protected override float WaterColorMix => 0;

    protected override float WaterDiffractionFrequency => 4.0f;

    protected override float WaterDiffractionSpeed => 0.22f;

    protected override float WaterSpeed => 0.0f;

    protected override TextureResource WaterSufraceTexture { get; }
        = new("Terrain/DarkIce/TileDarkIce1.jpg");

    protected override void PrepareProtoTile(Settings settings)
    {
      base.PrepareProtoTile(settings);

      settings.AmbientSoundProvider = new TileAmbientSoundProvider(
          new AmbientSoundPreset(new SoundResource("Ambient/wind"),
                                 suppressionCoef: 1,
                                 isSupressingMusic: true,
                                 isUsingAmbientVolume: false));
    }
  }
}