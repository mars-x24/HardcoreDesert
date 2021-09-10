namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
  using AtomicTorch.CBND.GameApi.ServicesClient.Components;

  public abstract class ProtoCharacterSkeletonNPC : ProtoCharacterSkeletonAnimalNPC
  {

    public override void ClientResetItemInHand(IComponentSkeleton skeletonRenderer)
    {
      skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand,
                                           attachmentName: "WeaponMelee",
                                           textureResource: null);
      skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand,
                                           attachmentName: "WeaponRanged",
                                           textureResource: null);
      skeletonRenderer.SetAttachment(this.SlotNameItemInHand, attachmentName: "WeaponRanged");
    }
  }
}