﻿using AtomicTorch.CBND.CoreMod.Systems.PvE;

namespace AtomicTorch.CBND.CoreMod.Items.Medical
{ 
  public static class MedicineCooldownDuration
  {
    public const double None = 0;

    public static double Short => PveSystem.SharedIsPve(false) ? 2 : 4;

    public static double Medium => PveSystem.SharedIsPve(false) ? 2 : 6;
    public static double Long => PveSystem.SharedIsPve(false) ? 2 : 8;

    public static double VeryLong => PveSystem.SharedIsPve(false) ? 2 : 11;

    /// <summary>
    /// This is just a synonym for the most long value.
    /// Used as max value for the status effect.
    /// Please note: cooldown duration for any medicine cannot exceed this duration.
    /// </summary>
    public static double Maximum => VeryLong;

  }
}