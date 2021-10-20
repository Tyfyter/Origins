using Microsoft.Xna.Framework;
using Origins.Projectiles;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
    public class Defiled_Sand : DefiledTile {
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.Sand[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sand];
            Main.tileMerge[TileID.Sand][Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Sand];
            Main.tileMerge[Type][TileID.Sand] = true;*/
            TileID.Sets.Falling[Type] = true;
			drop = ItemType<Defiled_Sand_Item>();
			AddMapEntry(new Color(175, 175, 175));
			SetModTree(Defiled_Tree.Instance);
            mergeID = TileID.Sand;
		}
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (WorldGen.noTileActions)
				return true;

			Tile above = Main.tile[i, j - 1];
			Tile below = Main.tile[i, j + 1];
			bool canFall = true;

			if (below == null || below.active())
				canFall = false;

			if (above.active() && (TileID.Sets.BasicChest[above.type] || TileID.Sets.BasicChestFake[above.type] || above.type == TileID.PalmTree || TileLoader.IsDresser(above.type)))
				canFall = false;

			if (canFall) {
				//Set the projectile type to ExampleSandProjectile
				int projectileType = ProjectileType<Defiled_Sand_Ball>();
				float positionX = i * 16 + 8;
				float positionY = j * 16 + 8;

				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.tile[i, j].ClearTile();
                    OriginGlobalProj.hostileNext = true;
					int proj = Projectile.NewProjectile(positionX, positionY, 0f, 0.41f, projectileType, 10, 0f, Main.myPlayer);
					Main.projectile[proj].ai[0] = 1f;
					Main.projectile[proj].hostile = true;
					WorldGen.SquareTileFrame(i, j);
				}
				else if (Main.netMode == NetmodeID.Server) {
					Main.tile[i, j].active(false);
					bool spawnProj = true;

					for (int k = 0; k < 1000; k++) {
						Projectile otherProj = Main.projectile[k];

						if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == projectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(positionX, positionY)) < 4f) {
							spawnProj = false;
							break;
						}
					}

					if (spawnProj) {
						int proj = Projectile.NewProjectile(positionX, positionY, 0f, 2.5f, projectileType, 10, 0f, Main.myPlayer);
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
		public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<Defiled_Tree_Sapling>();
		}
        public override bool CreateDust(int i, int j, ref int type) {
            type = DefiledWastelands.DefaultTileDust;
            return true;
        }
    }
    public class Defiled_Sand_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Sand");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.SandBlock);
            item.createTile = TileType<Defiled_Sand>();
            item.ammo = AmmoID.Sand;
		}
        public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
            type = ProjectileType<Defiled_Sand_Ball>();
        }
    }
    public class Defiled_Sand_Ball : ModProjectile {
        public override bool CloneNewInstances => true;
        protected bool falling = true;
		protected int tileType;
        bool init = true;
		protected const int dustType = 51;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled Sand Ball");
			ProjectileID.Sets.ForcePlateDetection[projectile.type] = true;
		}

		public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.SandBallGun);
            projectile.hostile = false;
			projectile.knockBack = 6f;
			projectile.penetrate = -1;
            projectile.aiStyle = 1;
			//Set the tile type to ExampleSand
			tileType = TileType<Defiled_Sand>();
		}

		public override void AI() {
            if(init) {
                falling = projectile.hostile;
                init = false;
            }
			//Change the 5 to determine how much dust will spawn. lower for more, higher for less
			if (Main.rand.Next(5) == 0) {
				int dust = Dust.NewDust(projectile.position, projectile.width, projectile.height, dustType);
				Main.dust[dust].velocity.X *= 0.4f;
			}

			projectile.tileCollide = true;
			projectile.localAI[1] = 0f;

			if (projectile.ai[0] == 1f) {
				if (!falling) {
					projectile.ai[1] += 1f;

					if (projectile.ai[1] >= 60f) {
						projectile.ai[1] = 60f;
						projectile.velocity.Y += 0.2f;
					}
				}
				else
					projectile.velocity.Y += 0.41f;
			}
			else if (projectile.ai[0] == 2f) {
				projectile.velocity.Y += 0.2f;

				if (projectile.velocity.X < -0.04f)
					projectile.velocity.X += 0.04f;
				else if (projectile.velocity.X > 0.04f)
					projectile.velocity.X -= 0.04f;
				else
					projectile.velocity.X = 0f;
			}

			projectile.rotation += 0.1f;

			if (projectile.velocity.Y > 10f)
				projectile.velocity.Y = 10f;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            Vector2 velocity = projectile.velocity;
			if (falling)
				projectile.velocity = Collision.AnyCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, true);
			else
				projectile.velocity = Collision.TileCollision(projectile.position, projectile.velocity, projectile.width, projectile.height, true, true, 1);
            if(falling) {
				int tileX = (int)(projectile.Center.X) / 16;
				int tileY = (int)(projectile.Center.Y) / 16;
                if(projectile.velocity!=velocity||Main.tile[tileX, tileY + 1].active())projectile.Kill();
            }else if(projectile.velocity!=velocity) {
                falling = true;
                projectile.hostile = true;
            }
			return false;
		}

		public override void Kill(int timeLeft) {
			if (projectile.owner == Main.myPlayer && !projectile.noDropItem) {
				int tileX = (int)(projectile.position.X + projectile.width / 2) / 16;
				int tileY = (int)(projectile.position.Y + projectile.width / 2) / 16;

				Tile tile = Main.tile[tileX, tileY];
				Tile tileBelow = Main.tile[tileX, tileY + 1];

				if (tile.halfBrick() && projectile.velocity.Y > 0f && System.Math.Abs(projectile.velocity.Y) > System.Math.Abs(projectile.velocity.X))
					tileY--;

				if (!tile.active()) {
					bool onMinecartTrack = tileY < Main.maxTilesY - 2 && tileBelow != null && tileBelow.active() && tileBelow.type == TileID.MinecartTrack;

					if (!onMinecartTrack)
						WorldGen.PlaceTile(tileX, tileY, tileType, false, true);

					if (!onMinecartTrack && tile.active() && tile.type == tileType) {
						if (tileBelow.halfBrick() || tileBelow.slope() != 0) {
							WorldGen.SlopeTile(tileX, tileY + 1, 0);

							if (Main.netMode == NetmodeID.Server)
								NetMessage.SendData(MessageID.TileChange, -1, -1, null, 14, tileX, tileY + 1);
						}

						if (Main.netMode != NetmodeID.SinglePlayer)
							NetMessage.SendData(MessageID.TileChange, -1, -1, null, 1, tileX, tileY, tileType);
					}
				}
			}
		}

        public override bool CanDamage() => projectile.localAI[1] != -1f;
    }
}
