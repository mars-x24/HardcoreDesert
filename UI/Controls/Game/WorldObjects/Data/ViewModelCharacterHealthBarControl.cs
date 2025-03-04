﻿using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
using AtomicTorch.CBND.GameApi.Data.State;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
  public class ViewModelCharacterHealthBarControl : BaseViewModel
  {
    public string LevelString => "Level " + this.Level.ToString();// + " H:" + this.characterCurrentStats.HealthCurrent.ToString();

    public int Level => this.MobPublicState is null ? -1 : this.MobPublicState.Level;

    public bool HasLevel => this.Level > 1;

    public CharacterMobPublicState MobPublicState { get; set; }

    private CharacterCurrentStats characterCurrentStats;

    public CharacterCurrentStats CharacterCurrentStats
    {
      get => this.characterCurrentStats;
      set
      {
        if (this.characterCurrentStats == value)
        {
          return;
        }

        if (this.characterCurrentStats is not null)
        {
          this.ReleaseSubscriptions();
        }

        this.characterCurrentStats = value;

        if (this.characterCurrentStats is null)
        {
          return;
        }

        // set current values
        this.StatBar.ValueCurrent = this.characterCurrentStats.HealthCurrent;
        this.StatBar.ValueMax = this.characterCurrentStats.HealthMax;

        // subscribe on updates
        this.characterCurrentStats.ClientSubscribe(
            _ => _.HealthCurrent,
            this.HealthCurrentUpdated,
            this);

        this.characterCurrentStats.ClientSubscribe(
            _ => _.HealthMax,
            this.HealthMaxUpdated,
            this);

        this.HealthCurrentUpdated(this.characterCurrentStats.HealthCurrent);
      }
    }

    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
    public ViewModelHUDStatBar StatBar { get; } = new("Health");

    protected override void DisposeViewModel()
    {
      this.characterCurrentStats = null;
      base.DisposeViewModel();
    }

    private void HealthCurrentUpdated(float healthCurrent)
    {
      this.StatBar.ValueCurrent = healthCurrent;
    }

    private void HealthMaxUpdated(float healthMax)
    {
      this.StatBar.ValueMax = healthMax;
    }
  }
}