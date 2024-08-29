using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Third_Eye : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"MasterAcc"
		];
		const int chargeup_time = 180;
		const int reset_time = 600;
		
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.damage = 180;
			Item.knockBack = 24;
			Item.useTime = 180;
			Item.reuseDelay = 480;
			Item.shoot = ModContent.ProjectileType<Third_Eye_Deathray>();
			Item.value = Item.sellPrice(gold: 6);
			Item.master = true;
			Item.rare = ItemRarityID.Cyan;
		}
		public static Vector2 GetEyePos(Player player) => player.MountedCenter + new Vector2(5 * player.direction, -14);
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.thirdEyeUseTime = Item.useTime;
			originPlayer.thirdEyeResetTime = Item.reuseDelay;
			ref int thirdEyeTime = ref originPlayer.thirdEyeTime;
			if (thirdEyeTime < Item.useTime) {
				const int max_dist = 2500 * 2500;
				Vector2 startPoint = GetEyePos(player);
				Vector2 targetPos = default;
				float targetWeight = 0;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy() && npc.DistanceSQ(startPoint) < max_dist) {
						float weight = npc.lifeMax * 0.01f + npc.defense * 1f + npc.damage * 1f;
						if (weight > targetWeight && npc.Center != startPoint) {
							targetWeight = weight;
							targetPos = npc.Center;
						}
					}
				}
				if (targetWeight > 0) {
					if (++thirdEyeTime >= Item.useTime) {
						float dir = Main.rand.NextFloatDirection();
						Projectile.NewProjectile(
							player.GetSource_Accessory(Item),
							startPoint,
							(targetPos - startPoint).SafeNormalize(default),
							Item.shoot,
							player.GetWeaponDamage(Item),
							player.GetWeaponKnockback(Item),
							player.whoAmI,
							dir * 0.8f + Math.Sign(dir) * 0.1f,
							-1
						);
					} else {
						Vector2 dustSpawnPos = startPoint + (Main.rand.NextFloat() * (MathF.PI * 2f)).ToRotationVector2() * new Vector2(6.75f, 14.75f);
						Dust dust = Dust.NewDustDirect(startPoint - new Vector2(4), 8, 8, DustID.Vortex, player.velocity.X / 2f, player.velocity.Y / 2f);
						dust.velocity = Vector2.Normalize(startPoint - dustSpawnPos) * 3.5f * (10f - (thirdEyeTime > (Item.useTime * 0.667f) ? 1 : 0) * 2f) / 10f;
						dust.noGravity = true;
						dust.scale = 0.8f;
						dust.customData = player;
					}
				}
				originPlayer.thirdEyeActive = true;
			}
		}
	}
	public class Third_Eye_Deathray : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PhantasmalDeathray;
		Vector2 RealVelocity => Projectile.velocity.RotatedBy(Projectile.ai[0] * Projectile.ai[1]);
		const int sample_points = 3;
		
		public override void SetDefaults() {
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 6;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.hide = true;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.direction = Math.Sign(Projectile.velocity.X);
			Projectile.Center = Third_Eye.GetEyePos(player);
			Vector2 unit = RealVelocity;
			float[] laserScanResults = new float[sample_points];
			Collision.LaserScan(Projectile.Center, unit.SafeNormalize(default), 4 * Projectile.scale, 1200f, laserScanResults);
			Projectile.localAI[1] = laserScanResults.Average();
			if (Projectile.ai[1] < 1) {
				Projectile.ai[1] += 0.05f;
			} else {
				Projectile.Kill();
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 unit = RealVelocity;
			float point = 0f;
			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(), targetHitbox.Size(),
				Projectile.Center, Projectile.Center + unit * Projectile.localAI[1], 12,
			ref point);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.velocity == Vector2.Zero) return false;
			Vector2 velocity = RealVelocity;
			float rotation = velocity.ToRotation() - MathHelper.PiOver2;
			float scale = Projectile.scale * 0.333f;

			Texture2D beamStartTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D beamMiddleTexture = TextureAssets.Extra[21].Value;
			Texture2D beamEndTexture = TextureAssets.Extra[22].Value;
			float lengthLeft = Projectile.localAI[1];
			Color color = new Color(255, 255, 255, 0) * 0.9f;
			Main.EntitySpriteDraw(beamStartTexture, Projectile.Center - Main.screenPosition, null, color, rotation, beamStartTexture.Size() / 2f, scale, SpriteEffects.None, 0);
			lengthLeft -= (beamStartTexture.Height / 2 + beamEndTexture.Height) * scale;
			Vector2 drawPos = Projectile.Center;
			drawPos += velocity * scale * beamStartTexture.Height / 2f;
			if (lengthLeft > 0f) {
				float framePos = 0f;
				Rectangle frame = new Rectangle(0, 16 * (Projectile.timeLeft / 3 % 5), beamMiddleTexture.Width, 16);
				while (framePos + 1f < lengthLeft) {
					if (lengthLeft - framePos < frame.Height)
						frame.Height = (int)(lengthLeft - framePos);

					Main.EntitySpriteDraw(beamMiddleTexture, drawPos - Main.screenPosition, frame, color, rotation, new Vector2(frame.Width / 2, 0f), scale, SpriteEffects.None, 0);
					framePos += frame.Height * scale;
					drawPos += velocity * frame.Height * scale;
					frame.Y += 16;
					if (frame.Y + frame.Height > beamMiddleTexture.Height)
						frame.Y = 0;
				}
			}

			Main.EntitySpriteDraw(beamEndTexture, drawPos - Main.screenPosition, null, color, rotation, beamEndTexture.Frame().Top(), scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
