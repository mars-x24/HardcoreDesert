﻿using AtomicTorch.CBND.CoreMod.Characters;
using AtomicTorch.CBND.CoreMod.Characters.Player;
using AtomicTorch.CBND.CoreMod.ClientOptions.General;
using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
using AtomicTorch.CBND.GameApi.Data.Characters;
using System;

namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data
{
  public class ViewModelCharacterOverlayControl : BaseViewModel
  {
    private readonly Action callbackVisualStateChanged;

    private readonly ICharacter character;

    private readonly ICharacterPublicState publicState;

    private string visualStateName;

    public ViewModelCharacterOverlayControl(ICharacter character, Action callbackVisualStateChanged)
    {
      this.character = character;
      this.callbackVisualStateChanged = callbackVisualStateChanged;
      this.publicState = character.GetPublicState<ICharacterPublicState>();

      if (!character.IsNpc)
      {
        this.ViewModelCharacterNameControl = new ViewModelCharacterNameControl(character);
      }

      this.ViewModelCharacterUnstuckInfoControl = new ViewModelCharacterUnstuckInfoControl(character);

      this.ViewModelCharacterHealthBarControl = new ViewModelCharacterHealthBarControl();
      this.ViewModelCharacterHealthBarControl.CharacterCurrentStats = this.publicState.CurrentStats;
      this.ViewModelCharacterHealthBarControl.MobPublicState = this.publicState as CharacterMobPublicState; //MOD

      this.ViewModelCharacterPublicStatusEffects = new ViewModelCharacterPublicStatusEffects(
          this.publicState.CurrentPublicStatusEffects);

      ClientUpdateHelper.UpdateCallback += this.Update;

      this.visualStateName = this.GetDesiredVisualStateName();
    }

    public ViewModelCharacterHealthBarControl ViewModelCharacterHealthBarControl { get; }

    public ViewModelCharacterNameControl ViewModelCharacterNameControl { get; }

    public ViewModelCharacterPublicStatusEffects ViewModelCharacterPublicStatusEffects { get; }

    public ViewModelCharacterUnstuckInfoControl ViewModelCharacterUnstuckInfoControl { get; }

    public string GetVisualStateName()
    {
      return this.visualStateName;
    }

    protected override void DisposeViewModel()
    {
      base.DisposeViewModel();
      ClientUpdateHelper.UpdateCallback -= this.Update;
    }

    private string GetDesiredVisualStateName()
    {
      if (this.publicState.IsDead)
      {
        return "Collapsed";
      }

      if (!ClientTimeOfDayVisibilityHelper.ClientIsObservable(this.character))
      {
        return "Collapsed";
      }

      var isVisible = true;

      if (!this.character.IsNpc)
      {
        if (this.character.IsCurrentClientCharacter)
        {
          isVisible = GeneralOptionDisplayHealthbarAboveCurrentCharacter.IsDisplay
                      && this.character.ProtoGameObject.GetType() == typeof(PlayerCharacter);
        }

        if (isVisible
            && (((PlayerCharacterPublicState)this.publicState)
                .CurrentPublicActionState is CharacterLaunchpadEscapeAction.PublicState))
        {
          // launching on a rocket
          isVisible = false;
        }
      }

      return isVisible
                  ? "Visible"
                  : "Collapsed";
    }

    private void SetVisualStateName(string stateName)
    {
      if (this.visualStateName == stateName)
      {
        return;
      }

      this.visualStateName = stateName;
      this.callbackVisualStateChanged();
    }

    private void Update()
    {
      this.SetVisualStateName(this.GetDesiredVisualStateName());
    }
  }
}