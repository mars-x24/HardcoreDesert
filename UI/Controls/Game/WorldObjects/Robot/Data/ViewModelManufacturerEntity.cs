namespace HardcoreDesert.UI.Controls.Game.WorldObjects.Robot.Data
{
  using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
  using System;

  public class ViewModelManufacturerEntity : BaseViewModel
  {
    private ITextureResource icon;

    private bool isEnabled;

    public readonly IProtoObjectStructure Entity;

    public ViewModelManufacturerEntity(IProtoObjectStructure entity, bool defaultIsEnabled = false)
    {
      Entity = entity;
      Name = entity.Name;
      Id = entity.Id;
      isEnabled = defaultIsEnabled;
    }

    public string Name { get; }

    public string Id { get; }

    public bool IsEnabled
    {
      get => isEnabled;
      set
      {
        if (value == IsEnabled)
        {
          return;
        }

        isEnabled = value;
        NotifyThisPropertyChanged();
        IsEnabledChanged?.Invoke(this);
      }
    }

    public void Load(bool isEnabled)
    {
      this.isEnabled = isEnabled;
      //NotifyThisPropertyChanged();
      NotifyPropertyChanged(nameof(this.IsEnabled));
    }

    public event Action<ViewModelManufacturerEntity> IsEnabledChanged;

    public TextureBrush Icon
    {
      get
      {
        if (icon == null)
        {
          icon = Entity.Icon;

          if (icon == null)
            icon = new TextureResource("Content/Textures/StaticObjects/ObjectUnknown.png");
        }
        return Api.Client.UI.GetTextureBrush(icon);
      }
    }
  }
}