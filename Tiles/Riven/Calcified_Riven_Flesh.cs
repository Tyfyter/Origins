using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Calcified_Riven_Flesh : OriginTile, IRivenTile {
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Riven_Pot>(), 0, 0));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			AddMapEntry(new Color(141, 148, 178));
			MinPick = 65;
			MineResist = 1.5f;
			DustType = Riven_Hive.DefaultTileDust;
		}
		/*public override void RandomUpdate(int i, int j) {
			if (WorldGen.genRand.NextBool((int)(100 * MathHelper.Lerp(151, 151 * 2.8f, MathHelper.Clamp(Main.maxTilesX / 4200f - 1f, 0f, 1f)))) && !TileObject.CanPlace(i, j + 1, TileType<Wrycoral>(), 2, 0, out TileObject objectData, onlyCheck: false, checkStay: true)) {
				TileObject.Place(objectData);
				//Main.LocalPlayer.Teleport(new Vector2(i, j).ToWorldCoordinates(), 1);
			}
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) {
					above.ResetToType((ushort)ModContent.TileType<Acetabularia>());
				} else {
					above.ResetToType((ushort)ModContent.TileType<Riven_Foliage>());
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}*/
	}
	public class Calcified_Riven_Flesh_Item : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Riven_Flesh_Item.Spug_Flesh_Entry).Name;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Calcified_Riven_Flesh>());
		}
	}
}
