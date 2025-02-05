using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public class Ocotillo : ModTile {
		public static int ID { get; private set; }
		internal static TileObjectData data;
		public override void Unload() => data = null;
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height - 1).Concat([18]).ToArray();
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.RandomStyleRange = 6;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.AnchorValidTiles = [];
			TileObjectData.newTile.Origin = new Point16(1, 4);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 2, 1);
			data = TileObjectData.newTile;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(80, 120, 40), Language.GetOrRegister(this.GetLocalizationKey("DisplayName")));
			ID = Type;
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			yield return new Item(ModContent.ItemType<Ocotillo_Finger>(), 1, -1);
			yield break;
		}
	}
	public class Ocotillo_Item : TestingItem {
		public override string Texture => typeof(Ocotillo).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Ocotillo>());
		}
		public override void AddRecipes() {
			List<int> tiles = [];
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Conversion.Sand[i] || TileID.Sets.Conversion.Sandstone[i] || TileID.Sets.Conversion.HardenedSand[i]) {
					tiles.Add(i);
				}
			}
			Ocotillo.data.AnchorValidTiles = tiles.ToArray();
		}
	}
}
