namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class TileSnow : ProtoTile, IProtoTileCold
  {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/Snow/TileSnow1.jpg",
                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new("Terrain/Snow/TileSnow2.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 9;

        public override TextureAtlasResource CliffAtlas { get; }
            = new("Terrain/Cliffs/TerrainCliffsSnow.png",
                columns: 6,
                rows: 4,
                isTransparent: true);

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Snow;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Snow";

        public override string WorldMapTexturePath
            => "Map/Snow.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Rocky"),
                                       // suppression is disabled otherwise ice sound would not be heard
                                       suppressionCoef: 0,
                                       isSupressingMusic: true));

            // rock plates
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector: null));

            // small rocks
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayRough,
                    noiseSelector: new CombinedNoiseSelector(
                        new NoiseSelector(from: 0.7,
                                          to: 1,
                                          noise: new PerlinNoise(seed: 436234631,
                                                                 scale: 23,
                                                                 octaves: 5,
                                                                 persistance: 0.5,
                                                                 lacunarity: 1.7)),
                        new NoiseSelector(from: 0.4,
                                          to: 0.6,
                                          noise: new PerlinNoise(seed: 623743472,
                                                                 scale: 64,
                                                                 octaves: 5,
                                                                 persistance: 0.5,
                                                                 lacunarity: 1.8)))));

            // add rocks
            settings.AddDecalDoubleWithOffset(
                ProtoTileDecal.CollectTextures("Terrain/Snow/SnowRocks*"),
                size: (2, 2),
                drawOrder: DrawOrder.GroundDecalsUnder,
                requiresCompleteProtoTileCoverage: true,
                noiseSelector:
                new CombinedNoiseSelector(
                    new NoiseSelector(
                        from: 0.9,
                        to: 1,
                        noise: new WhiteNoise(seed: 236508123)),
                    new NoiseSelector(
                        from: 0.9,
                        to: 1,
                        noise: new WhiteNoise(seed: 824532465))));

            // add large individual bones decals
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Snow/SnowBones*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.995,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 856234981))));

            settings.AddDecal(
                new ProtoTileDecal("Terrain/Snow/SnowBonesLarge*",
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.995,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 23872153))));
        }
    }
}