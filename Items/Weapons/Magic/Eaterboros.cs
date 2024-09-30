using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
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
namespace Origins.Items.Weapons.Magic {
	public class Eaterboros : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Sword"
		];
		public override void SetDefaults() {
			Item.damage = 27;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 7;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 25;
			Item.useAnimation = 11;
			Item.shoot = ModContent.ProjectileType<Eaterboros_Slash>();
			Item.shootSpeed = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 12f;
			Item.useTurn = false;
			Item.channel = true;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 0;
		}
		public override bool MeleePrefix() => true;
		public bool? Hardmode => true;
	}
	public class Eaterboros_Slash : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Eaterboros_Hilt";
		static AutoLoadingAsset<Texture2D> eaterTexture = "Origins/Items/Weapons/Magic/Eaterboros_Segment_Attached";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
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
			Projectile.ai[2] = 2;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead) {
				Projectile.active = false;
				return;
			}
			bool newEater = false;
			if (player.channel) {
				player.itemTime = player.itemTimeMax;
				player.TryUpdateChannel(Projectile);

				int before = (int)Projectile.ai[2];
				Projectile.ai[2] += 1f / player.itemAnimationMax;
				newEater = before != (int)Projectile.ai[2];
				if ((newEater && !player.CheckMana(player.HeldItem, pay: true)) || Projectile.ai[2] >= 7) player.TryCancelChannel(Projectile);
				player.manaRegenDelay = (int)player.maxRegenDelay;

				if (Main.myPlayer == Projectile.owner) {
					Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(default);
					if (direction.X != Projectile.velocity.X || direction.Y != Projectile.velocity.Y) {
						// This will sync the projectile, most importantly, the velocity.
						Projectile.netUpdate = true;
					}
					Projectile.velocity = direction;
				}
				Projectile.friendly = false;
			} else {
				Projectile.friendly = true;
			}
			if (Projectile.localAI[2] < Projectile.ai[2]) Projectile.localAI[2] = Projectile.ai[2];
			Projectile.ai[1] = player.direction = Math.Sign(Projectile.velocity.X);

			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			swingFactor = MathHelper.Lerp(MathF.Pow(swingFactor, 2f), MathF.Pow(swingFactor, 0.5f), swingFactor * swingFactor);
			Projectile.rotation = MathHelper.Lerp(-2f, 2f, swingFactor) * Projectile.ai[1] * (1 + Projectile.localAI[2] / 14);
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2) + (Vector2)new PolarVec2(0, realRotation);
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);

			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]) * Projectile.width;
			Vector2 boxPos = default;
			Vector2 halfSize = Projectile.Size / 2;
			for (int j = 0; j <= Projectile.ai[2] + 1; j++) {
				boxPos = Projectile.Center + vel * (j + 0.75f) - halfSize;
				Projectile.EmitEnchantmentVisualsAt(boxPos, Projectile.width, Projectile.height);
			}
			if (newEater) {
				for (int i = 0; i < 4; i++) {
					Dust.NewDust(
						boxPos,
						Projectile.width,
						Projectile.height,
						DustID.ScourgeOfTheCorruptor
					);
				}
			}
		}
		public override bool ShouldUpdatePosition() => false;
		int collideIndex = -1;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]) * Projectile.width;
			collideIndex = -1;
			for (int j = 0; j <= Projectile.ai[2] + 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * (j + 0.5f);
				hitbox.Offset((int)offset.X, (int)offset.Y);
				collideIndex = j;
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (collideIndex != -1) {
				if (Main.myPlayer == Projectile.owner) {
					Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]) * Projectile.width;
					Vector2 halfSize = Projectile.Size / 2;
					int projType = ModContent.ProjectileType<Eaterboros_Segment_Free>();
					int lastProj = -1;
					for (int i = (int)Projectile.ai[2] + 2; --i > collideIndex;) {
						lastProj = Projectile.NewProjectile(
							Projectile.GetSource_OnHit(target),
							Projectile.Center + vel * (i + 0.75f),
							vel * 8f / Projectile.width,
							projType,
							Projectile.damage / 2,
							Projectile.knockBack / 5,
							Projectile.owner,
							lastProj
						);
						for (int j = 0; j < 4; j++) {
							Dust.NewDust(
								Projectile.Center + vel * (i + 0.75f) - halfSize,
								Projectile.width,
								Projectile.height,
								DustID.ScourgeOfTheCorruptor
							);
						}
					}
					Projectile.ai[2] = collideIndex - 2;
				}
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool PreDraw(ref Color lightColor) {
			float rotation = Projectile.rotation + Projectile.velocity.ToRotation() - MathHelper.PiOver4 * Projectile.ai[1];
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]) * Projectile.width;
			for (int i = (int)Projectile.ai[2] + 2; --i > 0;) {
				Main.EntitySpriteDraw(
					eaterTexture,
					(Projectile.Center + vel * (i + 0.75f)) - Main.screenPosition,
					null,
					lightColor,
					rotation,
					new Vector2(18, 5),// origin point in the sprite, 'round which the whole sword rotates
					Projectile.scale,
					Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
					0
				);
			}
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				new Vector2(0, 13),// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
	public class Eaterboros_Segment_Free : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.TinyEater);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.friendly = false;
			Projectile.alpha = 0;
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[0] == -1;
		public override void AI() {
			int lastIndex = (int)Projectile.ai[0];
			if (lastIndex != -1) {
				Projectile last = Main.projectile[lastIndex];
				if (!last.active || last.type != Type) {
					Projectile.ai[0] = -1;
					return;
				}
				float dX = last.Center.X - Projectile.Center.X;
				float dY = last.Center.Y - Projectile.Center.Y;
				Projectile.rotation = (float)Math.Atan2(dY, dX);
				if (dX != 0f || dY != 0f) {
					float dist = (float)Math.Sqrt(dY * dY + dX * dX);
					dist = (dist - 16) / dist;
					dX *= dist;
					dY *= dist;
					Projectile.position.X += dX;
					Projectile.position.Y += dY;
				}
				Projectile.velocity = last.velocity;
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.friendly = true;
				float targetDist = 600f * 600f;
				Vector2 targetCenter = Projectile.position;
				int target = -1;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy()) {
						Vector2 diff = npc.Hitbox.ClosestPointInRect(Projectile.Center) - Projectile.Center;
						float dist = diff.LengthSquared();
						if (dist < targetDist) {
							targetDist = dist;
							targetCenter = npc.Center;
							target = npc.whoAmI;
						}
					}
				}
				if (target != -1) {
					float turnSpeed = 2f;
					float currentSpeed = Projectile.velocity.Length();
					Projectile.velocity = Vector2.Normalize(Projectile.velocity + (targetCenter - Projectile.Center).SafeNormalize(default) * turnSpeed) * currentSpeed;
				}
			}
		}
	}
}
