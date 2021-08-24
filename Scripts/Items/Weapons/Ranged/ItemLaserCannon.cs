namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemLaserCannon : ProtoItemWeaponRangedEnergy
    {
        private static readonly TextureResource TextureResourceBeam
            = new("FX/WeaponTraces/TraceBeamLaser.png");

        public override double CharacterAnimationAimingRecoilDuration => 0.6;

        public override double CharacterAnimationAimingRecoilPower => 1.2;

        public override string Description =>
            "Vanguard Technology laser cannon based on pragmium core technology, its energy condenser allows a high power discharge at the cost of a long cooldown between shots.";

        public override uint DurabilityMax => 400;

        public override double EnergyUsePerShot => 250;

        public override double FireInterval => 1.75;

        public override string Name => "Vanguard Laser cannon";

        public override float ShotVolumeMultiplier => 1.0f;

        public override double SpecialEffectProbability => 0.3;
		
		public override bool IsSemiAutomatic => false;

        public override void ClientOnWeaponHitOrTrace(
            ICharacter firingCharacter,
            Vector2D worldPositionSource,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            in Vector2Ushort fallbackCharacterPosition,
            IReadOnlyList<WeaponHitData> hitObjects,
            in Vector2D endPosition,
            bool endsWithHit)
        {
            ComponentWeaponEnergyBeam.Create(
                sourcePosition: worldPositionSource,
                targetPosition: endPosition,
                traceStartWorldOffset: 0.17,
                texture: TextureResourceBeam,
                beamWidth: 0.5,
                originOffset: Vector2D.Zero,
                duration: 0.35,
                endsWithHit,
                fadeInDistance: 0.1,
                fadeOutDistanceHit: 0.05,
                fadeOutDistanceNoHit: 0.2,
                blendMode: BlendMode.AlphaBlendPremultiplied);
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
                && RandomHelper.RollWithProbability(0.25))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(
                    // add 0.4 seconds of dazed effect
                    intensity: 0.4 / StatusEffectDazed.MaxDuration);
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ComponentWeaponEnergyBeam.PreloadAssets();
            Client.Rendering.PreloadTextureAsync(TextureResourceBeam);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            // see ClientOnWeaponHitOrTrace for the custom laser ray implementation
            return WeaponFireTracePresets.LaserBlue;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyLaserWeaponBlue)
                       .Set(textureScreenOffset: (24, 12));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 40,
                armorPiercingCoef: 0.4,
                finalDamageMultiplier: 1.4,
                rangeMax: 13,
                damageDistribution: new DamageDistribution(DamageType.Heat, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
			return WeaponsSoundPresets.WeaponRangedLaserCannon;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnLaserHit(damagedCharacter, damage);
            // also, see SharedOnHit as it adds Dazed
        }
    }
}