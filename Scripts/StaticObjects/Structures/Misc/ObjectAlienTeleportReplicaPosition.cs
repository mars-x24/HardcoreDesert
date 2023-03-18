using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
using AtomicTorch.CBND.CoreMod.Helpers.Client;
using AtomicTorch.CBND.CoreMod.Items;
using AtomicTorch.CBND.CoreMod.Items.Generic;
using AtomicTorch.CBND.CoreMod.SoundPresets;
using AtomicTorch.CBND.CoreMod.StaticObjects;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
using AtomicTorch.CBND.CoreMod.Systems.Construction;
using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
using AtomicTorch.CBND.CoreMod.Systems.Physics;
using AtomicTorch.CBND.CoreMod.Zones;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.World;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting;
using AtomicTorch.CBND.GameApi.ServicesClient.Components;
using AtomicTorch.CBND.GameApi.ServicesServer;
using AtomicTorch.GameEngine.Common.Primitives;
using System;
using System.Collections.Generic;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc
{
  public class ObjectAlienTeleportReplicaPosition : ProtoObjectStructure
  {
    private static readonly Vector2Ushort ComposedTextureSize = (1024, 1076);

    private static readonly IReadOnlyItemLightConfig LightSourceConfig
        = new ItemLightConfig()
        {
          Color = LightColors.Alien,
          LogicalSize = (15, 15),
          Size = (15, 15),
          WorldOffset = (522.0 / ScriptingConstants.TileSizeRealPixels,
                           824.0 / ScriptingConstants.TileSizeRealPixels)
        };

    private static readonly Vector2Ushort TextureResourceBackOffset = (106, 486);

    private static readonly Vector2Ushort TextureResourceBackSize = (813, 590);

    private static readonly Vector2Ushort TextureResourceBaseSize = (1024, 768);

    private static readonly Vector2Ushort TextureResourceFrontOffset = (106, 82);

    private static readonly Vector2Ushort TextureResourceFrontSize = (813, 751);

    private readonly ITextureResource textureResourceBack;

    private readonly TextureResource textureResourceBase;

    private readonly ITextureResource textureResourceFront;

    public ObjectAlienTeleportReplicaPosition()
    {
      var texturePath = $"StaticObjects/Misc/{nameof(ObjectAlienTeleportReplica)}";
      this.textureResourceBack = new TextureResource(texturePath + "Back");
      this.textureResourceBase = new TextureResource(texturePath + "Base");
      this.textureResourceFront = new TextureResource(texturePath + "Front");
    }

    public override string Name => "Alien teleport replica";

    public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

    public override string Description => "Alien teleport replica is a permanent structure, it can't be build in land claim or boss area.";

    public override double ObstacleBlockDamageCoef => 1;

    public override float StructurePointsMax => 100000;

    public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
    {
      var renderer = blueprint.SpriteRenderer;
      renderer.TextureResource = this.Icon;
      renderer.Size = (ComposedTextureSize.X * 0.5,
                       ComposedTextureSize.Y * 0.5);
    }

    public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
    {
      return base.SharedGetObjectCenterWorldOffset(worldObject) + (0, 1.25);
    }

    protected override ITextureResource ClientCreateIcon()
    {
      return ClientProceduralTextureHelper.CreateComposedTexture(
          "Composed " + this.Id,
          isTransparent: true,
          isUseCache: true,
          customSize: ComposedTextureSize,
          textureResourcesWithOffsets: new[]
          {
                    new TextureResourceWithOffset(this.textureResourceBase,
                                                  offset: (0, TextureResourceBaseSize.Y - ComposedTextureSize.Y)),
                    new TextureResourceWithOffset(this.textureResourceBack,
                                                  offset: (0, TextureResourceBackSize.Y - ComposedTextureSize.Y)
                                                          + TextureResourceBackOffset.ToVector2D()),
                    new TextureResourceWithOffset(this.textureResourceFront,
                                                  offset: (0, TextureResourceFrontSize.Y - ComposedTextureSize.Y)
                                                          + TextureResourceFrontOffset.ToVector2D())
          });
    }

    protected override void ClientInitialize(ClientInitializeData data)
    {
      base.ClientInitialize(data);

      Client.Rendering.CreateSpriteRenderer(
          data.GameObject,
          this.textureResourceBack,
          positionOffset: TextureResourceBackOffset.ToVector2D()
                          / ScriptingConstants.TileSizeRealPixels);

      Client.Rendering.CreateSpriteRenderer(
          data.GameObject,
          this.textureResourceFront,
          positionOffset: TextureResourceFrontOffset.ToVector2D()
                          / ScriptingConstants.TileSizeRealPixels);

      ClientLighting.CreateLightSourceSpot(data.GameObject.ClientSceneObject,
                                           LightSourceConfig);
    }

    protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
    {
      base.ClientSetupRenderer(renderer);
      renderer.DrawOrder = DrawOrder.Floor + 1;
    }

    protected override void CreateLayout(StaticObjectLayout layout)
    {
      layout.Setup("####",
                   "####",
                   "####");
    }

    protected override void PrepareConstructionConfig(
      ConstructionTileRequirements tileRequirements,
      ConstructionStageConfig build,
      ConstructionStageConfig repair,
      ConstructionUpgradeConfig upgrade,
      out ProtoStructureCategory category)
    {
      tileRequirements.Clear();
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoFarmPlot);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoFloor);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoNpcsAround);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoPlatforms);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoPlayersNearby);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNoStaticObjects);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorNotCliffOrSlope);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorSameHeightLevelAsPlayer);
      tileRequirements.Add(ConstructionTileRequirements.ValidatorSolidGround);
      tileRequirements.Add(LandClaimSystem.ValidatorFreeLandEvenForServer);
      tileRequirements.Add(ValidatorTooCloseToAnotherTeleport);
      tileRequirements.Add(ValidatorNoImportantZones);

      category = GetCategory<StructureCategoryOther>();

      build.StagesCount = 4;
      build.StageDurationSeconds = BuildDuration.VeryLong;

      build.AddStageRequiredItem<ItemTeleportAlien1>(count: 1);
      build.AddStageRequiredItem<ItemTeleportAlien2>(count: 1);
      build.AddStageRequiredItem<ItemTeleportAlien3>(count: 3);
      build.AddStageRequiredItem<ItemTeleportAlien4>(count: 1);

    }

    private const int MinDistanceBetweenTeleport = 50;
    public const string ErrorTooCloseToAnotherTeleport = "Too close to another teleport";
    private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToAnotherTeleport
            = new(ErrorTooCloseToAnotherTeleport,
                  c =>
                  {
                    var startPosition = c.StartTilePosition;
                    var objectsInBounds = SharedFindObjectsNearby<ProtoObjectTeleport>(startPosition);
                    foreach (var obj in objectsInBounds)
                    {
                      if (ReferenceEquals(obj, c.ObjectToRelocate))
                      {
                        continue;
                      }

                      switch (obj.ProtoGameObject)
                      {
                        case ProtoObjectTeleport:
                          return false;

                        case ProtoObjectConstructionSite
                              when ProtoObjectConstructionSite.SharedGetConstructionProto(obj) is
                                       ProtoObjectTeleport:
                          return false;
                      }
                    }

                    return true;
                  });

    public const string ErrorNoImportantZones = "Not allowed on roads, ruins, giant source or boss zones";
    private static readonly ConstructionTileRequirements.Validator ValidatorNoImportantZones
            = new(ErrorNoImportantZones,
                  c =>
                  {
                    if (IsClient)
                      return true;

                    var list = new List<ProtoZoneDefault>();
                    list.Add(Api.GetProtoEntity<ZoneEventFinalBoss>());
                    list.Add(Api.GetProtoEntity<ZoneEventBoss>());
                    list.Add(Api.GetProtoEntity<ZoneGenericRoads>());
                    list.Add(Api.GetProtoEntity<ZoneGiantPragmiumSource>());
                    list.Add(Api.GetProtoEntity<ZoneRuinsLootIndustrial>());
                    list.Add(Api.GetProtoEntity<ZoneRuinsLootLaboratory>());
                    list.Add(Api.GetProtoEntity<ZoneRuinsLootMilitary>());
                    list.Add(Api.GetProtoEntity<ZoneRuinsLootResidential>());
                    list.Add(Api.GetProtoEntity<ZoneRuinsMobsNormal>());
                    list.Add(Api.GetProtoEntity<ZoneSnowAlien>());

                    foreach (var zone in list)
                    {
                      if (zone is null || zone.ServerZoneInstance.IsEmpty)
                        continue;

                      if (zone.ServerZoneInstance.IsContainsPosition(c.StartTilePosition))
                        return false;
                    }

                    return true;
                  });

    protected static IEnumerable<IStaticWorldObject> SharedFindObjectsNearby
        <TProtoObject>(Vector2Ushort startPosition)
        where TProtoObject : class, IProtoStaticWorldObject
    {
      var world = IsServer
                      ? (IWorldService)Server.World
                      : (IWorldService)Client.World;

      var bounds = new RectangleInt(startPosition, size: (1, 1));
      bounds = bounds.Inflate(MinDistanceBetweenTeleport);

      var objectsInBounds = world.GetStaticWorldObjectsOfProtoInBounds<TProtoObject>(bounds);
      return objectsInBounds;
    }

    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return this.textureResourceBase;
    }

    protected override void SharedCreatePhysics(CreatePhysicsData data)
    {
      data.PhysicsBody
          // bl
          .AddShapeRectangle(size: (0.8, 0.4), offset: (0.4, 0.4))
          .AddShapeRectangle(size: (0.8, 0.5), offset: (0.4, 0.5), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6, 0.4), offset: (0.5, 1.1), group: CollisionGroups.HitboxRanged)
          // br
          .AddShapeRectangle(size: (0.8, 0.4), offset: (2.8, 0.4))
          .AddShapeRectangle(size: (0.8, 0.5), offset: (2.8, 0.5), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6, 0.4), offset: (2.9, 1.1), group: CollisionGroups.HitboxRanged)
          // tl
          .AddShapeRectangle(size: (0.8, 0.4), offset: (0.4, 1.9))
          .AddShapeRectangle(size: (0.8, 0.5), offset: (0.4, 2.0), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6, 0.4), offset: (0.5, 2.6), group: CollisionGroups.HitboxRanged)
          // tr
          .AddShapeRectangle(size: (0.8, 0.4), offset: (2.8, 1.9))
          .AddShapeRectangle(size: (0.8, 0.5), offset: (2.8, 2.0), group: CollisionGroups.HitboxMelee)
          .AddShapeRectangle(size: (0.6, 0.4), offset: (2.9, 2.6), group: CollisionGroups.HitboxRanged)
          // click
          .AddShapeRectangle(size: (3.6, 2.2), offset: (0.2, 0.15), group: CollisionGroups.ClickArea);
    }

    public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
    {
      base.ServerOnBuilt(structure, byCharacter);

      //change the structure for the real teleporter
      var position = structure.TilePosition;
      Server.World.DestroyObject(structure);
      Server.World.CreateStaticWorldObject<ObjectAlienTeleportReplica>(position);
    }
  }
}