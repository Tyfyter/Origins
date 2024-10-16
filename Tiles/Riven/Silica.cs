﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Silica : OriginTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
			TileID.Sets.isDesertBiomeSand[Type] = true;
			TileID.Sets.Conversion.Sand[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sand];
            Main.tileMerge[TileID.Sand][Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Sand];
            Main.tileMerge[Type][TileID.Sand] = true;*/
			TileID.Sets.Falling[Type] = true;
			AddMapEntry(new Color(194, 200, 200));
			mergeID = TileID.Sand;
			DustType = DustID.Bone;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (WorldGen.noTileActions)
				return true;

			Tile above = Main.tile[i, j - 1];
			Tile below = Main.tile[i, j + 1];
			bool canFall = true;

			if (below == null || below.HasTile)
				canFall = false;

			if (above.HasTile && (TileID.Sets.BasicChest[above.TileType] || TileID.Sets.BasicChestFake[above.TileType] || above.TileType == TileID.PalmTree))// || TileLoader.IsDresser(above.TileType)
				canFall = false;

			if (canFall) {
				//Set the projectile type to ExampleSandProjectile
				int projectileType = ProjectileType<Silica_Ball>();
				float positionX = i * 16 + 8;
				float positionY = j * 16 + 8;

				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.tile[i, j].ClearTile();
					int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), positionX, positionY, 0f, 0.41f, projectileType, 10, 0f, Main.myPlayer);
					Main.projectile[proj].ai[0] = 1f;
					Main.projectile[proj].hostile = true;
					WorldGen.SquareTileFrame(i, j);
				} else if (Main.netMode == NetmodeID.Server) {
					Tile tile0 = Main.tile[i, j];
					tile0.HasTile = false;
					bool spawnProj = true;

					for (int k = 0; k < 1000; k++) {
						Projectile otherProj = Main.projectile[k];

						if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == projectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f) {
							spawnProj = false;
							break;
						}
					}

					if (spawnProj) {
						int proj = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), positionX, positionY, 0f, 2.5f, projectileType, 10, 0f, Main.myPlayer);
						Main.projectile[proj].velocity.Y = 0.5f;
						Main.projectile[proj].position.Y += 2f;
						Main.projectile[proj].netUpdate = true;
					}

					NetMessage.SendTileSquare(-1, i, j, 1);
					WorldGen.SquareTileFrame(i, j);
				}
				return false;
			}
			return true;
		}
	}
	public class Silica_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SandBlock);
			Item.createTile = TileType<Silica>();
			Item.ammo = AmmoID.Sand;
		}
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			type = ProjectileType<Silica_Ball>();
		}
	}
	public class Silica_Ball : ModProjectile {
		protected override bool CloneNewInstances => true;
		protected bool falling = true;
		protected int tileType;
		bool init = true;
		protected const int dustType = 51;

		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Sand Ball");
			ProjectileID.Sets.ForcePlateDetection[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SandBallGun);
			Projectile.hostile = false;
			Projectile.knockBack = 6f;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 1;
			//Set the tile type to ExampleSand
			tileType = TileType<Silica>();
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_TileBreak) Projectile.hostile = true;
		}
		public override void AI() {
			if (init) {
				falling = Projectile.hostile;
				init = false;
			}
			//Change the 5 to determine how much dust will spawn. lower for more, higher for less
			if (Main.rand.NextBool(5)) {
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType);
				Main.dust[dust].velocity.X *= 0.4f;
			}

			Projectile.tileCollide = true;
			Projectile.localAI[1] = 0f;

			if (Projectile.ai[0] == 1f) {
				if (!falling) {
					Projectile.ai[1] += 1f;

					if (Projectile.ai[1] >= 60f) {
						Projectile.ai[1] = 60f;
						Projectile.velocity.Y += 0.2f;
					}
				} else {
					Projectile.velocity.Y += 0.41f;
				}
			} else if (Projectile.ai[0] == 2f) {
				Projectile.velocity.Y += 0.2f;

				if (Projectile.velocity.X < -0.04f)
					Projectile.velocity.X += 0.04f;
				else if (Projectile.velocity.X > 0.04f)
					Projectile.velocity.X -= 0.04f;
				else
					Projectile.velocity.X = 0f;
			}

			Projectile.rotation += 0.1f;

			if (Projectile.velocity.Y > 10f)
				Projectile.velocity.Y = 10f;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			Vector2 velocity = Projectile.velocity;
			if (falling)
				Projectile.velocity = Collision.AnyCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true);
			else
				Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height, true, true, 1);
			if (falling) {
				int tileX = (int)(Projectile.Center.X) / 16;
				int tileY = (int)(Projectile.Center.Y) / 16;
				if (Projectile.velocity != velocity || Main.tile[tileX, tileY + 1].HasTile) Projectile.Kill();
			} else if (Projectile.velocity != velocity) {
				falling = true;
				Projectile.hostile = true;
			}
			return false;
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			if (Projectile.owner == Main.myPlayer && !Projectile.noDropItem) {
				int tileX = (int)(Projectile.position.X + Projectile.width / 2) / 16;
				int tileY = (int)(Projectile.position.Y + Projectile.width / 2) / 16;

				Tile tile = Main.tile[tileX, tileY];
				Tile tileBelow = Main.tile[tileX, tileY + 1];

				if (tile.IsHalfBlock && Projectile.velocity.Y > 0f && Math.Abs(Projectile.velocity.Y) > System.Math.Abs(Projectile.velocity.X))
					tileY--;

				if (!tile.HasTile) {
					bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.HasTile && tileBelow.TileType == TileID.MinecartTrack;

					if (!onMinecartTrack)
						WorldGen.PlaceTile(tileX, tileY, tileType, false, true);

					if (!onMinecartTrack && tile.HasTile && tile.TileType == tileType) {
						if (tileBelow.IsHalfBlock || tileBelow.Slope != 0) {
							WorldGen.SlopeTile(tileX, tileY + 1, 0);

							if (Main.netMode == NetmodeID.Server)
								NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, tileX, tileY + 1);
						}

						if (Main.netMode != NetmodeID.SinglePlayer)
							NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, tileX, tileY, tileType);
					}
				}
			}
		}

		public override bool? CanDamage() => Projectile.localAI[1] != -1f ? null : false;
	}
}
