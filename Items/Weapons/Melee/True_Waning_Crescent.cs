using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class True_Waning_Crescent : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 73;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.shoot = ModContent.ProjectileType<True_Waning_Crescent_Thrown>();
			Item.shootSpeed = 15f;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofFright, 20)
			.AddIngredient(ItemID.SoulofMight, 20)
			.AddIngredient(ItemID.SoulofSight, 20)
			.AddIngredient<Waning_Crescent>()
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class True_Waning_Crescent_Thrown : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/True_Waning_Crescent";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			Projectile.aiStyle = ProjAIStyleID.Boomerang;
			return true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0 && ++Projectile.ai[2] > 4) {
				Projectile.ai[2] = 0;
				if (Projectile.owner == Main.myPlayer) Projectile.NewProjectileDirect(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<True_Waning_Crescent_P>(),
					Projectile.damage / 2,
					Projectile.knockBack / 3,
					Projectile.owner
				).localAI[2] = 20;
			}
			DoSpawnBeams(Projectile);
		}
		public static void DoSpawnBeams(Projectile projectile) {
			if (projectile.owner == Main.myPlayer) {
				if (projectile.localAI[2] <= 0) {
					float dist = 16 * 7;
					dist *= dist;
					Vector2 targetPos = default;
					bool foundTarget = Main.player[projectile.owner].DoHoming((target) => {
						float newDist = projectile.DistanceSQ(target.Center);
						if (newDist < dist) {
							dist = newDist;
							targetPos = target.Center;
							return true;
						}
						return false;
					});
					if (foundTarget) {
						Projectile.NewProjectile(
							projectile.GetSource_FromAI(),
							projectile.Center,
							projectile.DirectionTo(targetPos) * 10f,
							ModContent.ProjectileType<True_Waning_Crescent_P2>(),
							projectile.damage,
							projectile.knockBack,
							projectile.owner
						);
						projectile.localAI[2] = 20;
					}
				} else {
					projectile.localAI[2]--;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = 0;
			return null;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParticleOrchestrator.RequestParticleSpawn(
				false,
				ParticleOrchestraType.NightsEdge,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
		}
	}
	public class True_Waning_Crescent_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.ignoreWater = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			True_Waning_Crescent_Thrown.DoSpawnBeams(Projectile);
		}
		public override Color? GetAlpha(Color lightColor) {
			lightColor.A = 150;
			return lightColor * Math.Min(Projectile.timeLeft / 30f, 1);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParticleOrchestrator.RequestParticleSpawn(
				false,
				ParticleOrchestraType.NightsEdge,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
		}
	}
	public class True_Waning_Crescent_P2 : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			if (Projectile.ai[0] == 0) Projectile.aiStyle = ProjAIStyleID.Boomerang;
			return true;
		}
		public override void AI() {
			if (Projectile.aiStyle == 0) {
				Projectile.rotation += 0.4f * Projectile.direction;
				Projectile.alpha += 5;
				if (Projectile.alpha >= 255) Projectile.Kill();
			} else if (Projectile.ai[0] != 0) {
				Projectile.aiStyle = 0;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = 0;
			return null;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override Color? GetAlpha(Color lightColor) {
			const float color = 0.8f;
			const float alpha = 0.8f;
			return new Color(color, color, color, alpha) * (1 - Projectile.alpha / 255f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParticleOrchestrator.RequestParticleSpawn(
				false,
				ParticleOrchestraType.NightsEdge,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
			Projectile.ai[0] = 1;
		}
	}
}
