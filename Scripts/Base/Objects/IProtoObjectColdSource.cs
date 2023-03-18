namespace AtomicTorch.CBND.CoreMod.Objects
{
  using AtomicTorch.CBND.GameApi.Data.World;

  public interface IProtoObjectColdSource : IProtoWorldObject
  {
    double ColdIntensity { get; }

    double ColdRadiusMax { get; }

    double ColdRadiusMin { get; }
  }
}