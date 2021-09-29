using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Tyfyter.Utils {
	public static class FramingUtils {
		public static (ushort upLeft, ushort up, ushort upRight, ushort left, ushort right, ushort downLeft, ushort down, ushort downRight) GetAdjacentTileTypes(int i, int j) {
			Tile tile = Main.tile[i, j];

			Tile tileUp = Main.tile[i, j - 1];
			Tile tileDown = Main.tile[i, j + 1];
			Tile tileLeft = Main.tile[i - 1, j];
			Tile tileRight = Main.tile[i + 1, j];
			Tile tileDownLeft = Main.tile[i - 1, j + 1];
			Tile tileDownRight = Main.tile[i + 1, j + 1];
			Tile tileUpLeft = Main.tile[i - 1, j - 1];
			Tile tileUpRight = Main.tile[i + 1, j - 1];
			int upLeft = -1;
			int up = -1;
			int upRight = -1;
			int left = -1;
			int right = -1;
			int downLeft = -1;
			int down = -1;
			int downRight = -1;
			if (tileLeft != null && tileLeft.active()) {
				left = (Main.tileStone[tileLeft.type] ? 1 : tileLeft.type);
				if (tileLeft.slope() == 1 || tileLeft.slope() == 3) {
					left = -1;
				}
			}
			if (tileRight != null && tileRight.active()) {
				right = (Main.tileStone[tileRight.type] ? 1 : tileRight.type);
				if (tileRight.slope() == 2 || tileRight.slope() == 4) {
					right = -1;
				}
			}
			if (tileUp != null && tileUp.active()) {
				up = (Main.tileStone[tileUp.type] ? 1 : tileUp.type);
				if (tileUp.slope() == 3 || tileUp.slope() == 4) {
					up = -1;
				}
			}
			if (tileDown != null && tileDown.active()) {
				down = (Main.tileStone[tileDown.type] ? 1 : tileDown.type);
				if (tileDown.slope() == 1 || tileDown.slope() == 2) {
					down = -1;
				}
			}
			if (tileUpLeft != null && tileUpLeft.active()) {
				upLeft = (Main.tileStone[tileUpLeft.type] ? 1 : tileUpLeft.type);
			}
			if (tileUpRight != null && tileUpRight.active()) {
				upRight = (Main.tileStone[tileUpRight.type] ? 1 : tileUpRight.type);
			}
			if (tileDownLeft != null && tileDownLeft.active()) {
				downLeft = (Main.tileStone[tileDownLeft.type] ? 1 : tileDownLeft.type);
			}
			if (tileDownRight != null && tileDownRight.active()) {
				downRight = (Main.tileStone[tileDownRight.type] ? 1 : tileDownRight.type);
			}
			if (tile.slope() == 2) {
				up = -1;
				left = -1;
			}
			if (tile.slope() == 1) {
				up = -1;
				right = -1;
			}
			if (tile.slope() == 4) {
				down = -1;
				left = -1;
			}
			if (tile.slope() == 3) {
				down = -1;
				right = -1;
			}
			return ((ushort)upLeft, (ushort)up, (ushort)upRight, (ushort)left, (ushort)right, (ushort)downLeft, (ushort)down, (ushort)downRight);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="frameOption"></param>
		/// <param name="mergeDirections">upLeft, up, upRight, left, right, downLeft, down, downRight</param>
		public static void AutoFrame(int i, int j, int frameOption = -1, byte mergeDirections = 0) {
			Tile tile = Main.tile[i, j];
			int type = tile.type;
			int frameX = tile.frameX;
			int frameY = tile.frameY;
			if (frameOption == -1) {
				frameOption = WorldGen.genRand.Next(0, 3);
				tile.frameNumber((byte)frameOption);
			} else {
				frameOption = tile.frameNumber();
			}
			if (Main.tileLargeFrames[type] == 1) {
				int num58 = j % 4;
				int num59 = i % 3;
				frameOption = (new int[4, 3]
				{
				{ 2, 4, 2 },
				{ 1, 3, 1 },
				{ 2, 2, 4 },
				{ 1, 1, 3 }
				})[num58, num59] - 1;
			}
			bool upLeft = (mergeDirections&128)!=0;
			const byte up = 64;
			bool upRight = (mergeDirections &32) != 0;
			const byte left = 16;
			const byte right = 8;
			bool downLeft = (mergeDirections &4) != 0;
			const byte down = 2;
			bool downRight = (mergeDirections &1) != 0;
			switch (mergeDirections) {
				case up | down | left | right: {
					if (!upLeft && !upRight) {
						switch (frameOption) {
							case 0:
							tile.frameX = 108;
							tile.frameY = 18;
							break;
							case 1:
							tile.frameX = 126;
							tile.frameY = 18;
							break;
							default:
							tile.frameX = 144;
							tile.frameY = 18;
							break;
						}
					} else if (!downLeft && !downRight) {
						switch (frameOption) {
							case 0:
							tile.frameX = 108;
							tile.frameY = 36;
							break;
							case 1:
							tile.frameX = 126;
							tile.frameY = 36;
							break;
							default:
							tile.frameX = 144;
							tile.frameY = 36;
							break;
						}
					} else if (!upLeft && !downLeft) {
						switch (frameOption) {
							case 0:
							tile.frameX = 180;
							tile.frameY = 0;
							break;
							case 1:
							tile.frameX = 180;
							tile.frameY = 18;
							break;
							default:
							tile.frameX = 180;
							tile.frameY = 36;
							break;
						}
					} else if (!upRight && !downRight) {
						switch (frameOption) {
							case 0:
							tile.frameX = 198;
							tile.frameY = 0;
							break;
							case 1:
							tile.frameX = 198;
							tile.frameY = 18;
							break;
							default:
							tile.frameX = 198;
							tile.frameY = 36;
							break;
						}
					} else {
						switch (frameOption) {
							case 0:
							tile.frameX = 18;
							tile.frameY = 18;
							break;
							case 1:
							tile.frameX = 36;
							tile.frameY = 18;
							break;
							default:
							tile.frameX = 54;
							tile.frameY = 18;
							break;
						}
					}
				}break;
				case down | left | right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 18;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 36;
						tile.frameY = 0;
						break;
						default:
						tile.frameX = 54;
						tile.frameY = 0;
						break;
					}
				}break;
				case up | left | right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 18;
						tile.frameY = 36;
						break;
						case 1:
						tile.frameX = 36;
						tile.frameY = 36;
						break;
						default:
						tile.frameX = 54;
						tile.frameY = 36;
						break;
					}
				}break;
				case up | down | right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 0;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 0;
						tile.frameY = 18;
						break;
						default:
						tile.frameX = 0;
						tile.frameY = 36;
						break;
					}
				}break;
				case up | down | left: {
					switch (frameOption) {
						case 0:
						tile.frameX = 72;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 72;
						tile.frameY = 18;
						break;
						default:
						tile.frameX = 72;
						tile.frameY = 36;
						break;
					}
				}break;
				case down | right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 0;
						tile.frameY = 54;
						break;
						case 1:
						tile.frameX = 36;
						tile.frameY = 54;
						break;
						default:
						tile.frameX = 72;
						tile.frameY = 54;
						break;
					}
				}break;
				case down | left: {
					switch (frameOption) {
						case 0:
						tile.frameX = 18;
						tile.frameY = 54;
						break;
						case 1:
						tile.frameX = 54;
						tile.frameY = 54;
						break;
						default:
						tile.frameX = 90;
						tile.frameY = 54;
						break;
					}
				}break;
				case up | right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 0;
						tile.frameY = 72;
						break;
						case 1:
						tile.frameX = 36;
						tile.frameY = 72;
						break;
						default:
						tile.frameX = 72;
						tile.frameY = 72;
						break;
					}
				}break;
				case up | left: {
					switch (frameOption) {
						case 0:
						tile.frameX = 18;
						tile.frameY = 72;
						break;
						case 1:
						tile.frameX = 54;
						tile.frameY = 72;
						break;
						default:
						tile.frameX = 90;
						tile.frameY = 72;
						break;
					}
				}break;
				case up | down: {
					switch (frameOption) {
						case 0:
						tile.frameX = 90;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 90;
						tile.frameY = 18;
						break;
						default:
						tile.frameX = 90;
						tile.frameY = 36;
						break;
					}
				}break;
				case left | right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 108;
						tile.frameY = 72;
						break;
						case 1:
						tile.frameX = 126;
						tile.frameY = 72;
						break;
						default:
						tile.frameX = 144;
						tile.frameY = 72;
						break;
					}
				}break;
				case down: {
					switch (frameOption) {
						case 0:
						tile.frameX = 108;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 126;
						tile.frameY = 0;
						break;
						default:
						tile.frameX = 144;
						tile.frameY = 0;
						break;
					}
				}break;
				case up: {
					switch (frameOption) {
						case 0:
						tile.frameX = 108;
						tile.frameY = 54;
						break;
						case 1:
						tile.frameX = 126;
						tile.frameY = 54;
						break;
						default:
						tile.frameX = 144;
						tile.frameY = 54;
						break;
					}
				}break;
				case right: {
					switch (frameOption) {
						case 0:
						tile.frameX = 162;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 162;
						tile.frameY = 18;
						break;
						default:
						tile.frameX = 162;
						tile.frameY = 36;
						break;
					}
				}break;
				case left: {
					switch (frameOption) {
						case 0:
						tile.frameX = 216;
						tile.frameY = 0;
						break;
						case 1:
						tile.frameX = 216;
						tile.frameY = 18;
						break;
						default:
						tile.frameX = 216;
						tile.frameY = 36;
						break;
					}
				}break;
				case 0: {
					switch (frameOption) {
						case 0:
						tile.frameX = 162;
						tile.frameY = 54;
						break;
						case 1:
						tile.frameX = 180;
						tile.frameY = 54;
						break;
						default:
						tile.frameX = 198;
						tile.frameY = 54;
						break;
					}
				}break;
			}
			if (tile.frameX != frameX && tile.frameY != frameY) {
				WorldGen.tileReframeCount++;
				if (WorldGen.tileReframeCount < 55) {
					WorldGen.TileFrame(i - 1, j);
					WorldGen.TileFrame(i + 1, j);
					WorldGen.TileFrame(i, j - 1);
					WorldGen.TileFrame(i, j + 1);
				}
				WorldGen.tileReframeCount--;
			}
		}
	}
}