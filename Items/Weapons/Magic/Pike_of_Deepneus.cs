using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Tiles.Other;
using Origins.Dusts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Origins.Items.Weapons.Magic {
	public class Pike_of_Deepneus : ModItem {
		public const int baseDamage = 64;
		public override void SetDefaults() {
			Item.damage = 160;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Pike_of_Deepneus_P>();
			Item.knockBack = 8;
			Item.shootSpeed = 12;
			Item.mana = 34;
			Item.useStyle = -1;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.reuseDelay = 4;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3);
			Item.UseSound = SoundID.Item69;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddRecipeGroup("HellBars", 16)
			.AddRecipeGroup("AdamantiteBars", 16)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 14)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override void UseItemFrame(Player player) {
			float rotation = player.itemRotation * player.gravDir - MathHelper.PiOver2 - GetArmDrawAngle(player);
			player.SetCompositeArmFront(
				true,
				Player.CompositeArmStretchAmount.Full,
				rotation
			);
			float num23 = rotation * (float)player.direction;
			player.bodyFrame.Y = player.bodyFrame.Height * 3;
			if (num23 < -0.75) {
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				if (player.gravDir == -1f) {
					player.bodyFrame.Y = player.bodyFrame.Height * 4;
				}
			}
			if (num23 > 0.6) {
				player.bodyFrame.Y = player.bodyFrame.Height * 4;
				if (player.gravDir == -1f) {
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				player.whoAmI,
				ai0: player.itemAnimationMax
			);
			return false;
		}

		internal static float GetArmDrawAngle(Player player) {
			return Math.Max((player.itemAnimation / (float)player.itemAnimationMax) * 6 - 5, 0) * (MathHelper.PiOver2 * 0.85f) * player.direction;
		}
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.owner == player.whoAmI && proj.type == Item.shoot && proj.ai[0] != 0) {
					if (proj.ai[1] >= 1) mult = 0;
					break;
				}
			}
		}
	}
	public class Pike_of_Deepneus_P : ModProjectile, IOutlineDrawer {
		public override string Texture => "Origins/Items/Weapons/Magic/Pike_of_Deepneus";
		AutoLoadingAsset<Texture2D> glowTexture = "Origins/Items/Weapons/Magic/Pike_of_Deepneus_Glow";
		public float squeeze = 1f;
		public bool ReachedMaxCharge = false;
		public float chargeProgress = 0f;
		public bool playedDing = false;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = Projectile.height = 32;
			Projectile.extraUpdates = 3;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.ArmorPenetration = 40;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item167, Projectile.position);
			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(null, Projectile.Center, Projectile.oldVelocity * 0.1f, ModContent.ProjectileType<Pike_of_Deepneus_Stuck>(), 0, 0);
			Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30), 1f);
			dust.noGravity = false;
			dust.velocity *= 8f;
		}
		public override void AI() {
			/*Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.Firework_Yellow, 0, 0, 50, default, 0.6f);
			dust.noGravity = false;
			dust.velocity *= 4f;*/

			if (Projectile.ai[0] != 0) {
				squeeze = chargeProgress;
				Player player = Main.player[Projectile.owner];
				if (Projectile.owner == Main.myPlayer) {
					if (player.channel) {
						Vector2 newVel = (Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter)).SafeNormalize(Vector2.UnitX * player.direction);
						if (player.HeldItem.shoot == Type) {
							newVel *= player.HeldItem.shootSpeed * ((chargeProgress / 2f) + 0.5f);
						}
						if (newVel != Projectile.velocity) {
							Projectile.netUpdate = true;
						}
						Projectile.velocity = newVel;
						if (Projectile.ai[1] < 1 && !ReachedMaxCharge) {
							Projectile.ai[1] += 1 / (Projectile.ai[0] * 5);
							chargeProgress = Projectile.ai[1];

						} else {
							ReachedMaxCharge = true;
							chargeProgress *= 0.995f;
						}
					} else {
						if (ReachedMaxCharge) { SoundEngine.PlaySound(SoundID.Item16, Projectile.position); }
						Projectile.ai[0] = 0;
						Projectile.velocity *= 1 + Projectile.ai[1] * 0.5f;
						Projectile.netUpdate = true;
					}
				}
				player.itemRotation = Projectile.velocity.ToRotation();
				player.itemAnimation = (int)(player.itemAnimationMax * (1 + Projectile.ai[1] * 0.15f));
				player.itemTime = player.itemTimeMax = player.itemAnimation;
				player.heldProj = Projectile.whoAmI;
				player.ChangeDir(Projectile.direction = Math.Sign(Projectile.velocity.X));
				Projectile.rotation = player.itemRotation;
				Projectile.Center = player.MountedCenter
					+ new Vector2(player.direction * -4, -6 * player.gravDir)
					+ OriginExtensions.Vec2FromPolar(player.itemRotation - Pike_of_Deepneus.GetArmDrawAngle(player) * player.gravDir, 16)
					+ Projectile.velocity.SafeNormalize(default) * 36;
			} else {
				Projectile.hide = false;
				Projectile.tileCollide = true;
				Projectile.velocity.Y += 0.01f * (1f / chargeProgress - 1f);
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.ai[0] != 0) return false;
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 0.34f + chargeProgress * chargeProgress * .66f;
			modifiers.Knockback *= 1 + chargeProgress * 0.65f;
			for (int i = 0; i < 4; i++)
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Pike_of_Deepneus_Spark_Dust>(), Velocity: -Projectile.velocity.RotatedByRandom(0.35), newColor: Color.Yellow);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 14;
			height = 14;
			fallThrough = true;
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Vector2 position = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.rotation + (MathHelper.Pi * 0.8f * Projectile.direction - MathHelper.PiOver2 * player.gravDir);
			Vector2 scale = new Vector2(Projectile.scale, Projectile.scale);
			SpriteEffects spriteEffects = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			if (player.gravDir < 0) {
				rotation -= MathHelper.PiOver2 * 1.2f * Projectile.direction;
				spriteEffects ^= SpriteEffects.FlipHorizontally;
			}
			Vector2 origin = new Vector2(55, 9).Apply(spriteEffects, TextureAssets.Projectile[Type].Size());
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				position,
				null,
				lightColor,
				rotation,
				origin,
				scale,
				spriteEffects,
			0);
			this.DrawOutline();
			if (Projectile.ai[0] == 0 && chargeProgress >= 0.9f) {
				Main.EntitySpriteDraw(TextureAssets.Extra[ExtrasID.FallingStar].Value, position, null, Color.Yellow, Projectile.rotation + MathHelper.PiOver2, TextureAssets.Extra[ExtrasID.StarWrath].Size() / 2f - new Vector2(0, 32f), new Vector2(1f, 3f), SpriteEffects.None);

			}
			Main.EntitySpriteDraw(data);
			data.texture = glowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			float shineProgress = MathHelper.Lerp(0, 1, (chargeProgress - .9f) * 9f);
			if (chargeProgress >= .9f) {
				DrawPrettyStarSparkle(SpriteEffects.None, Projectile.Center - Main.screenPosition, Color.Yellow, Color.Turquoise, 0, Vector2.One * 5f * shineProgress, Vector2.One * 2);
				if (!playedDing) {
					playedDing = true;
					SoundEngine.PlaySound(SoundID.Item105, Projectile.Center);
				}
			}
			return false;
		}

		public Color? SetOutlineColor(float progress) {
			return Color.Lerp(Color.Yellow * chargeProgress, Color.Turquoise, (MathF.Sin(((float)Main.timeForVisualEffects) + 1f) / 2f) * 2f);
		}

		public DrawData[] OutlineDrawDatas {
			get {
				Vector2 position = Projectile.Center;
				float rotation = Projectile.rotation + (MathHelper.Pi * 0.8f * Projectile.direction - MathHelper.PiOver2);
				float scale = Projectile.scale;
				SpriteEffects spriteEffects = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				Vector2 origin = new Vector2(55, 9).Apply(spriteEffects, TextureAssets.Projectile[Type].Size());
				return [new DrawData(
					TextureAssets.Projectile[Type].Value,
					position,
					null,
					Color.White,
					rotation,
					origin,
					scale,
					spriteEffects,
				0)];
			}
		}
		public int OutlineSteps { get => 16; }
		public float OutlineOffset { get => 2; }

		private static void DrawPrettyStarSparkle(SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float rotation, Vector2 scale, Vector2 fatness) {
			Texture2D sparkleTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			Color bigColor = shineColor * 0.5f;
			Vector2 origin = sparkleTexture.Size() / 2f;
			Color smallColor = drawColor * 0.5f;
			Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X);
			Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y);

			// Bright, large part
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, bigColor, 0f + rotation, origin, scaleUpDown, dir);
			// Dim, small part
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, MathHelper.PiOver2 + rotation, origin, scaleLeftRight * 0.6f, dir);
			Main.EntitySpriteDraw(sparkleTexture, drawPos, null, smallColor, 0f + rotation, origin, scaleUpDown * 0.6f, dir);
		}
	}

	public class Pike_of_Deepneus_Stuck : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Pike_of_Deepneus";
		AutoLoadingAsset<Texture2D> glowTexture = "Origins/Items/Weapons/Magic/Pike_of_Deepneus_Glow";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = Projectile.height = 32;
			Projectile.extraUpdates = 3;
			Projectile.aiStyle = -1;
			Projectile.alpha = 0;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.ArmorPenetration = 40;
			Projectile.timeLeft = 60;
			Projectile.hide = false;
		}
		public override bool? CanDamage() => false;
		public override void AI() {
			Projectile.velocity *= 0.95f;
			Projectile.Opacity = MathHelper.Lerp(0f, 1f, Projectile.timeLeft / 60f);
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 position = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.rotation + (MathHelper.Pi * 0.8f * Projectile.direction - MathHelper.PiOver2);
			float scale = Projectile.scale;
			SpriteEffects spriteEffects = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Vector2 origin = new Vector2(55, 9).Apply(spriteEffects, TextureAssets.Projectile[Type].Size());
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				position,
				null,
				lightColor * Projectile.Opacity,
				rotation,
				origin,
				scale,
				spriteEffects,
			0);
			Main.EntitySpriteDraw(data);
			data.texture = glowTexture;
			data.color = Color.White * Projectile.Opacity;
			Main.EntitySpriteDraw(data);
			return false;

		}

	}
}
