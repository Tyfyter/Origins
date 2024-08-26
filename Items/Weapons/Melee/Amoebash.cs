using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Gores.NPCs;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.NPCs;
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
namespace Origins.Items.Weapons.Melee {
	public class Amoebash : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword"
        ];
		public override void SetDefaults() {
			Item.damage = 87;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 34;
			Item.useAnimation = 34;
			Item.shoot = ModContent.ProjectileType<Amoebash_Smash>();
			Item.shootSpeed = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 12f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 0;
		}
		public override bool MeleePrefix() => true;
		public bool? Hardmode => true;
	}
	public class Amoebash_Smash : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Amoebash";
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

			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.85f;
			Vector2 boxPos = Projectile.position + vel * 2;
			Projectile.EmitEnchantmentVisualsAt(boxPos, Projectile.width, Projectile.height);
			if (swingFactor > 0.4f && float.IsNaN(Projectile.ai[2])) {
				if (OriginExtensions.BoxOf(boxPos, boxPos + Projectile.Size).OverlapsAnyTiles()) {
					Projectile.ai[2] = Projectile.rotation;
					Vector2 slamDir = vel.RotatedBy(Projectile.ai[1] * MathHelper.PiOver2);
					Collision.HitTiles(boxPos, slamDir, Projectile.width, Projectile.height);
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, boxPos + Projectile.Size * 0.5f);

					IEntitySource source = Projectile.GetSource_FromAI();
					int projType = ModContent.ProjectileType<Amoebash_Shrapnel>();
					for (int j = 0; j <= 4; j++) {
						Projectile.NewProjectile(
							source,
							boxPos + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
							vel.RotatedBy(Projectile.ai[1] * -0.5f + Main.rand.NextFloat(-0.2f, 0.4f)) * Main.rand.NextFloat(0.2f, 0.3f),
							projType,
							Projectile.damage / 2,
							Projectile.knockBack * 0.2f,
							Projectile.owner
						);
						Gore.NewGore(
							source,
							boxPos + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
							slamDir * 0.1f,
							Main.rand.Next(R_Effect_Blood1.GoreIDs),
							1f
						);
					}
				}
			}
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = (Projectile.velocity.RotatedBy(Projectile.rotation) / 12f) * Projectile.width * 0.85f;
			for (int j = 1; j <= 2; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		static bool forcedCrit = false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			forcedCrit = false;
			if (!target.noTileCollide && float.IsNaN(Projectile.ai[2])) {
				Rectangle hitbox = target.Hitbox;
				Vector2 dir = Projectile.velocity.RotatedBy(Projectile.rotation + Projectile.ai[1] * 2.5f).SafeNormalize(default);
				hitbox.Offset((dir * 8).ToPoint());
				if (hitbox.OverlapsAnyTiles(fallThrough: false)) {
					Collision.HitTiles(hitbox.TopLeft(), dir, hitbox.Width, hitbox.Height);
					modifiers.SetCrit();
					forcedCrit = true;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 120, targetSeverity: 0.4f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
			target.velocity -= target.velocity * target.knockBackResist;
			if (!float.IsNaN(hit.Knockback)) {
				Vector2 dir = Projectile.velocity.RotatedBy(Projectile.rotation);
				if (!forcedCrit) dir += dir.RotatedBy(Projectile.ai[1] * MathHelper.PiOver2);
				target.velocity += dir.SafeNormalize(default) * hit.Knockback;
			}
			forcedCrit = false;
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
				Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]),
				new Vector2(10, 39 + 28 * Projectile.ai[1]),// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Amoebash_Shrapnel : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Ameballoon_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 3600;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			Projectile.rotation -= MathHelper.PiOver2;
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft < 3597) {
				SoundEngine.PlaySound(SoundID.NPCHit18.WithPitch(0.15f).WithVolumeScale(0.5f), Projectile.Center);
				for (int i = Main.rand.Next(6, 12); i-->0;) {
					Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(0f, 1f), ModContent.GoreType<R_Effect_Blood1_Small>());
				}
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			}
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
}
