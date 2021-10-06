namespace AtomicTorch.CBND.CoreMod.Events
{
  public interface IProtoEventDelayed
  {
    /// <summary>
    /// Show the event on map only after x seconds
    /// </summary>
    double ShowEventAfter { get; }

    double EventDurationSeconds { get; }
  }
}