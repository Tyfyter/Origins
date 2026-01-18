using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class The_Muffler : ModItem, IApplyPrefixItem {
		public static bool RollMiasma(float chargeLevel) => float.Max(Main.rand.NextFloat(0f, 3f), Main.rand.NextFloat(0f, 3f)) < chargeLevel;
		public static int MiasmaDuration(float chargeLevel) => 30;
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 14));
		}
		public float efficiency = 1f;
		public override void SetDefaults() {
			Item.damage = 40;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 32;
			Item.height = 32;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = Item.useAnimation = 40;
			Item.knockBack = 6f;
			Item.shootSpeed = 4f;
			Item.shoot = ModContent.ProjectileType<The_Muffler_P>();
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item23;
			Item.channel = true;
			efficiency = 1;
		}
		public override bool MeleePrefix() => true;
		public void ApplyPrefix(int pre) {
			efficiency = 40f / Item.useTime;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "PrefixSpeed") {
					tooltips[i] = new(Mod, "PrefixEfficiency", this.GetLocalization("PrefixEfficiency").Format((int)((efficiency - 1) * 100))) {
						IsModifier = true,
						IsModifierBad = efficiency < 1
					};
					break;
				}
			}
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 4);
	}
	public class The_Muffler_P : ModProjectile {
		public override string Texture => typeof(The_Muffler).GetDefaultTMLName();
		static AutoLoadingAsset<Texture2D> exhaustTexture = typeof(The_Muffler).GetDefaultTMLName("_Exhaust");
		const int exhaust_frames = 12;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 14;
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ChlorophyteJackhammer);
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
		}
		float ChargeLevel => float.Min(Main.player[Projectile.owner].OriginPlayer().mufflerAmount * 0.01f, 3);
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Offset((Projectile.velocity * 2).ToPoint());
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.rotation -= MathHelper.PiOver2;
			Projectile.Center = player.MountedCenter + Projectile.velocity.SafeNormalize(default) * 42 * Projectile.scale;
			Projectile.friendly = false;
			if (Projectile.frameCounter.CycleUp(3 - int.Min((int)ChargeLevel, 2))) {
				Projectile.frameCounter = 0;
				Projectile.frame.CycleUp(Main.projFrames[Type]);
				if ((Projectile.frame % (Main.projFrames[Type] / 2)) == 4) {
					if (The_Muffler.RollMiasma(ChargeLevel)) player.AddBuff(Miasma_Debuff.ID, The_Muffler.MiasmaDuration(ChargeLevel));
					Projectile.friendly = true;
					Vector2 slamPos = Projectile.Center + Projectile.velocity * 2f;
					SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolumeScale(0.15f).WithPitch(1), slamPos);
					for (int i = 4; i-- > 0;) {
						Dust.NewDustPerfect(
							slamPos,
							DustID.Torch,
							new Vector2(0, 1).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi))
						).velocity *= 2;
					}
				}
			}
			if (Projectile.localAI[0].TrySet((int)ChargeLevel)) {
				SoundEngine.PlaySound(SoundID.Research.WithVolumeScale(1).WithPitch(-0.5f), Projectile.Center);
			}
			if (Projectile.ai[2] != 0 && Projectile.localAI[2].CycleUp(3)) {
				SoundEngine.PlaySound(SoundID.DD2_BetsyHurt.WithPitch(0.4f));
				SoundEngine.PlaySound(SoundID.DD2_DrakinHurt.WithPitch(0.4f));
				//SoundEngine.PlaySound(SoundID.Zombie42.WithPitch(0.6f));
				SoundEngine.PlaySound(Origins.Sounds.PowerStomp.WithPitch(-0.5f));
				Projectile.ai[2].CycleUp(exhaust_frames);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Point offset = (Projectile.velocity.Normalized(out _) * -20).ToPoint();
			for (int i = 0; i < 2; i++) {
				projHitbox.Offset(offset);
				if (projHitbox.Intersects(targetHitbox)) return true;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float mufflerAmount = ChargeLevel;
			modifiers.SourceDamage *= (1 + mufflerAmount - int.Min((int)mufflerAmount, 2) * 0.85f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life <= 0) {
				Player player = Main.player[Projectile.owner];
				float efficiency = 1;
				if (player.HeldItem?.ModItem is The_Muffler muffler) efficiency = muffler.efficiency;
				player.OriginPlayer().mufflerAmount += target.lifeMax * efficiency;
				if (Projectile.ai[2] == 0) {
					Projectile.ai[2] = 1;
					Projectile.netUpdate = true;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center + player.gfxOffY * Vector2.UnitY;
			SpriteEffects effects = Projectile.direction * player.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				position - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				effects.ApplyToOrigin(new(72, 26), frame),
				Projectile.scale,
				effects
			);
			frame = exhaustTexture.Value.Frame(verticalFrames: exhaust_frames, frameY: (int)Projectile.ai[2]);
			Main.EntitySpriteDraw(
				exhaustTexture,
				position + new Vector2(-39, -25 * Projectile.direction * player.gravDir).RotatedBy(Projectile.rotation) * Projectile.scale - Main.screenPosition,
				frame,
				Color.White,
				Projectile.rotation,
				effects.ApplyToOrigin(new(27, 13), frame),
				Projectile.scale,
				effects
			);
			return false;
		}
	}
}
