using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Ammo.Canisters;
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

namespace Origins.Items.Weapons.Demolitionist {
	public class Big_Bang : ModItem {
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Big_Bang_P>(70, 34, 7.5f, 48, 32);
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 2);
			Item.UseSound = SoundID.Item62.WithPitch(0.4f);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 16)
			.AddIngredient<Carburite_Item>(22)
			.AddIngredient<Chambersite_Item>(10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return true;
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
			Projectile.extraUpdates = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
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
				Projectile.width = 17;
				Projectile.height = 17;
				Projectile.Center = center;
			}
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
				Vector3 baseColor = Main.rgbToHsl(lightColor);
				switch ((int)(baseColor.X * 6)) {
					case 0:
					baseColor.X += 0.25f;
					break;
					case 1:
					baseColor.X += 0.25f;
					break;
					case 2:
					baseColor.X += 0.25f;
					break;
					case 3:
					baseColor.X -= 0.25f;
					break;
					case 4:
					baseColor.X -= 0.25f;
					break;
					case 5:
					baseColor.X -= 0.25f;
					break;
				}
				Color color = Main.hslToRgb(baseColor);
				Origins.Shaders.Overbrighten.Apply(Projectile, data, overbrighten, overbrightenColor with {
					Value = color.ToVector3() * 2
				});
			} else {
				Origins.Shaders.Overbrighten.Apply(Projectile, data, noOverbrighten);
			}
			data.Draw(Main.spriteBatch);
		}
		public override void OnKill(int timeLeft) {
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
	}
}
