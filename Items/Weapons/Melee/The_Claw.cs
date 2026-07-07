using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Tools;
using Origins.Misc;
using Origins.NPCs.Brine.Boss;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Sets;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class The_Claw : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Flail
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 55;
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<The_Claw_Hook>();
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<The_Claw_Flail_P>();
				damage /= 2;
				player.StartChanneling(type);
			}
		}
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
	}
	public class The_Claw_Hook : ModProjectile {
		protected static AutoLoadingTexture chainTexture = typeof(The_Claw).GetDefaultTMLName("_Cable");
		protected static AutoGlowingTexture mandibleTextures = typeof(The_Claw).GetDefaultTMLName("_Mandible");
		public override string Texture => typeof(The_Claw_Hook).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = Projectile.height = 18;
			Projectile.aiStyle = ProjAIStyleID.Hook;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = false;
			Projectile.timeLeft *= 10;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 1;
		public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = 6f;
		public override void GrapplePullSpeed(Player player, ref float speed) {
			speed = 8f;
			if (hookTarget != -1) {
				NPC hookTarget = Main.npc.GetIfInRange(this.hookTarget);
				if (hookTarget?.active != true) return;
				speed *= Utils.Remap(
					Vector2.Dot(hookTarget.velocity.Normalized(out float npcSpeed), Main.player[Projectile.owner].MountedCenter.DirectionTo(Projectile.Center)),
					-1,
					1,
					1,
					1 + Math.Min(npcSpeed / 8, 4)
				);
				float preventContactFactor = ((hookTarget.Center.Clamp(player.Hitbox) - player.MountedCenter.Clamp(hookTarget.Hitbox)) / 32).LengthSquared();
				if (preventContactFactor < 1) speed *= float.Sqrt(preventContactFactor) * 2 - 1;
			}
		}
		public override float GrappleRange() => 440;
		public override bool? GrappleCanLatchOnTo(Player player, int x, int y) {
			if (hookTarget != -1 && Projectile.ai[0] == 2) return true;
			return base.GrappleCanLatchOnTo(player, x, y);
		}
		public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) {
			base.GrappleTargetPoint(player, ref grappleX, ref grappleY);
		}
		int hookTarget = -1;
		public override bool? CanDamage() => hookTarget == -1;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.aiStyle = -1;
			Projectile.ai[0] = 2;
			hookTarget = target.whoAmI;
			(Projectile.ai[1], Projectile.ai[2]) = (Projectile.Center.Clamp(target.Hitbox) - target.Center).RotatedBy(-target.rotation);
			Projectile.velocity = default;
			Projectile.netUpdate = true;
		}
		public override void AI() {
			if (hookTarget != -1) {
				Projectile.aiStyle = ProjAIStyleID.Hook;
				switch (Projectile.ai[0]) {
					case 2: {
						NPC hookTarget = Main.npc.GetIfInRange(this.hookTarget);
						if (hookTarget?.active != true) {
							Projectile.ai[0] = 1f;
							return;
						}
						Vector2 newCenter = hookTarget.Center + new Vector2(Projectile.ai[1], Projectile.ai[2]).RotatedBy(hookTarget.rotation);
						Projectile.Center = newCenter;
						Vector2 ownerDiff = Projectile.Center - Main.player[Projectile.owner].MountedCenter;
						break;
					}

					default:
					hookTarget = -1;
					break;
				}
			}
			if (Projectile.velocity != default) {
				Projectile.localAI[1] = Projectile.rotation - MathHelper.PiOver2;
			}
			Projectile.rotation = Projectile.localAI[1] + MathHelper.PiOver2;
		}
		public override bool PreDrawExtras() {
			Rectangle frame = chainTexture.Value.Bounds;
			chainTexture.Value.DrawChain(
				Projectile.Center, Main.player[Projectile.owner].MountedCenter,
				i => frame,
				10,
				verticalChainTexture: true
			);
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			const float close_rot = 0.4f;
			Vector2 position = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.rotation + MathHelper.Pi;
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				position,
				null,
				lightColor,
				rotation,
				new(30, 34),
				Projectile.scale,
				SpriteEffects.None
			);
			Main.EntitySpriteDraw(data);
			if (Projectile.ai[0] >= 2 || hookTarget != -1) data.rotation = rotation - close_rot;
			Vector2 xFactor = rotation.ToRotationVector2();
			Vector2 yFactor = xFactor.YX();
			xFactor.Y *= -1;
			data.texture = mandibleTextures.Texture;
			data.position = position + new Vector2(-23, 13).MatrixMult(xFactor, yFactor);
			data.origin = new(7, 7);
			Main.EntitySpriteDraw(data);
			data.texture = mandibleTextures.GlowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);

			if (Projectile.ai[0] >= 2 || hookTarget != -1) data.rotation = rotation + close_rot;
			data.texture = mandibleTextures.Texture;
			data.color = lightColor;
			data.position = position + new Vector2(23, 13).MatrixMult(xFactor, yFactor);
			data.origin = new(15, 7);
			data.effect = SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(data);
			data.texture = mandibleTextures.GlowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			projHitbox.Inflate(12, 12);
			return projHitbox.Add((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 16).Intersects(targetHitbox);
		}
	}
	public class The_Claw_Flail_P : The_Claw_Hook {
		const int ai_state_spinning = 0;
		const int ai_state_launching_forward = 1;
		const int ai_state_retracting = 2;
		const int ai_state_unused_state = 3;
		const int ai_state_forced_retracting = 4;
		const int ai_state_ricochet = 5;
		const int ai_state_dropping = 6;
		public override string Texture => typeof(The_Claw_Hook).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.aiStyle = ProjAIStyleID.Flail;
			Projectile.tileCollide = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.extraUpdates = 0;
		}
		public override void AI() {
			Projectile.rotation = (Projectile.Center - Main.player[Projectile.owner].MountedCenter).ToRotation() + MathHelper.PiOver2;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[0] == 0f) {
				Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
				Vector2 diff = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
				diff.Y /= 0.8f;
				float num = 77f;
				return diff.LengthSquared() <= num * num;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }
	}
	public struct AutoGlowingTexture(string asset) : IUnloadable, IBatchLoadable {
		AutoLoadingTexture texture = asset;
		AutoLoadingTexture glowTexture = asset + "_Glow";
		public Texture2D Texture => texture.Value;
		public Texture2D GlowTexture => glowTexture.Value;
		public static implicit operator AutoGlowingTexture(string asset) => new(asset);
		void IBatchLoadable.Load() {
			texture.LoadAsset();
			glowTexture.LoadAsset();
		}
		void IUnloadable.Unload() {
			texture.Unload();
			glowTexture.Unload();
		}
		void IBatchLoadable.Wait() {
			texture.Wait();
			glowTexture.Wait();
		}
	}
}
