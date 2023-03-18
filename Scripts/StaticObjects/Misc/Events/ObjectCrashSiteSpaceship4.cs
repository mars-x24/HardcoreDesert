using AtomicTorch.CBND.GameApi.Resources;
using System;

namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
  public class ObjectCrashSiteSpaceship4 : ProtoObjectCrashSiteSpaceship
  {
    protected override ITextureResource PrepareDefaultTexture(Type thisType)
    {
      return new TextureResource("StaticObjects/Misc/Events/ObjectCrashSiteSpaceship4",
                                 isTransparent: true);
    }
  }
}