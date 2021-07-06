using AtomicTorch.CBND.CoreMod.Systems.PvE;

namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
  public static class MedicineCooldownDuration
  {
    /// <summary>
    /// This is just a synonym for the most long value.
    /// Used as max value for the status effect.
    /// </summary>
    public const double Maximum = VeryLong;

    public const double None = 0;

    public const double Short = 2; //PveSystem.SharedIsPve(false) ? 2 : 3;

    public const double Medium = 2; // PveSystem.SharedIsPve(false) ? 2 : 5;

    public const double Long = 2; //PveSystem.SharedIsPve(false) ? 2 : 7;

    public const double VeryLong = 2; //PveSystem.SharedIsPve(false) ? 2 : 10;
  }
}