using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Other.Dyes;
using Origins.Tiles.Other;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Shimmer_Rod : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"OtherMagic"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.NimbusRod);
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shoot = ModContent.ProjectileType<Shimmer_Cloud_Held>();
			Item.channel = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.NimbusRod)
			.AddIngredient(ModContent.ItemType<Aetherite_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) mult = 0;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				OriginExtensions.FadeOutOldProjectilesAtLimit([ModContent.ProjectileType<Shimmer_Cloud_P>(), ModContent.ProjectileType<Shimmer_Cloud_Ball>()], 1, 52);
				return false;
			}
			return true;
		}
		public override void Load() {
			if (Main.dedServ) return;
			On_Main.DrawProjectiles += (orig, self) => {
				orig(self);
				if (Main.dedServ) return;
				if (cachedRain.Count == 0 && cachedClouds.Count == 0) return;
				try {
					GraphicsUtils.drawingEffect = true;
					Origins.shaderOroboros.Capture();
					while (cachedRain.Count != 0) {
						self.DrawProj(cachedRain.Pop());
					}
					while (cachedClouds.Count != 0) {
						self.DrawProj(cachedClouds.Pop());
					}
					ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(Shimmer_Dye.ShaderID, null);
					Origins.shaderOroboros.Stack(shader);
					Origins.shaderOroboros.Release();
				} finally {
					GraphicsUtils.drawingEffect = false;
				}
			};
		}
		internal static Stack<int> cachedClouds = [];
		internal static Stack<int> cachedRain = [];
	}
	public class Shimmer_Cloud_Held : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RainCloudMoving);
			Projectile.aiStyle = 0;
			Projectile.timeLeft = Projectile.timeLeft;
			Projectile.tileCollide = false;
		}
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set {
				Projectile.ai[0] = value.X;
				Projectile.ai[1] = value.Y;
			}
		}
		public float TargetRot {
			get {
				Vector2 diff = Main.MouseWorld - TargetPos;
				return diff == default ? 0 : diff.ToRotation() - MathHelper.PiOver2;
			}
		}
		public override void OnSpawn(IEntitySource source) {
			TargetPos = Main.MouseWorld;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			owner.heldProj = Projectile.whoAmI;
			if (Projectile.ai[2] == 0) {
				owner.itemTime = owner.itemTimeMax;
				owner.itemAnimation = owner.itemAnimationMax;
				Projectile.Center = owner.itemLocation + new Vector2(24 * owner.direction, -24 * owner.gravDir).RotatedBy(owner.itemRotation);
				if (!owner.channel && Main.myPlayer == Projectile.owner) {
					Projectile.ai[2] = 1;
					Projectile.localAI[0] = TargetRot;
					Projectile.netUpdate = true;
				}
			} else {
				Vector2 tan = new Vector2(24 * owner.direction, -24 * owner.gravDir).RotatedBy(owner.itemRotation);
				Projectile.Center = owner.itemLocation + tan;
				if (Main.myPlayer == Projectile.owner) {
					Vector2 diff = (TargetPos - Projectile.Center).SafeNormalize(Projectile.velocity);
					Vector2 normTan = new Vector2(-tan.Y * owner.direction, tan.X * owner.direction).SafeNormalize(Projectile.velocity);
					//Dust.NewDustPerfect(Projectile.Center, 6, diff * 8).noGravity = true;
					//Dust.NewDustPerfect(Projectile.Center, 6, normTan * 8).noGravity = true;
					float dot = 1 - Vector2.Dot(diff, normTan);
					float change = Projectile.localAI[1] - dot;
					if (Projectile.localAI[2] == 1 && (dot == 0 || change < 0) && !CollisionExtensions.OverlapsAnyTiles(Projectile.Hitbox)) {
						//Dust.NewDustPerfect(Projectile.Center, 23, normTan).noGravity = true;
						Projectile.NewProjectile(
							owner.GetSource_ItemUse(owner.HeldItem),
							Projectile.Center,
							diff * Projectile.velocity.Length(),
							ModContent.ProjectileType<Shimmer_Cloud_Ball>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner,
							Projectile.ai[0],
							Projectile.ai[1],
							Projectile.localAI[0]
						);
						Projectile.Kill();
					}
					if (owner.ItemAnimationEndingOrEnded) Projectile.Kill();
					Projectile.localAI[1] = dot;
					Projectile.localAI[2] = 1;
				}
			}
			Projectile.frameCounter++;
			if (Projectile.frameCounter > 4) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame > 3)
					Projectile.frame = 0;
			}
		}
		readonly Vector2[] raindicators = new Vector2[4];
		int raindicatorTimer = 0;
		public override bool PreDraw(ref Color lightColor) {
			if (!GraphicsUtils.drawingEffect) {
				Shimmer_Rod.cachedClouds.Push(Projectile.whoAmI);
				return false;
			}
			if (Main.myPlayer == Projectile.owner && Projectile.ai[2] == 0) {
				if (++raindicatorTimer % 15 == 1) {
					if (raindicatorTimer > 60) raindicatorTimer -= 60;
					raindicators[raindicatorTimer / 15] = new(Main.rand.Next(-10, 11), 32);
				}
				Color raindicatorColor = new Color(1f, 0, 0.08f, 0.3f) * 0.2f;
				float rot = TargetRot;
				for (int i = 0; i < raindicators.Length; i++) {
					if (raindicators[i].Y > 32 + 100) continue;
					Main.spriteBatch.Draw(
						TextureAssets.Projectile[Shimmer_Cloud_Rain.ID].Value,
						TargetPos - Main.screenPosition + raindicators[i].RotatedBy(rot),
						null,
						raindicatorColor,
						rot,
						new(1, 39),
						1,
						SpriteEffects.None,
					0);
					raindicators[i].Y += 4;
				}
			}
			lightColor = Color.LightGray;
			return true;
		}
		public override void PostDraw(Color lightColor) {
			if (!GraphicsUtils.drawingEffect) return;
			Rectangle frame = TextureAssets.Projectile[Type].Value.Frame(verticalFrames: 4);
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[Type].Value,
				TargetPos - Main.screenPosition,
				frame,
				Color.White * 0.4f,
				TargetRot,
				frame.Size() * 0.5f,
				1,
				SpriteEffects.None,
			0);
		}
		public override bool ShouldUpdatePosition() => false;
	}
	public class Shimmer_Cloud_Ball : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RainCloudMoving);
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 18000;
		}
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set {
				Projectile.ai[0] = value.X;
				Projectile.ai[1] = value.Y;
			}
		}
		public override void AI() {
			if (TargetPos != default) {
				Vector2 combined = Projectile.velocity * (TargetPos - Projectile.Center);
				if (Projectile.owner == Main.myPlayer && combined.X <= 0 && combined.Y <= 0) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						TargetPos,
						default,
						ModContent.ProjectileType<Shimmer_Cloud_P>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						Projectile.ai[2]
					);
					Projectile.Kill();
					OriginExtensions.FadeOutOldProjectilesAtLimit([ModContent.ProjectileType<Shimmer_Cloud_P>(), ModContent.ProjectileType<Shimmer_Cloud_Ball>()], 3, 52);
				}
			}
			float inShimmer = Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && Collision.shimmer ? 1 : 0;
			if (Projectile.localAI[0] != inShimmer) {
				Projectile.localAI[0] = inShimmer;
				if (inShimmer == 1) {
					Projectile.velocity.Y = -Projectile.velocity.Y;
					Projectile.ai[1] += (Projectile.Center.Y - Projectile.ai[1]) * 2;
					const float mirrorAngle = MathHelper.PiOver2;
					Projectile.ai[2] = mirrorAngle - GeometryUtils.AngleDif(mirrorAngle, Projectile.ai[2], out int dir) * dir;
				}
			}

			Projectile.rotation += Projectile.velocity.X * 0.02f;
			Projectile.frameCounter++;
			if (Projectile.frameCounter > 4) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame > 3)
					Projectile.frame = 0;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!GraphicsUtils.drawingEffect) {
				Shimmer_Rod.cachedClouds.Push(Projectile.whoAmI);
				return false;
			}
			lightColor = Color.LightGray;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			TargetPos = Projectile.Center;
			Projectile.velocity = default;
			return false;
		}
	}
	public class Shimmer_Cloud_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 6;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RainCloudRaining);
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 18000;
			Projectile.width = 28;
			Projectile.height = 28;
		}
		public override void AI() {
			Projectile.rotation = Projectile.ai[0];
			if (++Projectile.ai[1] % 10f < 1) {
				Vector2 unit = new Vector2(0, 1).RotatedBy(Projectile.rotation);
				Vector2 perp = new(unit.Y, -unit.X);
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center + unit * 24 + perp * Main.rand.Next(-14, 15),
					unit * 5,
					Shimmer_Cloud_Rain.ID,
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner,
					Projectile.ai[2]
				);
			}
			Projectile.frameCounter++;
			if (Projectile.frameCounter > 4) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame > 5)
					Projectile.frame = 0;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!GraphicsUtils.drawingEffect) {
				Shimmer_Rod.cachedClouds.Push(Projectile.whoAmI);
				return false;
			}
			lightColor = Color.LightGray;
			Rectangle frame = TextureAssets.Projectile[Type].Value.Frame(verticalFrames: 6, frameY: Projectile.frame);
			float timeFactor = Math.Min(Projectile.timeLeft / 52f, 1);
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor * timeFactor,
				Projectile.rotation,
				frame.Size() * 0.5f,
				Projectile.scale,
				0,
			0);
			return false;
		}
	}
	public class Shimmer_Cloud_Rain : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.HomingEffectivenessMultiplier[Type] = 2;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RainFriendly);
			Projectile.aiStyle = 0;
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.hide = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.hide = false;
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && Collision.shimmer) {
				Projectile.velocity.Y -= 0.2f;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projHitbox.Intersects(targetHitbox)) return true;
			const float factor = 1.5f;
			Rectangle hitbox = projHitbox;
			for (int i = 0; i < 7; i++) {
				hitbox.X = projHitbox.X - (int)(Projectile.velocity.X * i * factor);
				hitbox.Y = projHitbox.Y - (int)(Projectile.velocity.Y * i * factor);
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!GraphicsUtils.drawingEffect) {
				Shimmer_Rod.cachedRain.Push(Projectile.whoAmI);
				return false;
			}
			lightColor = new Color(1f, 0, 0.08f, 0.3f);
			return true;
		}
	}
}
