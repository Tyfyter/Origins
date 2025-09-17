using Microsoft.Xna.Framework.Graphics;
using Origins.Gores.NPCs;
using Origins.Items.Weapons.Melee;
using Origins.NPCs;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace Origins.Projectiles.Weapons {
	public abstract class MeleeSlamProjectile : ModProjectile {
		public abstract bool CanHitTiles();
		public virtual void OnHitTiles(Vector2 position, Vector2 direction) { }
		public virtual float GetRotation(float swingFactor) => MathHelper.Lerp(-2.75f, 2f, swingFactor) * Projectile.ai[1];
		public virtual float GetSwingFactor() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			return MathHelper.Lerp(MathF.Pow(swingFactor, 2f), MathF.Pow(swingFactor, 0.5f), swingFactor * swingFactor);
		}
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.ai[1] = itemUse.Player.direction;
			}
			Projectile.ai[2] = float.NaN;
			Projectile.netUpdate = true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || player.CCed) {
				Projectile.active = false;
				return;
			}
			float swingFactor = GetSwingFactor();
			if (!float.IsNaN(Projectile.ai[2])) {
				Projectile.rotation = Projectile.ai[2];
			} else {
				Projectile.rotation = GetRotation(swingFactor);
			}
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false) + (Vector2)new PolarVec2(0, realRotation);

			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation * player.gravDir) / 12f) * Projectile.width * 0.85f * Projectile.scale;
			Vector2 size = Projectile.Size * Projectile.scale;
			Vector2 boxPos = Projectile.Center + vel * 2 - size * 0.5f;
			Projectile.EmitEnchantmentVisualsAt(boxPos, (int)size.X, (int)size.Y);
			if (float.IsNaN(Projectile.ai[2]) && CanHitTiles()) {
				if (OriginExtensions.BoxOf(boxPos, boxPos + size).OverlapsAnyTiles()) {
					Projectile.ai[2] = Projectile.rotation;
					OnHitTiles(boxPos, vel);
					Projectile.netUpdate = true;
				}
			}
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation * Main.player[Projectile.owner].gravDir) / 12f) * Projectile.width * 0.85f * Projectile.scale;
			projHitbox.Inflate((int)((projHitbox.Width * Projectile.scale - projHitbox.Width) * 0.5f), (int)((projHitbox.Height * Projectile.scale - projHitbox.Height) * 0.5f));
			for (int j = 1; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				if (j == 1) {
					int shrink = (int)(-8 * Projectile.scale);
					hitbox.Inflate(shrink, shrink);
				}
				Vector2 offset = vel * (j - 0.5f);
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public DrawData GetDrawData(Color lightColor, Vector2 origin) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			SpriteEffects effects = (Projectile.ai[1] * Main.player[Projectile.owner].gravDir) > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			return new(texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation * Main.player[Projectile.owner].gravDir + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1] * Main.player[Projectile.owner].gravDir),
				origin.Apply(effects, texture.Size()),// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				effects
			);
		}
		public abstract override bool PreDraw(ref Color lightColor);
	}
}
