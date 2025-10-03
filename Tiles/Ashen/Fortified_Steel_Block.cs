using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Fortified_Steel_Block : OriginTile, IAshenTile {
		public string[] Categories => [
            "Stone"
        ];
        public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.Stone[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;/*
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
			Main.tileMerge[Type][TileID.Stone] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
			}*/
			AddMapEntry(new Color(255, 200, 200));
			//mergeID = TileID.Stone;
			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
	public class Fortified_Steel_Block2 : Fortified_Steel_Block {
		public override string Texture => base.Texture.Replace("2", "1");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			MinPick = 100;
		}
	}
	public class Fortified_Steel_Block3 : Fortified_Steel_Block {
		public override string Texture => base.Texture.Replace("3", "1");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			MinPick = 210;
		}
	}
	public class Fortified_Steel_Block1_Item : ModItem, ICustomWikiStat {
		public virtual int MinePower => 65;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fortified_Steel_Block>());
		}
		public LocalizedText PageTextMain => WikiPageExporter.GetDefaultMainPageText(this)
			.WithFormatArgs(MinePower,
			Language.GetText("Mods.Origins.Generic.Ashen_Factory"),
			"Stone"
		);
	}
	public class Fortified_Steel_Block2_Item : Fortified_Steel_Block1_Item {
		public override string Texture => base.Texture.Replace("2", "1");
		public override int MinePower => 100;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fortified_Steel_Block2>());
		}
	}
	public class Fortified_Steel_Block3_Item : Fortified_Steel_Block1_Item {
		public override string Texture => base.Texture.Replace("3", "1");
		public override int MinePower => 210;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fortified_Steel_Block2>());
		}
	}
}
