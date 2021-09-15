using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Origins.Walls;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public static class RivenHive {
		public const int NeededTiles = 200;
		public const int ShaderTileCount = 25;
		public static class SpawnRates {
			public const float Fighter = 1;
			public const float Mummy = 1;
			public const float Jelly = 0.8f;
			public const float Tank = 0.6f;
			public const float Shark1 = 0.4f;
			public const float Crawler = 0.8f;
		}
		public static class Gen {
			static int lesionCount = 0;
			public static void StartHive(int i, int j) {
				const float strength = 2.4f;
				const float wallThickness = 4f;
				ushort fleshID = (ushort)ModContent.TileType<Riven_Flesh>();
				ushort weakFleshID = (ushort)ModContent.TileType<Weak_Riven_Flesh>();
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				lesionCount = 0;
				int j2 = j;
				if (j2 > Main.worldSurface) {
					j2 = (int)Main.worldSurface;
				}
				for (; !SolidTile(i, j2); j2++) {
				}
				Vector2 position = new Vector2(i, j2);
				for (int x = i - 30; x < i + 30; x++) {
					for (int y = j2 - 25; y < j2 + 15; y++) {
						float diff = (((y - j2) * (y - j2) * 1.5f) + (x - i) * (x - i));
						if (diff > 800) {
							continue;
						}
						Main.tile[x, y].ResetToType(fleshID);
						if (diff < 750) {
							Main.tile[x, y].wall = fleshWallID;
						}
					}
				}
				Vector2 vector = new Vector2(0, -1).RotatedByRandom(1.6f);
				int distance = 0;
				while (Main.tile[(int)position.X, (int)position.Y].active() && Main.tileSolid[Main.tile[(int)position.X, (int)position.Y].type]) {
					//Main.tile[(int)position.X, (int)position.Y].ResetToType(TileID.EmeraldGemspark);
					//SquareTileFrame((int)position.X, (int)position.Y);
					position += vector;
					if (++distance >= 160) {
						break;
					}
				}
				vector = -vector;
				(Vector2 position, Vector2 velocity) last = (position, vector);
				//Tile t = Main.tile[(int)last.position.X, (int)last.position.Y];
				(int x, int y, Vector2 direction, double length) startValues = ((int)last.position.X, (int)last.position.Y, last.velocity.RotatedByRandom(0.5f), distance * genRand.NextFloat(0.4f, 0.6f));
				last = GenRunners.WalledVeinRunner(startValues.x, startValues.y, strength, startValues.direction, startValues.length, weakFleshID, wallThickness);
				//t.ResetToType(TileID.AmethystGemspark);
				Vector2 manualVel = new Vector2(last.velocity.X, 0.2f);
				//t = Main.tile[(int)last.position.X, (int)last.position.Y];
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(new Vector2(-manualVel.X, 0.2f)), genRand.NextFloat(distance * 0.4f, distance * 0.6f) * (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, new Vector2(0, 1).RotatedByRandom(0.2f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
				//t.ResetToType(TileID.AmethystGemspark);
				manualVel.X = -manualVel.X;
				//t = Main.tile[(int)last.position.X, (int)last.position.Y];
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(new Vector2(-manualVel.X, 0.2f)), genRand.NextFloat(distance * 0.4f, distance * 0.6f) * (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, Vector2.Normalize(manualVel), genRand.NextFloat(distance * 0.4f, distance * 0.6f) / (Math.Abs(manualVel.X) + 0.5f), weakFleshID, wallThickness, wallType: fleshWallID);
				last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, new Vector2(0, 1).RotatedByRandom(0.2f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
				//t.ResetToType(TileID.AmethystGemspark);
				for (int index = 0; index < 10; index++) {
					//t = Main.tile[(int)last.position.X, (int)last.position.Y];
					last = GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedByRandom(0.8f), genRand.NextFloat(distance * 0.2f, distance * 0.3f), weakFleshID, wallThickness, wallType: fleshWallID);
					if (index < 8) {
						GenRunners.WalledVeinRunner((int)last.position.X, (int)last.position.Y, strength, last.velocity.RotatedBy(genRand.Next(2) * 2 - 1).RotatedByRandom(0.8f), genRand.NextFloat(distance * 0.4f, distance * 0.6f), weakFleshID, wallThickness, wallType: fleshWallID);
					}
					PolarVec2 vel = new PolarVec2(1, last.velocity.ToRotation());
					OriginExtensions.AngularSmoothing(ref vel.Theta, MathHelper.PiOver2, 0.7f);
					//t.ResetToType(TileID.AmethystGemspark);
					last = (last.position, (Vector2)vel);
				}
				//t = Main.tile[(int)last.position.X, (int)last.position.Y];
				//t.ResetToType(TileID.AmethystGemspark);
				Point caveCenter = HiveCave((int)last.position.X, (int)last.position.Y);
				Vector2 cavernOpening = last.position - caveCenter.ToVector2();
				GenRunners.VeinRunner((int)last.position.X, (int)last.position.Y, strength, cavernOpening.SafeNormalize(Vector2.Zero), cavernOpening.Length());
				GenRunners.VeinRunner(startValues.x, startValues.y, strength, startValues.direction, startValues.length);
				(Vector2 position, Vector2 velocity)[] arms = new (Vector2 position, Vector2 velocity)[4];
				arms[0] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength, new Vector2(1, -0.25f).RotatedByRandom(0.2f), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				arms[1] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength, new Vector2(1, 0.25f).RotatedByRandom(0.2f), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				arms[2] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength, new Vector2(-1, -0.25f).RotatedByRandom(0.2f), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				arms[3] = last = GenRunners.WalledVeinRunner(caveCenter.X, caveCenter.Y, strength, new Vector2(-1, 0.25f).RotatedByRandom(0.2f), genRand.NextFloat(32, 64), weakFleshID, wallThickness, wallType: fleshWallID);
				HiveCave((int)last.position.X, (int)last.position.Y, genRand.NextFloat(0.3f, 0.5f));
				for (int arm = 0; arm < 4; arm++) {
					GenRunners.VeinRunner((int)arms[arm].position.X, (int)arms[arm].position.Y, strength, -arms[arm].velocity, 24);
				}
			}
			public static Point HiveCave(int i, int j, float sizeMult = 1f) {
				ushort fleshID = (ushort)ModContent.TileType<Riven_Flesh>();
				ushort fleshWallID = (ushort)ModContent.WallType<Riven_Flesh_Wall>();
				ushort lesionID = (ushort)ModContent.TileType<Riven_Lesion>();
				int i2 = i + (int)(genRand.Next(-26, 26) * sizeMult);
				int j2 = j + (int)(genRand.Next(-2, 22) * sizeMult);
				Queue<Point> lesionPlacementSpots = new Queue<Point>();
				for (int x = i2 - (int)(33 * sizeMult + 5); x < i2 + (int)(33 * sizeMult + 5); x++) {
					for (int y = j2 + (int)(28 * sizeMult + 4); y >= j2 - (int)(28 * sizeMult + 4); y--) {
						float sq = Math.Max(Math.Abs(y - j2) * 1.5f, Math.Abs(x - i2));
						float diff = (float)Math.Sqrt((sq * sq + (((y - j2) * (y - j2) * 1.5f) + (x - i2) * (x - i2))) * 0.5f * (GetWallDistOffset(x) * 0.0316076058772687986171132238548f + 1));
						if (diff > 35 * sizeMult) {
							continue;
						}
						Main.tile[x, y].ResetToType(fleshID);
						Main.tile[x, y].wall = fleshWallID;
						if (diff < 35 * sizeMult - 5 || ((y - j) * (y - j)) + (x - i) * (x - i) < 25 * sizeMult * sizeMult) {
							Main.tile[x, y].active(false);
							if (diff > 34 * sizeMult - 5 && Main.tile[x, y+1].TileIsType(fleshID)) {
								lesionPlacementSpots.Enqueue(new Point(x, y));
							}
						}
					}
				}
				List<Point> validLesionPlacementSpots = new List<Point>();
                while (lesionPlacementSpots.Count>0) {
					Point current = lesionPlacementSpots.Dequeue();
					if (!Main.tile[current.X, current.Y].active() && !Main.tile[current.X, current.Y - 1].active() && Main.tile[current.X, current.Y + 1].active()) {
						if (!Main.tile[current.X - 1, current.Y].active() && !Main.tile[current.X - 1, current.Y - 1].active() && Main.tile[current.X - 1, current.Y + 1].active()) {
							validLesionPlacementSpots.Add(new Point(current.X - 1, current.Y));
						}
						if (!Main.tile[current.X + 1, current.Y].active() && !Main.tile[current.X + 1, current.Y - 1].active() && Main.tile[current.X + 1, current.Y + 1].active()) {
							validLesionPlacementSpots.Add(new Point(current.X, current.Y));
						}
					}
                }
                for (int index = 0; index < 4; index++) {
                    if (validLesionPlacementSpots.Count < 1 || lesionCount > 18) {
						break;
                    }
					Point current = genRand.Next(validLesionPlacementSpots);

					Place2x2(current.X, current.Y, lesionID, 0);

					lesionCount++;
					validLesionPlacementSpots.Remove(current.OffsetBy(-1));
					validLesionPlacementSpots.Remove(current);
					validLesionPlacementSpots.Remove(current.OffsetBy(1));
				}
				return new Point(i2, j2);
			}
			public static float GetWallDistOffset(float value) {
				float x = value * 0.4f;
				float halfx = x * 0.5f;
				float quarx = x * 0.5f;
				float fx0 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				halfx += 0.5f;
				float fx1 = (float)Math.Min(Math.Pow(halfx % 3, halfx % 5), 2);
				float fx2 = fx0 * (float)(Math.Min(Math.Pow(quarx % 3, quarx % 5), 2) + 0.5f);
				return fx0 - fx2 + fx1;
			}
		}
		public static void CheckLesion(int i, int j, int type) {
			if (destroyObject) {
				return;
			}
            int x = Main.tile[i, j].frameX != 0 ? i - 1 : i;
            int y = Main.tile[i, j].frameY != 0 && Main.tile[i, j].frameY != 36 ? j - 1 : j;
            for (int k = 0; k < 2; k++) {
				for (int l = 0; l < 2; l++) {
					Tile tile = Main.tile[x + k, y + l];
					if (tile != null && (!tile.nactive() || tile.type != type)) {
						destroyObject = true;
						break;
					}
				}
				if (destroyObject) {
					break;
				}
			}
			if (!destroyObject) {
				return;
			}
			for (int m = x; m < x + 2; m++) {
				for (int n = y; n < y + 2; n++) {
					if (Main.tile[m, n].type == type) {
						KillTile(m, n);
					}
				}
			}
			if (Main.netMode != NetmodeID.MultiplayerClient && !noTileActions) {
				if (genRand.Next(2) == 0) {
					spawnMeteor = true;
				}
				int num3 = Main.rand.Next(5);
				if (!shadowOrbSmashed) {
					num3 = 0;
				}
				switch (num3) {
					case 0: {
						Item.NewItem(x * 16, y * 16, 32, 32, 800, 1, pfix:-1);
						int stack = genRand.Next(100, 101);
						Item.NewItem(x * 16, y * 16, 32, 32, 97, stack);
						break;
					}
					case 1:
					Item.NewItem(x * 16, y * 16, 32, 32, 1256, 1, pfix: -1);
					break;
					case 2:
					Item.NewItem(x * 16, y * 16, 32, 32, 802, 1, pfix: -1);
					break;
					case 3:
					Item.NewItem(x * 16, y * 16, 32, 32, 3062, 1, pfix: -1);
					break;
					case 4:
					Item.NewItem(x * 16, y * 16, 32, 32, 1290, 1, pfix: -1);
					break;
				}
				shadowOrbSmashed = true;
				shadowOrbCount++;
				if (shadowOrbCount >= 3) {
					shadowOrbCount = 0;
					float fx = x * 16;
					float fy = y * 16;
					float distance = -1f;
					int plr = 0;
					for (int pindex = 0; pindex < 255; pindex++) {
						float currentDist = Math.Abs(Main.player[pindex].position.X - fx) + Math.Abs(Main.player[pindex].position.Y - fy);
						if (currentDist < distance || distance == -1f) {
							plr = pindex;
							distance = currentDist;
						}
					}
					NPC.SpawnOnPlayer(plr, 1);
				} else {
					LocalizedText localizedText = Lang.misc[10];
					if (shadowOrbCount == 2) {
						localizedText = Lang.misc[11];
					}
					if (Main.netMode == NetmodeID.SinglePlayer) {
						Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
					}else if (Main.netMode == NetmodeID.Server) {
						NetMessage.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), new Color(50, 255, 130));
					}
					AchievementsHelper.NotifyProgressionEvent(7);
				}
			}
			Main.PlaySound(SoundID.NPCKilled, i * 16, j * 16);
			destroyObject = false;
		}
	}
}
