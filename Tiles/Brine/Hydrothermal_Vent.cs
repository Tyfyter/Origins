using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Tiles.Defiled;
using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
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

			AddMapEntry(new Color(18, 73, 56), CreateMapEntryName());
			DustType = DustID.GreenMoss;
		}
		public override void RandomUpdate(int i, int j) {
			if (!NPC.downedGolemBoss) return;
			Tile self = Framing.GetTileSafely(i, j);
			if (self.TileFrameX % 36 == 0 && self.TileFrameY == 0) {
				Transform(i, j, (ushort)ModContent.TileType<Hydrothermal_Vent_Goopy>());
			}
		}
		protected static bool Transform(int i, int j, ushort toType) {
			Tile tile = Main.tile[i, j];
			int style = TileObjectData.GetTileStyle(tile);
			if (style < 0) return false;
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, style);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out int y);
			for (int k = 0; k < data.Width; k++) {
				for (int l = 0; l < data.Height; l++) {
					Framing.GetTileSafely(x + k, y + l).TileType = toType;
				}
			}
			return true;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (Framing.GetTileSafely(i, j).TileFrameY == 0 && Main.rand.NextFloat(1000) < Main.gfxQuality * 2f) {
				Gore.NewGore(Entity.GetSource_None(), new Vector2(i + Main.rand.Next(2), j + Main.rand.Next(2)) * 16, default, GoreID.ChimneySmoke1 + Main.rand.Next(3));
			}
		}
	}
	public class Hydrothermal_Vent_Goopy : Hydrothermal_Vent {
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

			AddMapEntry(new Color(56, 73, 56), CreateMapEntryName());
			DustType = DustID.GreenMoss;

		}
		public override void RandomUpdate(int i, int j) { }
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) { }
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => Main.LocalPlayer.HeldItem is not { pick: > 0 };
		public override bool RightClick(int i, int j) {
			if (Main.LocalPlayer.HeldItem is not { pick: > 0 }) return false; // null check & pick power check
			if (!Transform(i, j, (ushort)ModContent.TileType<Hydrothermal_Vent>())) return false;
			int item = Item.NewItem(WorldGen.GetNPCSource_ShakeTree(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Baryte_Item>(), Main.rand.Next(6, 11));
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
			}
			return true;
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