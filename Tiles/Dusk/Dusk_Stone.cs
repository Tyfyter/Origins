using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dusk {
	public class Dusk_Stone : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			ItemDrop = ItemType<Dusk_Stone_Item>();
			AddMapEntry(new Color(0, 0, 0));
		}
		public override void PostSetDefaults() {
			Main.tileNoSunLight[Type] = true;
		}
	}
	public class Dusk_Stone_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dusk Stone");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Dusk_Stone>();
		}
	}
}
