using Microsoft.Xna.Framework;
using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Terrarang : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Boomerang
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LightDisc);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 80;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.shoot = ModContent.ProjectileType<Terrarang_Thrown>();
			Item.shootSpeed = 16f;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.BrokenHeroSword)
			.AddIngredient<True_Light_Disc>()
			.AddIngredient<True_Waning_Crescent>()
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 6;
		}
	}
	public class Terrarang_Thrown : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Terrarang";
		
		public override void SetDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 5;
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
			Projectile.CloneDefaults(ProjectileID.LightDisc);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.light = 0f;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			Projectile.aiStyle = 3;
			if (Projectile.ai[0] != 0) {
				Projectile.extraUpdates = 1;
				float speed = Projectile.velocity.Length();
				if (speed > 0) {
					Vector2 dir = Projectile.DirectionTo(Main.player[Projectile.owner].MountedCenter);
					if (Vector2.Dot(dir, Projectile.velocity / speed) > 0) {
						Projectile.velocity += dir * Math.Max(18f - speed, 0) * 0.16f;
					}
				}
			}
			return true;
		}
		public override void AI() {
			if (++Projectile.ai[2] > 6) {
				Projectile.ai[2] = 0;
				if (Projectile.owner == Main.myPlayer) Projectile.NewProjectileDirect(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Terrarang_P>(),
					Projectile.damage / 3,
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
							foundTarget = true;
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
							ModContent.ProjectileType<Terrarang_P2>(),
							projectile.damage / 2,
							projectile.knockBack / 1.5f,
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
				ParticleOrchestraType.TerraBlade,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			default(TerrarangDrawer).Draw(Projectile);
			return true;
		}
	}
	public readonly struct TerrarangDrawer {
		private static readonly VertexStrip _vertexStrip = new();
		public readonly void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["LightDisc"];
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(2f);
			miscShaderData.Apply();
			for (int i = 0; i < proj.oldPos.Length; i++) {
				Vector2 pos = proj.oldPos[i];
				if (pos != default) Lighting.AddLight(pos, StripColors(i / (float)proj.oldPos.Length).ToVector3() * 0.2f);
			}
			_vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
		private readonly Color StripColors(float progressOnStrip) {
			float num = 1f - progressOnStrip;
			Color result = Color.Lerp(new Color(0, 162, 232), new Color(34, 177, 76), num) * num;
			result.A = 0;
			return result;
		}
		private readonly float StripWidth(float progressOnStrip) => 16f;
	}
	public class Terrarang_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 25;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.ignoreWater = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Terrarang_Thrown.DoSpawnBeams(Projectile);
		}
		public override Color? GetAlpha(Color lightColor) {
			lightColor.A = 150;
			return lightColor * Math.Min(Projectile.timeLeft / 30f, 1);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParticleOrchestrator.RequestParticleSpawn(
				false,
				ParticleOrchestraType.TerraBlade,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
		}
	}
	public class Terrarang_P2 : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Terrarang_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.ignoreWater = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
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
		public override Color? GetAlpha(Color lightColor) {
			lightColor.A = 150;
			lightColor.R /= 2;
			lightColor.B /= 2;
			return lightColor * Math.Min(Projectile.timeLeft / 30f, 1);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParticleOrchestrator.RequestParticleSpawn(
				false,
				ParticleOrchestraType.TerraBlade,
				new() {
					PositionInWorld = Main.rand.NextVector2FromRectangle(target.Hitbox)
				}
			);
			Projectile.ai[0] = 1;
		}
	}
}
