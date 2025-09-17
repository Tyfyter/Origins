using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class True_Light_Disc : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Boomerang"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LightDisc);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 70;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 13;
			Item.useAnimation = 13;
			Item.shoot = ModContent.ProjectileType<True_Light_Disc_Thrown>();
			Item.shootSpeed = 15f;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 24)
			.AddIngredient(ItemID.LightDisc)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 8;
		}
	}
	public class True_Light_Disc_Thrown : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/True_Light_Disc";
		
		public override void SetDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 5;
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
			Projectile.CloneDefaults(ProjectileID.LightDisc);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			Projectile.aiStyle = 3;
			if (Projectile.ai[0] != 0) {
				float speed = Projectile.velocity.Length();
				if (speed > 0) {
					Vector2 dir = Projectile.DirectionTo(Main.player[Projectile.owner].MountedCenter);
					if (Vector2.Dot(dir, Projectile.velocity / speed) > 0) {
						Projectile.velocity += dir * Math.Max(24f - speed, 0) * 0.16f;
					}
				}
			}
			return true;
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
				ParticleOrchestraType.Excalibur,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			default(LightDiscDrawer).Draw(Projectile);
			return true;
		}
		public override void PostDraw(Color lightColor) {
			base.PostDraw(lightColor);
		}
	}
}
