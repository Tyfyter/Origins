using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Fuel_Rod_Ball : ModItem {
		public override void SetStaticDefaults() {
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GolfBallDyedBlack);
			Item.damage = 45;
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.shoot = ModContent.ProjectileType<Fuel_Rod_Ball_P>();
			Item.value = Item.sellPrice(gold: 1);
			Item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
	}
	public class Fuel_Rod_Ball_P : Golf_Ball_Projectile {
		public override string Texture => typeof(Fuel_Rod_Ball).GetDefaultTMLName() + "_Outer";
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Fuel_Rod_Ball).GetDefaultTMLName() + "_Inner";
		public int ChargeLevel => (int)(Projectile.ai[2] / 28);
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClasses.ThrownExplosive;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity: Player player }) Projectile.ai[0] = player.altFunctionUse;
		}
		public override bool PreAI() {
			Projectile.TryGetOwner(out Player player);
			switch ((int)Projectile.ai[0]) {
				case 2: {
					Projectile.Center = player.MountedCenter + new Vector2(player.direction * 12, Utils.PingPongFrom01To010(++Projectile.ai[1] / 14) * 16);
					if (Projectile.IsLocallyOwned() && PlayerInput.Triggers.JustPressed.MouseLeft) {
						Projectile.velocity = (Main.MouseWorld - player.MountedCenter).Normalized(out float dist) * (4 + float.Pow(dist, 0.25f) * 4);
						Projectile.ai[0] = 0;
						Projectile.ai[1] = 0;
						return false;
					}
					if (!player.channel) Projectile.active = false;
					Projectile.ai[2]++;
					if (ChargeLevel >= 3) Projectile.Kill();
					return false;
				}
				case 1:
				Projectile.velocity.X += Math.Sign(player.Center.X - Projectile.Center.X);
				if (Projectile.Hitbox.Intersects(player.Hitbox)) Projectile.active = false;
				return base.PreAI();

				default:
				if (Projectile.velocity.WithinRange(default, 1) && (Projectile.Hitbox.Add(Vector2.UnitY * (1 + Projectile.height)) with { Height = 2 }).OverlapsAnyTiles(false)) {
					Projectile.ai[0] = 1;
				}
				return base.PreAI();
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage += ChargeLevel * 0.5f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage += ChargeLevel * 0.5f;
		}
		public override bool? CanDamage() => null;
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Projectile.penetrate-- > 0) Projectile.Kill();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.penetrate > 0) Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item14);
		}
		public override void PostDraw(Color lightColor) {
			Color color = Color.White;
			switch (ChargeLevel) {
				case 0:
				color = Color.Orange;
				break;
				case 1:
				color = Color.Yellow;
				break;
				case 2:
				color = Color.White;
				break;
			}
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.Center - Main.screenPosition,
				null,
				color,
				Projectile.rotation,
				glowTexture.Value.Size() * 0.5f,
				Projectile.scale,
				0
			);
		}
	}
}
