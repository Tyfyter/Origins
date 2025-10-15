using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using PegasusLib;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Defiled_Regolith : OriginTile, IDefiledTile {
		public string[] Categories => [
            WikiCategories.Stone
        ];
        public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Defiled_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Defiled_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.Stone[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
			TileID.Sets.GeneralPlacementTiles[Type] = false;

			AddMapEntry(new Color(165, 165, 165));
			AddDefiledTile();
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Defiled_Wastelands.DefaultTileDust;
			MinPick = 200;
			MineResist = 4;
		}
		public override bool CanExplode(int i, int j) => false;
	}
	public class Defiled_Regolith_Item : ModItem {
		public override string Texture => typeof(Defiled_Stone_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Defiled_Regolith>());
			Item.color = new(200, 200, 200);
		}
	}
}
