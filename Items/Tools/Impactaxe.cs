using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
namespace Origins.Items.Tools {
	public class Impactaxe : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Tool"
		];
		public override void SetDefaults() {
			Item.damage = 32;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.shoot = ModContent.ProjectileType<Impactaxe_Smash>();
			Item.shootSpeed = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 12f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 0;
		}
		public override bool MeleePrefix() => true;
		public bool? Hardmode => false;
	}
	public class Impactaxe_Smash : ModProjectile {
		public override string Texture => "Origins/Items/Tools/Impactaxe";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = Projectile.height = 32;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				if (itemUse.Entity is Player player) {
					Projectile.ai[1] = player.direction;
					Projectile.scale *= player.GetAdjustedItemScale(itemUse.Item);
				} else {
					Projectile.scale *= itemUse.Item.scale;
				}
			}
			Projectile.ai[2] = float.NaN;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead) {
				Projectile.active = false;
				return;
			}
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			swingFactor = MathHelper.Lerp(MathF.Pow(swingFactor, 2f), MathF.Pow(swingFactor, 0.5f), swingFactor * swingFactor);
			if (!float.IsNaN(Projectile.ai[2])) {
				Projectile.rotation = Projectile.ai[2];
			} else {
				Projectile.rotation = MathHelper.Lerp(-2.75f, 2f, swingFactor) * Projectile.ai[1];
			}
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2) + (Vector2)new PolarVec2(0, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);

			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) / 12f * Projectile.width * 0.85f;
			Vector2 boxPos = Projectile.position + vel;
			Projectile.EmitEnchantmentVisualsAt(boxPos, Projectile.width, Projectile.height);
			if (float.IsNaN(Projectile.ai[2])) {
				if (swingFactor > 0.4f) {
					if (OriginExtensions.BoxOf(boxPos, boxPos + Projectile.Size).OverlapsAnyTiles()) {
						Explode();
					}
				}
			} else {
				Projectile.friendly = false;
			}
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) / 12f * Projectile.width * 0.85f;
			projHitbox.Offset((int)vel.X, (int)vel.Y);
			return projHitbox.Intersects(targetHitbox);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Explode(target.whoAmI);
		}
		void Explode(params int[] immuneTargets) {
			if (float.IsNaN(Projectile.ai[2])) {
				Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) / 12f * Projectile.width * 0.85f;
				Vector2 boxPos = Projectile.position + vel;
				Projectile.ai[2] = Projectile.rotation;
				Vector2 slamDir = vel.RotatedBy(Projectile.ai[1] * MathHelper.PiOver2);
				Collision.HitTiles(boxPos, slamDir, Projectile.width, Projectile.height);
				Vector2 center = boxPos + Projectile.Size * 0.5f;
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, center);

				Projectile proj = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromAI(),
					center,
					vel.RotatedBy(Projectile.ai[1] * -0.5f + Main.rand.NextFloat(-0.2f, 0.4f)) * Main.rand.NextFloat(0.2f, 0.3f),
					ModContent.ProjectileType<Impactaxe_Explosion>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
				for (int i = 0; i < immuneTargets.Length; i++) {
					proj.localNPCImmunity[immuneTargets[i]] = -1;
				}
				Projectile.friendly = false;
				Projectile.netUpdate = true;
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation + Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Projectile.ai[1],
				new Vector2(10, 20 + 10 * Projectile.ai[1]),// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Impactaxe_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.ExplosiveVersion[DamageClass.Melee];
		public override int Size => 72;
		public override bool DealsSelfDamage => true;
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				const int rad = 4;
				Vector2 center = Projectile.Center;
				int i = (int)(center.X / 16);
				int j = (int)(center.Y / 16);
				Projectile.ExplodeTiles(
					center,
					rad,
					i - rad,
					i + rad,
					j - rad,
					j + rad,
					Projectile.ShouldWallExplode(center, rad, i - rad, i + rad, j - rad, j + rad)
				);
			}
			base.AI();
		}
	}
}
