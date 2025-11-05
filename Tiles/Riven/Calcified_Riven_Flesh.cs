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
			Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
			Main.tileMerge[Type][TileID.Stone] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i] || Main.tileSand[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			AddMapEntry(new Color(141, 148, 178));
			MinPick = 65;
			MineResist = 1.5f;
			DustType = Riven_Hive.DefaultTileDust;
		}
	}
	public class Calcified_Riven_Flesh_Item : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Riven_Flesh_Item.Spug_Flesh_Entry).Name;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Calcified_Riven_Flesh>());
		}
	}
}
