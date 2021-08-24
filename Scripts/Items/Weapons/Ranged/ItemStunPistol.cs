namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemStunPistol : ProtoItemWeaponRangedEnergy
    {
        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override string Description =>
            "Vanguard Technologies stun pistol draws power from equiped power banks to recharge its plasma pragmium condenser producing high voltage energy trails that can stun the target and cause damage even through armor due to its frozing properties.";

        public override double DamageApplyDelay => 0.1;

        public override uint DurabilityMax => 500;

        public override double EnergyUsePerShot => 100;

        public override double FireInterval => 0.4;

        public override string Name => "Vanguard Stun pistol";

        public override double ReadyDelayDuration => 0.2;

        public override double SpecialEffectProbability => 0.8;

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 0.5},
                cycledSequence: new[] { -1.5, -0.5, 0.5, 1.5 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.StunPlasma;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyPlasmaWeapon)
                       .Set(textureScreenOffset: (-20, 12));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 10, // low damage, high stunning power
                armorPiercingCoef: 0.4,
                finalDamageMultiplier: 1.75,
                rangeMax: 8,
                damageDistribution: new DamageDistribution (DamageType.Cold, 1.0));
        }

        public override void SharedOnHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            out bool isDamageStop)
        {
            base.SharedOnHit(weaponCache, damagedObject, damage, hitData, out isDamageStop);

            if (damage < 1)
            {
                return;
            }

            if (IsServer
                && damagedObject is ICharacter damagedCharacter
                && RandomHelper.RollWithProbability(0.8))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(
                    // add dazed effect
                    intensity: 1.0);
            }
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedStunPistol;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnLaserHit(damagedCharacter, damage);
            // also, see SharedOnHit as it adds Dazed
        }
    }
}