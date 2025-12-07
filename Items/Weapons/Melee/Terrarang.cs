using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Terrarang : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LightDisc);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 25;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.shoot = ModContent.ProjectileType<Terrarang_Thrown>();
			Item.shootSpeed = 17f;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 15;
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
	public class Terrarang_Thrown : ModProjectile, IOutlineDrawer {
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
			Projectile.aiStyle = -1;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 3;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.rotation += 0.4f;
			Projectile.localAI[1] -= 0.25f;
			if (Projectile.ai[0] != 0)
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity.ToRotation().AngleTowards(Projectile.Center.DirectionTo(Main.MouseWorld).ToRotation(),0.1f).ToRotationVector2() * 16, player.DirectionFrom(Projectile.Center) * 12f, MathHelper.Clamp(MathHelper.Lerp(0, 0.1f, MathF.Pow(2, 10 * (Projectile.ai[0] / 36) - 10)), 0, 1));
			}
			else
				Projectile.velocity = Projectile.velocity.RotatedByRandom(0.2f);

			

			if (player.Distance(Projectile.Center) < 32 && Projectile.ai[0] > 30)
				Projectile.Kill();

			Projectile.ai[0]++;
		}
		private static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness) {
			Texture2D sparkleTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			Color bigColor = shineColor * opacity * 0.5f;
			bigColor.A = 0;
			Vector2 origin = sparkleTexture.Size() / 2f;
			Color smallColor = drawColor * 0.5f;
			float lerpValue = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
			Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X) * lerpValue;
			Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y) * lerpValue;
			bigColor *= lerpValue;
			smallColor *= lerpValue;
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			projHitbox.Width = 64;
			projHitbox.Height = 64;
			return projHitbox.Intersects(targetHitbox);
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

			return null;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[0] < 34) 
			{
				if (Projectile.velocity.X != oldVelocity.X) {
					Projectile.velocity.X = -oldVelocity.X;
				}
				if (Projectile.velocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
				SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			}
			Projectile.tileCollide = false;
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
			for (int trail = 0; trail < 3; trail++) {
				Main.EntitySpriteDraw(TextureAssets.Projectile[ProjectileID.Excalibur].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[ProjectileID.Excalibur].Value.Frame(1, 4), Color.Lerp(Color.SpringGreen * (1f - trail / 4f), Color.Goldenrod * (1f - trail / 4f), trail / 4f), Projectile.localAI[1] * 2 + MathHelper.ToRadians(trail * 55), TextureAssets.Projectile[ProjectileID.Excalibur].Value.Frame(1, 4).Size() / 2f, 0.4f, Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
				Main.EntitySpriteDraw(TextureAssets.Projectile[ProjectileID.Excalibur].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[ProjectileID.Excalibur].Value.Frame(1, 4, 0, 2), Color.Lerp(Color.Yellow * (1f - trail / 4f), Color.SpringGreen * (1f - trail / 4f), trail / 4f), Projectile.localAI[1] * 2 + MathHelper.ToRadians(trail * 55), TextureAssets.Projectile[ProjectileID.Excalibur].Value.Frame(1, 4, 4).Size() / 2f, 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
				Main.EntitySpriteDraw(TextureAssets.Projectile[ProjectileID.Excalibur].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[ProjectileID.Excalibur].Value.Frame(1, 4, 0, 3), Color.White * (1f - trail / 4f), Projectile.localAI[1] * 2 + MathHelper.ToRadians(trail * 55), TextureAssets.Projectile[ProjectileID.Excalibur].Value.Frame(1, 4, 4).Size() / 2f, 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally);
				DrawPrettyStarSparkle(1f, SpriteEffects.None, Projectile.Center - Main.screenPosition, Color.SpringGreen, Color.Goldenrod, 1f, 0f, 1f, 2f, 3f, 0f, Vector2.One * 2f, Vector2.One);

			}
			this.DrawOutline();
			return true;
		}

		public Color? SetOutlineColor(float progress) {
			return Color.Lerp(Color.SpringGreen,Color.Goldenrod,0.1f);
		}

		public DrawData[] OutlineDrawDatas => [new DrawData(TextureAssets.Projectile[Type].Value, Projectile.Center, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2f,1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None)];
		public int OutlineSteps => 8;
		public float OutlineOffset => 2;
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
			Color result = Color.Lerp(Color.SpringGreen, Color.Green, num) * num;
			result.A = 0;
			return result;
		}
		private readonly float StripWidth(float progressOnStrip) => 64f;
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
			lightColor.A = 255;
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
