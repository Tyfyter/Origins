using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Big_Bang : ModItem {
		public static int BarrelLength => 64;
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Big_Bang_P>(128, 34, 7.5f, 48, 32);
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 10);
			Item.UseSound = SoundID.Item92;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(Origins.Sounds.Lightning.WithPitch(1.2f));
			return base.UseItem(player);
		}
		public override Vector2? HoldoutOffset() => new(-14f, -6);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 unit = velocity.Normalized(out _);
			position += unit * 8 + unit.Perpendicular(player.direction) * 10;
		}
	}
	public class Big_Bang_P : ModProjectile, ICanisterProjectile, IShadedProjectile {
		public override string Texture => base.Texture + "_Generic";
		public static AutoLoadingTexture outerTexture = "";
		public static AutoLoadingTexture innerTexture = typeof(Big_Bang_P).GetDefaultTMLName();
		public AutoLoadingTexture OuterTexture => outerTexture;
		public AutoLoadingTexture InnerTexture => innerTexture;
		public int Shader => Origins.Shaders.Overbrighten.ShaderID;
		static readonly Parameter noOverbrighten = Parameter.uOpacity;
		static readonly Parameter overbrighten = Parameter.uOpacity with { Value = 1.5f };
		static readonly Parameter overbrightenColor = Parameter.uColor;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
			Origins.MagicTripwireDetonationStyle[Type] = 2;
			Main.projFrames[Type] = 6;
			Origins.Shaders.Overbrighten.BindParameters(
				noOverbrighten,
				overbrighten,
				overbrightenColor
			);
		}
		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.timeLeft = 240;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 2;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.velocity != default) Projectile.ai[1] = MathF.Round(Big_Bang.BarrelLength / Projectile.velocity.Length());
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Firework_Red, 0, 0, 200);
			dust.noGravity = true;
			dust.velocity *= 3f;

			if (Projectile.ai[1] > 0) {
				Projectile.ai[1]--;
				Projectile.numUpdates++;
			}
			const int HalfSpriteWidth = 20 / 2;
			const int HalfSpriteHeight = 20 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			if (Projectile.frameCounter.CycleUp(4)) Projectile.frame.CycleUp(Main.projFrames[Type]);
			if (Projectile.ai[0] != 0 && Projectile.width == 20) {
				Vector2 center = Projectile.Center;
				Projectile.Size *= Projectile.scale;
				Projectile.Center = center;
			}
			Lighting.AddLight(Projectile.Center, GlowColor);
			if (Projectile.localAI[0].CycleUp(Main.rand.Next(8, 19))) {
				const float max_dist = 16 * 10;
				Vector2 dir = Main.rand.NextVector2Unit();
				Vector2 pos = Projectile.Center;
				float dist = CollisionExt.Raymarch(pos, dir, max_dist);
				if (dist < max_dist) {
					Dust.NewDustPerfect(pos, ModContent.DustType<Big_Bang_Arc_Dust>(), pos + dir * dist, 7).customData = this;
					SoundEngine.PlaySound(Origins.Sounds.LittleZap, pos + dir * (dist * 0.5f));
				}
			}
		}
		Vector3 GlowColor {
			get {
				_ = ZapColors;
				return field;
			}
			set;
		}
		Vector3 OverbrightenColor {
			get {
				_ = ZapColors;
				return field;
			}
			set;
		}
		(float scale, Color color)[] ZapColors {
			get {
				if (field is null) SetupColors();
				return field;
				void SetupColors() {
					Vector3 color;
					if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile canister) && canister.CanisterData is CanisterData { HasSpecialEffect: true } canisterData) {
						color = canisterData.InnerColor.ToVector3();
					} else {
						color = new(1f, 0.32f, 0.51f);
					}
					GlowColor = color;
					HSLColor baseColor = default;
					baseColor.RGBV = color;
					switch ((int)(baseColor.H * 6)) {
						case 0:
						baseColor.Hue += 0.25f;
						break;
						case 1:
						baseColor.Hue += 0.25f;
						break;
						case 2:
						baseColor.Hue += 0.25f;
						break;
						case 3:
						baseColor.Hue -= 0.25f;
						break;
						case 4:
						baseColor.Hue -= 0.25f;
						break;
						case 5:
						baseColor.Hue -= 0.25f;
						break;
					}
					Vector3 obColor = baseColor.RGBV * 2;
					OverbrightenColor = obColor;
					ZapColors = [
						(0.2f, Overbrighten(color, obColor)),
						(0.1f, Overbrighten(color * 1.5f, obColor)),
						(0.05f, Overbrighten(color * 2.25f, obColor))
					];
					static Color Overbrighten(Vector3 color, Vector3 uColor) {
						float brightness = 0;
						if (color.X > 1)
							brightness += color.X - 1;
						if (color.Y > 1)
							brightness += color.Y - 1;
						if (color.Z > 1)
							brightness += color.Z - 1;

						return new Color((color + uColor * brightness) * 0.5f) with { A = 0 };
					}
				}
			}
			set;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			if (Projectile.ai[0] != 0) projectile.scale = 0.85f;
			Texture2D texture = InnerTexture;
			lightColor = canisterData.InnerColor;
			if (!canisterData.HasSpecialEffect) {
				texture = TextureAssets.Projectile[Type].Value;
				lightColor = Color.White;
			}
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Vector2 origin = frame.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			DrawData data = new(
				texture,
				projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			if (canisterData.HasSpecialEffect) {
				Origins.Shaders.Overbrighten.Apply(Projectile, data, overbrighten, overbrightenColor with {
					Value = OverbrightenColor
				});
			} else {
				Origins.Shaders.Overbrighten.Apply(Projectile, data, noOverbrighten);
			}
			data.Draw(Main.spriteBatch);
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(Origins.Sounds.Bonk, Projectile.Center);
			SoundEngine.PlaySound(Origins.Sounds.Lightning.WithVolume(0.6f), Projectile.Center);
			if (!Projectile.IsLocallyOwned() || Projectile.ai[0] != 0) return;
			int count = 3 + Main.rand.Next(3);
			Vector2 center = Projectile.Center;
			retry:
			List<Vector2> projectiles = OriginExtensions.PoissonDiskSampling(Main.rand, Projectile.Hitbox, v => v.WithinRange(center, 3), 2);
			if (projectiles.Count < count) goto retry;
			float speed = Projectile.oldVelocity.Length();
			for (int i = Math.Min(projectiles.Count - 1, count); i >= 0; i--) {
				if (projectiles[i] == center && count < projectiles.Count) projectiles[i] = projectiles[count++];
				Projectile.localNPCImmunity.CopyTo(
					Projectile.SpawnProjectile(
						Projectile.GetSource_FromThis(),
						center,
						(Projectile.velocity * 0.2f + (projectiles[i] - center)).Normalized(out float dist) * (speed * Main.rand.NextFloat(0.25f, 0.5f) + dist),
						Type,
						Projectile.damage / 2,
						Projectile.knockBack,
						1
					).localNPCImmunity.AsSpan()
				);
			}
		}
		public class Big_Bang_Arc_Dust : ModDust {
			public override string Texture => "Terraria/Images/Item_1";
			public override bool Update(Dust dust) {
				if (--dust.alpha <= 0) dust.active = false;
				return false;
			}
			public override bool MidUpdate(Dust dust) => false;
			public override bool PreDraw(Dust dust) {
				if (dust.customData is not Big_Bang_P self) return dust.active = false;
				Main.spriteBatch.DrawLightningArcBetween(
					self.Projectile.Center - Main.screenPosition,
					dust.velocity - Main.screenPosition,
					Main.rand.NextFloat(-4, 4),
					colors: self.ZapColors
				);
				return false;
			}
		}
	}
}
