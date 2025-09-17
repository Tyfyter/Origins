using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Waning_Crescent : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Boomerang"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 40;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.shoot = ModContent.ProjectileType<Waning_Crescent_Thrown>();
			Item.shootSpeed = 11.75f;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddRecipeGroup(OriginSystem.EvilBoomerangRecipeGroupID)
			.AddIngredient(ItemID.Flamarang)
			.AddIngredient(ItemID.ThornChakram)
			.AddIngredient(ItemID.Trimarang)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Waning_Crescent_Thrown : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Waning_Crescent";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			Projectile.aiStyle = 3;
			return true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0 && ++Projectile.ai[2] > 4) {
				Projectile.ai[2] = 0;
				if (Projectile.owner == Main.myPlayer) Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Waning_Crescent_P>(),
					Projectile.damage / 2,
					Projectile.knockBack / 3,
					Projectile.owner
				);
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
	public class Waning_Crescent_P : ModProjectile {
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
		public override Color? GetAlpha(Color lightColor) {
			lightColor.A = 200;
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
}
