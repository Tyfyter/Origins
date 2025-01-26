using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Tiles.Defiled;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Brine {
	public class Hydrothermal_Vent : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileLighted[Type] = false;
			Main.tileBlockLight[Type] = false;
			//TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.Origin = new(0, 3);
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, 4).ToArray();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(68, 68, 68), CreateMapEntryName());
			DustType = DustID.GreenMoss;
		}
	}
	public class Hydrothermal_Vent_Item : ModItem, ICustomWikiStat, IItemObtainabilityProvider {
		public IEnumerable<int> ProvideItemObtainability() => [Type];
		public override string Texture => "Origins/Tiles/Brine/Hydrothermal_Vent";
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = Item.CommonMaxStack;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Hydrothermal_Vent>();
		}

		public bool ShouldHavePage => false;
	}
}