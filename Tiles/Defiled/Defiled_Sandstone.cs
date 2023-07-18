using Microsoft.Xna.Framework;
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
	public class Defiled_Sandstone : OriginTile, DefiledTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = Main.tileMergeDirt[TileID.Sandstone];
			TileID.Sets.Conversion.Sandstone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			//Main.tileMerge[TileID.Sandstone][Type] = true;
			//Main.tileMerge[Type] = Main.tileMerge[TileID.Sandstone];
			//Main.tileMerge[Type][TileID.Sandstone] = true;
			/*for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Sandstone];
            }*/
			ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ItemType<Defiled_Sandstone_Item>();
			AddMapEntry(new Color(150, 150, 150));
			mergeID = TileID.Sandstone;
			AddDefiledTile();
		}
		public override bool CreateDust(int i, int j, ref int type) {
			type = Defiled_Wastelands.DefaultTileDust;
			return true;
		}
	}
	public class Defiled_Sandstone_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Sandstone");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Defiled_Sandstone>();
		}
	}
}
