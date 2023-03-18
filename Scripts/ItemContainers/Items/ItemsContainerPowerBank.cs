using AtomicTorch.CBND.CoreMod.Items.Devices;
using AtomicTorch.CBND.GameApi.Data.Characters;
using AtomicTorch.CBND.GameApi.Data.Items;

namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
  public class ItemsContainerPowerBank : BaseItemsContainerFor<IProtoItemPowerBank>
  {
    public override void SharedValidateCanInteract(ICharacter character, IItemsContainer container, bool writeToLog)
    {
      //don't call base - non-world error
    }
  }
}