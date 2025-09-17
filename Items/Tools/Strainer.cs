using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Tools {
	public class Strainer : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LaserDrill);
			Item.damage = 13;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 65;
			Item.width = 36;
			Item.height = 24;
			Item.useTime = 15;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Green;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<Strainer_P>();
			Item.shootSpeed = 16;
			Item.tileBoost = 7;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 12)
			.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Strainer_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
		}

		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ownerHitCheck = true;
			Projectile.aiStyle = 0;
			Projectile.hide = true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			Projectile.timeLeft = 60;

			// Animation code could go here if the projectile was animated. 

			// Plays a sound every 20 ticks. In aiStyle 20, soundDelay is set to 30 ticks.
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);
				Projectile.soundDelay = 20;
			}

			Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);
			if (Main.myPlayer == Projectile.owner) {
				// This code must only be ran on the client of the projectile owner
				if (player.channel) {
					PolarVec2 offset;
					if (Main.SmartCursorIsUsed && player.IsTargetTileInItemRange(player.HeldItem) && Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile) {
						offset = (PolarVec2)(new Vector2(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates() - playerCenter);
						offset.R = MathHelper.Max(offset.R - 8, player.HeldItem.shootSpeed * Projectile.scale + 26);
					} else {
						float tileBoost = player.HeldItem.tileBoost;
						Vector2 tileRange = new Vector2(Player.tileRangeX + tileBoost, Player.tileRangeY + tileBoost) * 16;
						Vector2 mouseOffset = Main.MouseWorld - playerCenter;
						
						if (GeometryUtils.GetIntersectionPoints(Vector2.Zero, mouseOffset, -tileRange, tileRange, out List<Vector2> intersections)) {
							offset = (PolarVec2)intersections[0];
						} else {
							offset = (PolarVec2)mouseOffset;
						}
						offset.R = MathHelper.Max(offset.R - 8, player.HeldItem.shootSpeed * Projectile.scale + 26);
					}
					// Calculate a normalized vector from player to mouse and multiply by holdoutDistance to determine resulting holdoutOffset
					Vector2 holdoutOffset = (Vector2)offset;
					if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y) {
						// This will sync the projectile, most importantly, the velocity.
						Projectile.netUpdate = true;
					}

					// Projectile.velocity acts as a holdoutOffset for held projectiles.
					Projectile.velocity = holdoutOffset;
				} else {
					Projectile.Kill();
				}
			}

			if (Projectile.velocity.X > 0f) {
				player.ChangeDir(1);
			} else if (Projectile.velocity.X < 0f) {
				player.ChangeDir(-1);
			}

			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
			player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
			player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
			Projectile.Center = playerCenter; // Centers the projectile on the player. Projectile.velocity will be added to this in later Terraria code causing the projectile to be held away from the player at a set distance.
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

			if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 0;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			bool swapDir = Projectile.velocity.X * player.gravDir < 0;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 unit = Projectile.velocity.SafeNormalize(default);
			float baseUnitScale = player.HeldItem.shootSpeed * Projectile.scale;
			Vector2 baseOffset = baseUnitScale * unit;
			Vector2 basePos = player.RotatedRelativePoint(player.MountedCenter) + baseOffset;
			OriginExtensions.DrawGrappleChain(
				basePos + unit * (baseUnitScale - 4),
				Projectile.Center - unit * (swapDir ? 1 : 0),
				texture,
				[
					new Rectangle(46, 6 + Projectile.frame * 24, 8, 14),
					new Rectangle(46, 6 + (Projectile.frame ^ 1) * 24, 8, 14)
				],
				new Color(225, 225, 225, 180),
				true,
				action: (pos) => {
					Lighting.AddLight(pos, Color.Cyan.ToVector3() * 0.05f);
				}
			);
			Main.EntitySpriteDraw(
				texture,
				basePos - Main.screenPosition,
				new Rectangle(0, 0 + Projectile.frame * 24, 20, 22),
				lightColor,
				Projectile.rotation + (swapDir ? MathHelper.PiOver2 : -MathHelper.PiOver2),
				new Vector2(swapDir ? 7 : 13, 11),
				Projectile.scale,
				swapDir ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0);
			Main.EntitySpriteDraw(
				texture,
				basePos - Main.screenPosition,
				new Rectangle(22, 0 + Projectile.frame * 24, 22, 22),
				new Color(255, 255, 255, 255),
				Projectile.rotation + (swapDir ? MathHelper.PiOver2 : -MathHelper.PiOver2),
				new Vector2(swapDir ? 19 : 3, 11),
				Projectile.scale,
				swapDir ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				new Rectangle(56, 4 + Projectile.frame * 24, 22, 16),
				Lighting.GetColor(Projectile.Center.ToTileCoordinates()),
				Projectile.rotation + (swapDir ? MathHelper.PiOver2 : -MathHelper.PiOver2),
				new Vector2(11, 9 - Projectile.frame * 2),
				Projectile.scale,
				swapDir ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0);
			return false;
		}
		[Obsolete("Just here while I update PegasusLib")]
		public static bool GetIntersectionPoints(Vector2 start, Vector2 end, Vector2 topLeft, Vector2 bottomRight, out List<Vector2> intersections) {
			intersections = [];
			Vector2 intersection;
			if (Intersects(start, end, topLeft, new Vector2(bottomRight.X, topLeft.Y), out intersection)) intersections.Add(intersection);
			if (Intersects(start, end, new Vector2(bottomRight.X, topLeft.Y), bottomRight, out intersection)) intersections.Add(intersection);
			if (Intersects(start, end, bottomRight, new Vector2(topLeft.X, bottomRight.Y), out intersection)) intersections.Add(intersection);
			if (Intersects(start, end, new Vector2(topLeft.X, bottomRight.Y), topLeft, out intersection)) intersections.Add(intersection);
			return intersections.Count != 0;
		}
		public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection) {
			intersection = a1;
			Vector2 b = a2 - a1;
			Vector2 d = b2 - b1;
			float bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if (bDotDPerp == 0) return true;

			Vector2 c = b1 - a1;
			float t = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
			if (t < 0 || t > 1) {
				return false;
			}

			float u = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
			if (u < 0 || u > 1) {
				return false;
			}
			intersection = a1 + t * b;
			return true;
		}
	}
}
