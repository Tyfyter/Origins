using Origins.Tiles.Other;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Eye_Of_Aether : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 15;
		}
		public override void SetDefaults() {
			Item.DefaultToThrownWeapon(ModContent.ProjectileType<Eye_Of_Aether_P>(), 20, 4);
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.DamageType = DamageClass.Default;
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Blue;
			Item.noUseGraphic = true;
		}
		public override void UseItemFrame(Player player) {
			if (player.itemAnimation / (float)player.itemAnimationMax < 0.85f) player.bodyFrame.Y = player.bodyFrame.Height * 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ItemID.Lens, 2)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 5)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = new Vector2(player.direction, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			position += velocity * 12;
			velocity = new Vector2(0, -16);
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback
			);
			return false;
		}
	}
	public class Eye_Of_Aether_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 20;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			const int fall = 40;
			const int float_up = fall + 30;
			const int float_in_place = float_up + 60;
			const int shoot = float_in_place + 30;
			float spin_speed = (Projectile.ai[0] - float_up) * 0.01f;
			Projectile.ai[0]++;
			if (Projectile.ai[0] < fall) {
				Projectile.velocity *= 0.97f;
				Projectile.velocity.Y += 0.4f;
			} else if (Projectile.ai[0] < float_up) {
				Projectile.velocity *= 0.93f;
				Projectile.velocity.Y -= 0.2f;
			} else if (Projectile.ai[1] == 0) {
				if (ModContent.GetInstance<OriginSystem>().shimmerPosition is Vector2 shimmerPosition) {
					SoundEngine.PlaySound(SoundID.Item15.WithPitch(-1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item15.WithPitch(0).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					SoundEngine.PlaySound(Origins.Sounds.PowerUp.WithPitch(-1), Projectile.Center);
					Projectile.ai[1] = shimmerPosition.X * 16;
					Projectile.ai[2] = shimmerPosition.Y * 16;
				} else {
					SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
					// could use some dusts or gores here
					Projectile.Kill();
				}
			} else if (Projectile.ai[0] < shoot) {
				if (Projectile.ai[0] < float_in_place) Projectile.velocity *= 0.93f;
				float targetAngle = (new Vector2(Projectile.ai[1], Projectile.ai[2]) - Projectile.Center).ToRotation() + MathHelper.PiOver4 * 0.65f;
				if (GeometryUtils.AngularSmoothing(ref Projectile.rotation, targetAngle, spin_speed)) {
					Projectile.rotation = targetAngle;
					if (Projectile.ai[0] % 13f < 1) {
						for (int i = 0; i < 3; i++) {
							Vector2 direction = (Main.rand.NextFloat(-1f, 1f) + targetAngle - MathHelper.PiOver4 * 0.65f).ToRotationVector2();
							ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
								PositionInWorld = Projectile.Center + direction * 64,
								MovementVector = direction * -4,
								UniqueInfoPiece = (byte)(0.727f * 255f)
							});
						}
					}
				}
			} else if (Projectile.timeLeft > 30) {
				Vector2 diff = new Vector2(Projectile.ai[1], Projectile.ai[2]) - Projectile.Center;
				Vector2 pos = Projectile.Center;
				float dist = diff.Length();
				const float speed = 8;
				Vector2 dir = diff * speed / dist;
				Rectangle area = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
				area.Inflate(16 * 50, 16 * 50);
				while (dist > 0) {
					if (area.Contains(pos)) {
						ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings {
							PositionInWorld = pos,
							MovementVector = dir,
							UniqueInfoPiece = (byte)(0.727f * 255f)
						});
					}
					pos += dir;
					dist -= speed;
				}
				Projectile.timeLeft = 30;
			}
			if (Projectile.timeLeft < 30) {
				bool aprilFools = OriginsModIntegrations.CheckAprilFools();
				Projectile.Opacity = Projectile.timeLeft / (aprilFools ? 15f : 30f);
				if (Projectile.timeLeft > 15) {
					SoundEngine.PlaySound(SoundID.Item15.WithPitch(-1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
				} else {
					if (aprilFools && Projectile.localAI[0] == 0) {
						Projectile.timeLeft = 15;
						Projectile.hostile = true;
						Projectile.damage = 3;
					}
					Projectile.tileCollide = true;
					Projectile.velocity *= 0.97f;
					Projectile.velocity.Y += 0.4f;
				}
			} else {
				Lighting.AddLight(Projectile.Center, 0.369f, 0.012f, 0.988f);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.localAI[0] = 1;
			return false;
		}
	}
}
