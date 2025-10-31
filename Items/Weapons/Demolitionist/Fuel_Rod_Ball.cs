using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Golf;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Fuel_Rod_Ball : ModItem {
		public override void SetStaticDefaults() {
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.shootSpeed = 12f;
			Item.width = 18;
			Item.height = 20;
			Item.maxStack = 1;
			Item.useAnimation = 14;
			Item.useTime = 14;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.damage = 45;
			Item.useStyle = ItemUseStyleID.HiddenAnimation;
			Item.DamageType = DamageClasses.ThrownExplosive;
			Item.shoot = ModContent.ProjectileType<Fuel_Rod_Ball_P>();
			Item.value = Item.sellPrice(gold: 1);
			Item.UseSound = SoundID.Item1;
			Item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
	}
	public class Fuel_Rod_Ball_P : Golf_Ball_Projectile {
		public override string Texture => typeof(Fuel_Rod_Ball).GetDefaultTMLName() + "_Outer";
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Fuel_Rod_Ball).GetDefaultTMLName() + "_Inner";
		public int ChargeLevel => (int)(Projectile.ai[2] / (useTimeMax * 2));
		int useTimeMax = 14;
		Color GlowColor {
			get {
				switch (ChargeLevel) {
					case 1:
					return Color.Orange;
					case 2:
					return Color.Yellow;
					case 3:
					return Color.White;
				}
				return Color.Transparent;
			}
		}
		static void GetShotVelocity(Player player, out Vector2 position, out Vector2 velocity, out float progress) {
			position = player.MountedCenter + Vector2.UnitX * player.direction * player.width * 0.5f;
			velocity = (Main.MouseWorld - position).Normalized(out float dist);
			progress = Utils.GetLerpValue(32, 320, dist, true);
			velocity *= (4 + progress * 20f);
		}
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
			if (source is EntitySource_Parent { Entity: Player player }) {
				Projectile.ai[0] = player.altFunctionUse;
				useTimeMax = player.itemTimeMax;
			}
		}
		public override bool PreAI() {
			Projectile.TryGetOwner(out Player player);
			switch ((int)Projectile.ai[0]) {
				case 2: {
					Projectile.Center = player.MountedCenter + new Vector2(player.direction * 12, Utils.PingPongFrom01To010(++Projectile.ai[1] / useTimeMax) * 16);
					if (Projectile.IsLocallyOwned() && PlayerInput.Triggers.JustPressed.MouseLeft) {
						GetShotVelocity(player, out Projectile.position, out Projectile.velocity, out _);
						Projectile.position -= Projectile.Size * 0.5f;
						Projectile.ai[0] = 0;
						Projectile.ai[1] = 0;
						Projectile.netUpdate = true;
						player.SetDummyItemTime(useTimeMax);
						return false;
					}
					if ((Projectile.ai[1] + useTimeMax / 2) % (useTimeMax * 2) >= useTimeMax) {
						player.heldProj = Projectile.whoAmI;
						player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction * -1.5f);
					} else {
						player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, player.direction * -1.2f);
					}
					if (!player.channel) Projectile.active = false;
					player.SetDummyItemTime(useTimeMax);
					Projectile.ai[2]++;
					if (ChargeLevel > 3) Projectile.Kill();
					return false;
				}
				case 1:
				if (ChargeLevel > 0) Projectile.Kill();
				Projectile.velocity += (player.Center - Projectile.Center).Normalized(out _);
				Projectile.velocity *= 0.97f;
				if (Projectile.Hitbox.Intersects(player.Hitbox)) Projectile.active = false;
				if (++Projectile.localAI[2] >= 60 * 2) Projectile.Kill();
				return base.PreAI();

				default:
				if ((Projectile.IsLocallyOwned() && PlayerInput.Triggers.JustPressed.MouseRight) || (Projectile.velocity.WithinRange(default, 1) && (Projectile.Hitbox.Add(Vector2.UnitY * (1 + Projectile.height)) with { Height = 2 }).OverlapsAnyTiles(false))) {
					Projectile.ai[0] = 1;
					Projectile.netUpdate = true;
				}
				return base.PreAI();
			}
		}
		public override void PostAI() {
			Lighting.AddLight(Projectile.Center, GlowColor.ToVector3() * 0.3f);
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
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)useTimeMax);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			useTimeMax = reader.ReadByte();
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item14);
		}
		public override void PostDraw(Color lightColor) {
			Color color = GlowColor;
			color = Color.Lerp(Color.Chocolate.MultiplyRGBA(lightColor), color, GlowColor.A / 255f);
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
			if (!Projectile.IsLocallyOwned() || Projectile.ai[0] != 2) return;
			Vector2 position = Projectile.position;
			try {
				GetShotVelocity(Main.LocalPlayer, out Projectile.position, out Vector2 impactVelocity, out float progress);
				Projectile.position -= Projectile.Size * 0.5f;
				GolfHelper.DrawPredictionLine(Projectile, impactVelocity, progress, 1);
			} finally {
				Projectile.position = position;
			}
		}
	}
}
