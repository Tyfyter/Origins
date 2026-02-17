using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using PegasusLib;
using ReLogic.Content;
using System;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Origins.Graphics;
using Origins.Items.Accessories;
namespace Origins.Items.Weapons.Melee {
	public class Bone_Latcher : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			// Call this method to quickly set some of the properties below.
			Item.DefaultToWhip(ModContent.ProjectileType<Bone_Latcher_P>(), 20, 2, 4, 35);

			Item.DamageType = DamageClass.Melee;
			Item.damage = 20;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
	}
	public class Bone_Latcher_P : ModProjectile, IWhipProjectile, ITangelaHaver {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.DamageType = DamageClass.Melee;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

			if (Timer >= swingTime || owner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;

			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				List<Vector2> points = Projectile.WhipPointsForCollision;
				Projectile.FillWhipControlPoints(Projectile, points);
				SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
			}
		}

		public void GetWhipSettings(out float timeToFlyOut, out int segments, out float rangeMultiplier) {
			timeToFlyOut = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
			segments = 20;
			rangeMultiplier = 1.1f * Projectile.scale;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float range = 192 * Projectile.scale;
			if (target.DistanceSQ(Main.player[Projectile.owner].Center) > range * range) {
				modifiers.Knockback *= 1.5f;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			float range = 192 * Projectile.scale;
			Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;
			if (!Main.player[Projectile.owner].WithinRange(target.Center, range)) Projectile.direction *= -1;
			return base.CanHitNPC(target);
		}

		private void DrawLine(List<Vector2> list) {
			if (TangelaVisual.DrawOver) return;
			Texture2D texture = TextureAssets.FishingLine.Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new(frame.Width / 2, 2);

			Vector2 pos = list[0];
			float progress = 0;
			for (int i = 0; i < list.Count - 1; i++) {
				Vector2 element = list[i];
				Vector2 diff = list[i + 1] - element;

				float rotation = diff.ToRotation() - MathHelper.PiOver2;
				float length = diff.Length();
				Vector2 scale = new(2, length / frame.Height);

				TangelaVisual.DrawTangela(this, texture, pos - Main.screenPosition, frame, rotation, origin, scale, SpriteEffects.None, extraOffset: new(0, progress));

				pos += diff;
				progress += length;
			}
		}
		public int? TangelaSeed { get; set; }

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new();
			Projectile.FillWhipControlPoints(Projectile, list);
			DrawLine(list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new(0, 0, 48, 28);
				Vector2 origin = new(24, 14);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 1) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
}
