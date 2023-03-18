using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
using AtomicTorch.CBND.GameApi.Extensions;
using AtomicTorch.CBND.GameApi.Resources;
using AtomicTorch.CBND.GameApi.Scripting;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HardcoreDesert.UI.Controls.Game.WorldObjects.Robot.Data
{
  public class ViewModelManufacturerEntity : BaseViewModel
  {
    private ITextureResource icon;

    private bool isEnabled;

    public readonly IProtoObjectStructure Entity;

    public ViewModelManufacturerEntity(IProtoObjectStructure entity, bool defaultIsEnabled = false) : base(false)
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

    /// <summary>
    /// Entity icon.
    /// </summary>
    public virtual Brush Icon
    {
      get
      {
        if (icon == null)
        {
          icon = new ProceduralTexture("Robot icon for " + Name,
                  proceduralTextureRequest => GenerateIcon(proceduralTextureRequest),
                  isTransparent: true,
                  isUseCache: false);
        }
        return Api.Client.UI.GetTextureBrush(icon);
      }
    }

    public virtual async Task<ITextureResource> GenerateIcon(
          ProceduralTextureRequest request,
          ushort textureWidth = 512,
          ushort textureHeight = 512,
          sbyte spriteQualityOffset = 0)
    {
      if (!(GetPropertyByName(Entity, "Icon") is ITextureResource iconResource))
      {
        // Default icon.
        iconResource = new TextureResource(
            localFilePath: "Content/Textures/StaticObjects/ObjectUnknown.png",
            qualityOffset: spriteQualityOffset);
      }
      return iconResource;
    }

    private static object GetPropertyByName(object obj, string name)
    {
      return obj?.GetType().ScriptingGetProperty(name)?.GetValue(obj, null);
    }
  }
}