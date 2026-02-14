using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Tools;
using Origins.Items.Weapons.Summoner.Minions;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Huff_Puffer_Bait : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToThrownWeapon(ModContent.ProjectileType<Huff_Puffer>(), 20, 8);
			Item.noUseGraphic = true;
			Item.DamageType = DamageClass.Summon;
			Item.damage = 47;
			Item.knockBack = 0.2f;
			Item.rare = ItemRarityID.LightRed;
			Item.bait = 193;
			Item.consumable = false;
			Item.sentry = true;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override bool? CanConsumeBait(Player player) => false;
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				Projectile.NewProjectile(source, position, velocity, type, Item.damage, Item.knockBack, player.whoAmI);
				player.UpdateMaxTurrets();
			}
			return false;
		}
	}
	public class Huff_Puffer_Bait_Player : ModPlayer {
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			if (attempt.playerFishingConditions.Bait.ModItem is Huff_Puffer_Bait) {
				itemDrop = attempt.playerFishingConditions.BaitItemType;
				sonar.Text = Language.GetOrRegister("Mods.Origins.Projectiles.Huff_Puffer.DisplayName").Value;
				sonar.Color = Color.White;
				sonar.DurationInFrames = 60;
				sonar.Velocity.Y = -7;
			}
		}
	}
	public class Huff_Puffer_Bait_Global_Projectile : GlobalProjectile {
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.bobber;
		public override void PostAI(Projectile projectile) {
			if (projectile.ai[1] > 0) {
				Player player = Main.player[projectile.owner];
				Item item = player.GetFishingConditions().Bait;
				if (item.ModItem is not Huff_Puffer_Bait) return;
				projectile.ai[1] = ItemID.None;
				projectile.ai[0] = 2;
				Projectile.NewProjectile(
					player.GetSource_ItemUse(item),
					projectile.Center,
					Vector2.Zero,
					item.shoot,
					(int)(player.GetWeaponDamage(item) * 1.2f),
					player.GetWeaponKnockback(item) * 1.5f
				);
				player.UpdateMaxTurrets();
			}
		}
	}
	namespace Minions {
		public class Huff_Puffer : ModProjectile {
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 5;
				Hydrolantern_Force_Global.ProjectileTypes.Add(Type);
				OriginsSets.Projectiles.Apostasy_AnimalMinions[Type] = true;
			}
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Summon;
				Projectile.width = 16;
				Projectile.height = 16;
				Projectile.tileCollide = true;
				Projectile.friendly = true;
				Projectile.sentry = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = Projectile.SentryLifeTime;
				Projectile.aiStyle = 0;
				Projectile.netImportant = true;
			}
			float rotationSpeed = 0;
			float Fade => Math.Min(Projectile.timeLeft / 52f, 1);
			bool Dead => Projectile.ai[0] >= 120;
			public override bool? CanCutTiles() => false;
			public override void AI() {
				Projectile.velocity *= Projectile.wet ? 0.96f : 0.99f;
				if (Projectile.wet) {
					float waveFactor = MathF.Sin(++Projectile.localAI[1] * 0.01f);
					Projectile.velocity.Y += waveFactor * 0.01f;
					Projectile.rotation += rotationSpeed;
					Projectile.rotation = MathF.Atan2(MathF.Sin(Projectile.rotation), MathF.Cos(Projectile.rotation));
					rotationSpeed -= Projectile.rotation * -0.001f * Dead.ToDirectionInt();
					rotationSpeed *= 0.99f;
					rotationSpeed += Projectile.velocity.X * 0.0002f;
					if (!Dead) {
						if (Projectile.ai[0] > 0) Projectile.ai[0]--;
						if (++Projectile.frameCounter > 6) {
							Projectile.frameCounter = 0;
							if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = Main.projFrames[Type] - 2;
							Projectile.localAI[0]++;
							if (Projectile.localAI[0] < ProjectileID.ToxicCloud) Projectile.localAI[0] = ProjectileID.ToxicCloud;
							if (Projectile.localAI[0] > ProjectileID.ToxicCloud3) Projectile.localAI[0] = ProjectileID.ToxicCloud;
						}
					}
					if (Math.Abs(Projectile.velocity.X) > 2) Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
				} else {
					Projectile.velocity.Y += 0.12f;
					Projectile.rotation += rotationSpeed;
					rotationSpeed *= 0.99f;
					rotationSpeed += Projectile.velocity.X * 0.0003f;
					if (!Dead) {
						Projectile.ai[0]++;
					} else {
						if (Projectile.timeLeft > 60) Projectile.timeLeft = 60;
					}
				}

				const int HalfSpriteWidth = 30 / 2;
				const int HalfSpriteHeight = 30 / 2;

				int HalfProjWidth = Projectile.width / 2;
				int HalfProjHeight = Projectile.height / 2;
				DrawOriginOffsetX = 0;
				DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
				DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			}
			public override void ModifyDamageHitbox(ref Rectangle hitbox) {
				if (Projectile.frame >= Main.projFrames[Type] - 2) {
					hitbox.Inflate(32, 32);
				} else {
					hitbox.Inflate(-8, -8);
				}
			}
			public override Color? GetAlpha(Color lightColor) => lightColor * Fade;
			public override bool OnTileCollide(Vector2 oldVelocity) {
				const float bounce = 0.4f;
				const float friction = 0.9f;
				float spin = 0;
				Vector2 newOldVelocity = Projectile.velocity;
				if (newOldVelocity.X != oldVelocity.X) {
					Projectile.velocity.X = oldVelocity.X * -bounce;
					spin -= (Projectile.velocity.Y - Projectile.velocity.Y * friction) * Math.Sign(oldVelocity.X);
					Projectile.velocity.Y = Projectile.velocity.Y * friction;
				}
				if (newOldVelocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = oldVelocity.Y * -bounce;
					spin += (Projectile.velocity.X - Projectile.velocity.X * friction) * Math.Sign(oldVelocity.Y);
					Projectile.velocity.X = Projectile.velocity.X * friction;
				}
				spin *= 0.5f;
				if (Math.Abs(spin) > 0.01f) {
					MathUtils.LinearSmoothing(ref rotationSpeed, spin, 0.2f);
				} else {
					float lowestDiff = MathHelper.TwoPi;
					float lowest = 0;
					for (int i = 0; i < 2; i++) {
						float current = GeometryUtils.AngleDif(Projectile.rotation, MathHelper.Pi * i, out _);
						if (current < lowestDiff) {
							lowestDiff = current;
							lowest = MathHelper.Pi * i;
						}
					}
					Vector2 mov = Vector2.Zero;
					mov.X = Math.Min(GeometryUtils.AngleDif(Projectile.rotation, lowest, out int dir), 1f) * dir * 2;
					Vector4 slopeCollision = Collision.SlopeCollision(Projectile.position, mov, Projectile.width, Projectile.height);
					Projectile.position = slopeCollision.XY();
					mov = slopeCollision.ZW();
					mov = Collision.TileCollision(Projectile.position, mov, Projectile.width, Projectile.height);
					Projectile.position += mov;
					rotationSpeed = mov.X * 0.05f;
				}
				return false;
			}
			public override void PostDraw(Color lightColor) {
				int cloudType = (int)Projectile.localAI[0];
				if (Projectile.localAI[0] == 0 || Projectile.frame < Main.projFrames[Type] - 2) return;
				Main.instance.LoadProjectile(cloudType);
				Texture2D texture = TextureAssets.Projectile[cloudType].Value;
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center - Main.screenPosition,
					null,
					lightColor * 0.5f,
					0,
					texture.Size() * 0.5f,
					2,
					SpriteEffects.None
				);
			}
		}
	}
}
