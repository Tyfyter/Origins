using Origins.Buffs;
using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;

namespace Origins.Items.Weapons.Crossmod {
	#region without thorium
	public class Watered_Down_Keytar : ModItem {
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public static SoundStyle BassSound => Origins.Sounds.RivenBass;
		public static SoundStyle SynthSound => SoundID.Item132;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
		public static void SetSharedDefaults(Item item, out int cost) {
			item.damage = 54;
			item.DamageType = DamageClass.Generic;
			item.knockBack = 1f;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = ItemUseStyleID.Shoot;
			item.noMelee = true;
			item.shootSpeed = 18;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.Pink;
			cost = 20;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.UseSound = BassSound;
			} else {
				Item.UseSound = SynthSound;
			}
			SoundEngine.PlaySound(Item.UseSound.Value.WithPitchOffset(
				Math.Min(((Main.MouseWorld - player.Center) / new Vector2(Main.screenWidth * 0.4f, Main.screenHeight * 0.4f)).Length(), 1) * 2 - 1
			));
			return null;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Watered_Down_Keytar_Bass>();
			}
		}
		public override void SetDefaults() {
			SetSharedDefaults(Item, out Item.mana);
			Item.shoot = ModContent.ProjectileType<Watered_Down_Keytar_Synth>();
		}
	}
	public class Watered_Down_Keytar_Synth : ModProjectile {
		public const int frame_time = 5;
		public const int frame_count = 1;
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override string Texture => "Origins/Items/Weapons/Crossmod/Keytar_Synth_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frame_count;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * (Projectile.direction - 1);
			Projectile.spriteDirection = Projectile.direction;
			if (++Projectile.frameCounter >= frame_time) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			Lighting.AddLight(Projectile.Center, 0.25f, 0.25f, 0.75f);
		}
		public static void OnHitNPC(NPC target) {
			target.AddBuff(BuffID.Venom, Main.rand.Next(180, 301));
			if (Main.rand.NextBool(4)) target.AddBuff(Toxic_Shock_Debuff.ID, Main.rand.Next(180, 301));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OnHitNPC(target);
		}
	}
	public class Watered_Down_Keytar_Bass : ModProjectile {
		public const int frame_time = 5;
		public const int frame_count = 1;
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override string Texture => "Origins/Items/Weapons/Crossmod/Keytar_Bass_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frame_count;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * (Projectile.direction - 1);
			Projectile.spriteDirection = Projectile.direction;
			if (++Projectile.frameCounter >= frame_time) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			Lighting.AddLight(Projectile.Center, 0.75f, 0.25f, 0.25f);
		}
		public static void OnHitNPC(NPC target) {
			if (target.wet && Main.rand.NextBool(4)) target.AddBuff(Cavitation_Debuff.ID, Main.rand.Next(180, 301));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OnHitNPC(target);
		}
	}
	#endregion without thorium
	#region with thorium
	[ExtendsFromMod("ThoriumMod")]
	public class Watered_Down_Keytar_Thorium : BardItem, ICustomWikiStat {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override void SetStaticDefaults() {
			Empowerments.AddInfo<EmpowermentProlongation>(2);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetBardDefaults() {
			Watered_Down_Keytar.SetSharedDefaults(Item, out int cost);
			Item.shoot = ModContent.ProjectileType<Watered_Down_Keytar_Synth_Thorium>();
			InspirationCost = cost / 10;
		}
		public override void BardUseAnimation(Player player) {
			if (player.altFunctionUse == 2) {
				Item.UseSound = Watered_Down_Keytar.BassSound;
			} else {
				Item.UseSound = Watered_Down_Keytar.SynthSound;
			}
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Watered_Down_Keytar_Bass_Thorium>();
			}
		}
		public string CustomStatPath => WikiPageExporter.GetWikiName(this) + "_Thorium";
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Watered_Down_Keytar_Synth_Thorium : BardProjectile {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.String;
		public override string Texture => "Origins/Items/Weapons/Crossmod/Keytar_Synth_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Watered_Down_Keytar_Synth.frame_count;
		}
		public override void SetBardDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * (Projectile.direction - 1);
			Projectile.spriteDirection = Projectile.direction;
			if (++Projectile.frameCounter >= Watered_Down_Keytar_Synth.frame_time) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			Lighting.AddLight(Projectile.Center, 0.25f, 0.25f, 0.75f);
		}
		public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Watered_Down_Keytar_Synth.OnHitNPC(target);
		}
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Watered_Down_Keytar_Bass_Thorium : BardProjectile {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.String;
		public override string Texture => "Origins/Items/Weapons/Crossmod/Keytar_Bass_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Watered_Down_Keytar_Bass.frame_count;
		}
		public override void SetBardDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * (Projectile.direction - 1);
			Projectile.spriteDirection = Projectile.direction;
			if (++Projectile.frameCounter >= Watered_Down_Keytar_Bass.frame_time) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			Lighting.AddLight(Projectile.Center, 0.75f, 0.25f, 0.25f);
		}
		public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Watered_Down_Keytar_Bass.OnHitNPC(target);
		}
	}
	#endregion with thorium
}