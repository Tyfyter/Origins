using CalamityMod.NPCs.TownNPCs;
using MonoMod.Cil;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Truss_Block : OriginTile, IAshenTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
			On_Collision.SlopeCollision += On_Collision_SlopeCollision;
			On_Collision.StepUp += On_Collision_StepUp;
		}

		Vector4 On_Collision_SlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, float gravity, bool fall) {
			try {
				TileID.Sets.Platforms[Type] = true;
				Vector4 ret = orig(Position, Velocity, Width, Height, gravity, fall);
				TileID.Sets.Platforms[Type] = false;
				return ret;
			} catch {
				TileID.Sets.Platforms[Type] = false;
				throw;
			}
		}
		void On_Collision_StepUp(On_Collision.orig_StepUp orig, ref Vector2 position, ref Vector2 velocity, int width, int height, ref float stepSpeed, ref float gfxOffY, int gravDir, bool holdsMatching, int specialChecksMode) {
			try {
				TileID.Sets.Platforms[Type] = true;
				orig(ref position, ref velocity, width, height, ref stepSpeed, ref gfxOffY, gravDir, holdsMatching, specialChecksMode);
				TileID.Sets.Platforms[Type] = false;
			} catch {
				TileID.Sets.Platforms[Type] = false;
				throw;
			}
		}

		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.CanBeSloped[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			OriginsSets.Tiles.DisableHoiking[Type] = true;
			AddMapEntry(FromHexRGB(0xBE9170));

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = DustID.Iron;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
		}
		public override bool Slope(int i, int j) {
			SlopeType first = SlopeType.SlopeDownLeft;
			SlopeType second = SlopeType.SlopeDownRight;
			if ((Framing.GetTileSafely(i + 1, j).TileIsType(Type) || Main.tile[i + 1, j].Slope == SlopeType.SlopeDownLeft || Main.tile[i + 1, j].Slope == SlopeType.SlopeUpLeft) && !Framing.GetTileSafely(i - 1, j).TileIsType(Type)) {
				first = SlopeType.SlopeDownRight;
				second = SlopeType.SlopeDownLeft;
			}
			if (Framing.GetTileSafely(i, j - 1).TileIsType(Type) && !Framing.GetTileSafely(i, j + 1).TileIsType(Type)) {
				if (Main.tile[i, j].Slope == SlopeType.Solid) {
					WorldGen.SlopeTile(i, j, (int)first + 2);
				} else if (Main.tile[i, j].Slope == first + 2) {
					WorldGen.SlopeTile(i, j, (int)second + 2);
				} else if (Main.tile[i, j].Slope == second + 2) {
					WorldGen.SlopeTile(i, j, (int)first);
				} else if (Main.tile[i, j].Slope == first) {
					WorldGen.SlopeTile(i, j, (int)second);
				} else {
					WorldGen.SlopeTile(i, j);
				}
			} else {
				if (Main.tile[i, j].Slope == SlopeType.Solid) {
					WorldGen.SlopeTile(i, j, (int)first);
				} else if (Main.tile[i, j].Slope == first) {
					WorldGen.SlopeTile(i, j, (int)second);
				} else if (Main.tile[i, j].Slope == second) {
					WorldGen.SlopeTile(i, j, (int)first + 2);
				} else if (Main.tile[i, j].Slope == first + 2) {
					WorldGen.SlopeTile(i, j, (int)second + 2);
				} else {
					WorldGen.SlopeTile(i, j);
				}
			}
			if (NetmodeActive.MultiplayerClient) {
				NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, i, j, (int)Main.tile[i, j].Slope);
			}
			return false;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
				case (1, 0):
				case (2, 0):
				case (3, 0):

				case (1, 2):
				case (2, 2):
				case (3, 2):

				case (6, 0):
				case (7, 0):
				case (8, 0):
				case (6, 1):
				case (7, 1):
				case (8, 1):
				case (6, 2):
				case (7, 2):
				case (8, 2):

				case (6, 4):
				case (7, 4):
				case (8, 4):

				case (9, 0):
				case (9, 1):
				case (9, 2):

				case (12, 0):
				case (12, 1):
				case (12, 2):

				case (9, 3):
				case (10, 3):
				case (11, 3):

				case (0, 3):
				case (1, 3):
				case (2, 3):
				case (3, 3):
				case (4, 3):
				case (5, 3):
				break;

				default:
				height = -1600;
				break;
			}
		}
	}
}
