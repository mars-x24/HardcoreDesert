namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  using AtomicTorch.CBND.GameApi.Resources;
  using System;

  public class ObjectCrashSiteSpaceship : ProtoObjectCrashSiteSpaceship
  {
    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return new TextureResource("StaticObjects/Misc/Events/ObjectCrashSiteSpaceship",
                                 isTransparent: true);
    }
  }
}