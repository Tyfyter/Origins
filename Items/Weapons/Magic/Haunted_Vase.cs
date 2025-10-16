using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

using Origins.Dev;
using PegasusLib;
namespace Origins.Items.Weapons.Magic {
	public class Haunted_Vase : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            WikiCategories.OtherMagic
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ToxicFlask);
			Item.damage = 57;
			Item.crit = 5;
			Item.knockBack *= 0.5f;
			Item.useAnimation = Item.useTime = 27;
			Item.width = 50;
			Item.height = 42;
			Item.glowMask = glowmask;
			Item.shoot = ModContent.ProjectileType<Haunted_Vase_P>();
			Item.shootSpeed *= 1.5f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;

		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: velocity.Length() * 0.3f);
			return false;
		}
	}
	public class Haunted_Vase_P : ModProjectile {
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ToxicFlask);
			Projectile.width = 32;
			Projectile.height = 32;
		}
		public override void AI() {
			Projectile.rotation -= (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.03f * Projectile.direction;
			Projectile.rotation += Math.Abs(Projectile.velocity.X) * 0.04f * Projectile.direction;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item107, Projectile.position);
			Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, -Projectile.oldVelocity * 0.2f, 704);
			Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, -Projectile.oldVelocity * 0.2f, 705);
			if (Projectile.owner == Main.myPlayer) {
				int count = Main.rand.Next(9, 12);
				int type = ModContent.ProjectileType<Haunted_Vase_Wisp>();
				for (int i = 0; i < count; i++) {
					Vector2 velocity = Main.rand.NextVector2Circular(1, 1);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(64, 64), velocity, type, Projectile.damage, 1f, Projectile.owner, ai1: Projectile.ai[1] * Main.rand.NextFloat(0.85f, 1.15f));
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor.MultiplyRGBA(new Color(255, 255, 255, 200)),
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
				0);
			Dust dust = Dust.NewDustDirect(Projectile.Center + (new Vector2(14, -14).RotatedBy(Projectile.rotation) * Projectile.scale) - new Vector2(4, 4), 0, 0, DustID.Shadowflame, 0f, -2f, Scale: (1.25f + (float)Math.Sin(Projectile.timeLeft) * 0.25f));
			dust.noGravity = true;
			dust.velocity = Vector2.Zero;
			dust.fadeIn = 0.5f;
			dust.alpha = 200;
			return false;
		}
	}
	public class Haunted_Vase_Wisp : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Haunted_Vase";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpiritFlame);
			Projectile.aiStyle = 0;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 120;
			Projectile.localAI[1] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			if (Projectile.localAI.Length < 3) Projectile.localAI = Projectile.localAI.WithLength(3);
			Projectile.localAI[2] = Main.rand.Next(30);
		}
		public override void AI() {
			if (Projectile.localAI[2] > 0f) {
				Projectile.localAI[2]--;
				Projectile.timeLeft--;
			}
			if (Projectile.localAI[0] > 0f) {
				Projectile.localAI[0]--;
			}
			if (Projectile.localAI[0] == 0f && Projectile.owner == Main.myPlayer) {
				Projectile.localAI[0] = 5f;
				float currentTargetDist = Projectile.ai[0] > 0 ? Main.npc[(int)Projectile.ai[0] - 1].Distance(Projectile.Center) : 0;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC targetOption = Main.npc[i];
					if (targetOption.CanBeChasedBy()) {
						float newTargetDist = targetOption.Distance(Projectile.Center);
						bool selectNew = Projectile.ai[0] <= 0f || currentTargetDist > newTargetDist;
						if (selectNew && (newTargetDist < 240f)) {
							Projectile.ai[0] = i + 1;
							currentTargetDist = newTargetDist;
						}
					}
				}
				if (Projectile.ai[0] > 0f) {
					Projectile.timeLeft = 300 - Main.rand.Next(120);
					Projectile.netUpdate = true;
				}
			}
			float scaleFactor = MathHelper.Clamp((30 - Projectile.localAI[2]) * 0.04f, 0.1f, 1f);
			Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Shadowflame, 0f, -2f);
			dust.noGravity = true;
			dust.velocity = Projectile.oldVelocity * 0.5f;
			dust.scale = scaleFactor;
			dust.fadeIn = 0.5f;
			dust.alpha = 200;

			int target = (int)Projectile.ai[0] - 1;
			if (target >= 0) {
				if (Main.npc[target].active) {
					if (Projectile.Distance(Main.npc[target].Center) > 1f) {
						Vector2 dir = Projectile.DirectionTo(Main.npc[target].Center);
						if (dir.HasNaNs()) {
							dir = Vector2.UnitY;
						}
						float angle = dir.ToRotation();
						PolarVec2 velocity = (PolarVec2)Projectile.velocity;
						float targetVel = Projectile.ai[1];
						bool changed = false;
						if (velocity.R != targetVel) {
							OriginExtensions.LinearSmoothing(ref velocity.R, targetVel, (targetVel - 0.5f) * 0.1f);
							changed = true;
						}
						if (velocity.Theta != angle) {
							OriginExtensions.AngularSmoothing(ref velocity.Theta, angle, 0.1f);
							changed = true;
						}
						if (changed) {
							Projectile.velocity = (Vector2)velocity;
						}
					}
					return;
				}
				Projectile.ai[0] = 0f;
				Projectile.netUpdate = true;
			} else {
				PolarVec2 velocity = (PolarVec2)Projectile.velocity;
				float targetVel = Projectile.ai[1];
				bool changed = false;
				if (velocity.R != targetVel) {
					OriginExtensions.LinearSmoothing(ref velocity.R, targetVel / 3f, (targetVel - 0.5f) * 0.1f);
					changed = true;
				}

				if (velocity.Theta != Projectile.localAI[1]) {
					OriginExtensions.AngularSmoothing(ref velocity.Theta, Projectile.localAI[1], (targetVel - 0.5f) * 0.03f);
					changed = true;
				} else {
					Projectile.localAI[1] = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
				}

				if (changed) {
					Projectile.velocity = (Vector2)velocity;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
}
