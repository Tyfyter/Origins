using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Fire_Extinguisher_Holder : OriginTile {
		public static int ID { get; private set; }
		TileItem item;
		public override void Load() {
			Mod.AddContent(item = new TileItem(this).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Gel, 4)
				.AddIngredient(ModContent.ItemType<Scrap>(), 8)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
			}));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

			// Names
			AddMapEntry(FromHexRGB(0xFFB18C), item.DisplayName);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.addTile(Type);
			ID = Type;
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(item.Type);
		}
		static Fire_Extinguisher HeldExtinguisher => Main.LocalPlayer?.HeldItem?.ModItem as Fire_Extinguisher;
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => Main.tile[i, j].TileFrameX != 0 || HeldExtinguisher is not null;
		public override bool RightClick(int i, int j) {
			Tile tile = Main.tile[i, j];
			short targetFrame = 0;
			short originalFrame = tile.TileFrameX;
			if (originalFrame == 0) {
				if (HeldExtinguisher is not Fire_Extinguisher extinguisher) return false;
				targetFrame = (short)(extinguisher.Durability + 1);
				Main.LocalPlayer.HeldItem.TurnToAir();
			}
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, 0);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out int y);
			for (int k = 0; k < data.Width; k++) {
				for (int l = 0; l < data.Height; l++) {
					Framing.GetTileSafely(x + k, y + l).TileFrameX = targetFrame;
				}
			}
			NetMessage.SendTileSquare(
				-1,
				x,
				y,
				data.Width,
				data.Height
			);
			if (targetFrame == 0) {
				int item = Item.NewItem(WorldGen.GetNPCSource_ShakeTree(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Fire_Extinguisher>());
				if (Main.item[item].ModItem is Fire_Extinguisher extinguisher) {
					extinguisher.Durability = originalFrame - 1;
					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
					}
				}
			}
			return base.RightClick(i, j);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			if (tileFrameX == 0) {
				tileFrameX = 16;
			} else {
				tileFrameX = 0;
			}
		}
	}
}
