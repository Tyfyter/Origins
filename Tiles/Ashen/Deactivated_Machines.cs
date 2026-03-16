using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Deactivated_Trenchmaker : OriginTile, IMultiTypeMultiTile {
		public static int ID { get; private set; }
		public static bool[,,] Shape = MultiTypeMultiTile.GenerateShapeMap(
			"     XX  |XXXX XX  |XXXX XX  ",
			"XXXXXXXXX|XXXXXXXX |XXXXXXXXX",
			"XXXXXXXXX|XXXXXXXXX|XXXXXXXXX",
			"XXXXXXXXX|XXXXXXXXX|XXXXXXXXX",
			"XXXXXXXXX|XXXXXXXX |XXXXXXXXX",
			" XXXXXXX | XXXXXXX | XXXXXXX ",
			" XXXXXXX | XXXXXXX | XXXXXXX ",
			" XXX XXX | XXX XXX | XXX XXX ",
			" XXX XXX | XXX XXX | XXX XXX "
		);
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			// Names
			AddMapEntry(new Color(154, 56, 11));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 9;
			TileObjectData.newTile.SetHeight(9);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.HookPlaceOverride = MultiTypeMultiTile.PlaceWhereTrue(IsPart);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			ID = Type;
			RegisterItemDrop(ModContent.ItemType<Deactivated_Trenchmaker_Item>(), -1);
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			if (!IsPart((tile.TileFrameX / 18) % Shape.GetLength(1), tile.TileFrameY / 18, TileObjectData.GetTileStyle(tile))) {
				tile.HasTile = false;
				return false;
			}
			if (tile.TileFrameY >= (Shape.GetLength(2) - 1) * 18) {
				Tile anchor = Main.tile[i, j + 1];
				if (!anchor.HasTile || !Main.tileSolid[anchor.TileType] || Main.tileSolidTop[anchor.TileType] || Main.tileNoAttach[anchor.TileType]) {
					WorldGen.KillTile(i, j);
					return false;
				}
			}
			return true;
		}
		public static bool IsPart(int i, int j, int style) {
			return Shape[style, i, j];
		}
		public bool IsValidTile(Tile tile, int left, int top, int style) {
			(int i, int j) = tile.GetTilePosition();
			i -= left;
			j -= top;
			if (IsPart(i, j, style)) {
				if (tile.TileType != Type) return false;
				Tile topLeft = Main.tile[left, top];
				return tile.TileFrameX == topLeft.TileFrameX + i * 18 && tile.TileFrameY == topLeft.TileFrameY + j * 18;
			}
			return true;
		}
		public bool CanBlockPlacement(Tile tile, int left, int top, int style) {
			if (!tile.HasTile) return false;
			Point pos = tile.GetTilePosition();
			return IsPart(pos.X - left, pos.Y - top, style);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => IsPart(x - left, y - top, style);
	}
	public class Deactivated_Trenchmaker_Side : OriginTile, IMultiTypeMultiTile {
		public static int ID { get; private set; }
		public static bool[,,] Shape = MultiTypeMultiTile.GenerateShapeMap(
			"  XXX XX|  XXX XX|        |XX XXX  |XX XXX  |        ",
			"XXXXXXXX|XXXXXXXX|XXXXXXX |XXXXXXXX|XXXXXXXX| XXXXXXX",
			"XXXXXXXX|XXXXXXXX|XXXXXXX |XXXXXXXX|XXXXXXXX| XXXXXXX",
			"XXXXXXXX|XXXXXXXX|XXXXXXX |XXXXXXXX|XXXXXXXX| XXXXXXX",
			"XXXXXXX |XXXXXXX |XXXXXXX | XXXXXXX| XXXXXXX| XXXXXXX",
			"XXXXXX  |XXXXXX  |XXXXXX  |  XXXXXX|  XXXXXX|  XXXXXX",
			"XXXXX   |XXXXX   |XXXXX   |   XXXXX|   XXXXX|   XXXXX",
			" XXXX   | XXXX   | XXXX   |   XXXX |   XXXX |   XXXX ",
			" XXXX   | XXXX   | XXXX   |   XXXX |   XXXX |   XXXX "
		);
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			// Names
			AddMapEntry(new Color(154, 56, 11));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 8;
			TileObjectData.newTile.SetHeight(9);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.HookPlaceOverride = MultiTypeMultiTile.PlaceWhereTrue(IsPart);
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile, 4, 1);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newAlternate.AnchorBottom = new(AnchorType.SolidTile, 4, 3);
			TileObjectData.addAlternate(3);
			TileObjectData.addTile(Type);
			ID = Type;
			RegisterItemDrop(ModContent.ItemType<Deactivated_Trenchmaker_Item>(), -1);
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public static bool IsPart(int i, int j, int style) {
			return Shape[style, i, j];
		}
		public bool IsValidTile(Tile tile, int left, int top, int style) {
			(int i, int j) = tile.GetTilePosition();
			i -= left;
			j -= top;
			if (IsPart(i, j, style)) {
				if (tile.TileType != Type) return false;
				Tile topLeft = Main.tile[left, top];
				return tile.TileFrameX == topLeft.TileFrameX + i * 18 && tile.TileFrameY == topLeft.TileFrameY + j * 18;
			}
			return true;
		}
		public bool CanBlockPlacement(Tile tile, int left, int top, int style) {
			if (!tile.HasTile) return false;
			Point pos = tile.GetTilePosition();
			return IsPart(pos.X - left, pos.Y - top, style);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => IsPart(x - left, y - top, style);
	}
	public class Deactivated_Trenchmaker_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(Deactivated_Trenchmaker.ID);
			Item.width = 14;
			Item.height = 28;
			Item.value = 150;
		}
		public override void HoldItem(Player player) {
			if (PlayerInput.Triggers.JustPressed.Up || PlayerInput.Triggers.JustPressed.Down) {
				if (Item.createTile == Deactivated_Trenchmaker.ID) Item.createTile = Deactivated_Trenchmaker_Side.ID;
				else Item.createTile = Deactivated_Trenchmaker.ID;
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Biocomponent10>(15)
			.AddIngredient<Scrap>(45)
			.AddTile<Metal_Presser>()
			.Register();
		}
	}
	public class Deactivated_Repairboy : OriginTile, IMultiTypeMultiTile {
		public static int ID { get; private set; }
		public static bool[,,] Shape = MultiTypeMultiTile.GenerateShapeMap(
			"  X|  X|  X|X  |X  |X  ",
			"XXX|XXX|XXX|XXX|XXX|XXX",
			"XXX|XXX|XXX|XXX|XXX|XXX"
		);
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(item => {
				this.DropTileItem(item);
				item.ResearchUnlockCount = 25;
			}).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient<Biocomponent10>(3)
				.AddIngredient<Scrap>(8)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;

			// Names
			AddMapEntry(new Color(154, 56, 11));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.SetHeight(3);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.HookPlaceOverride = MultiTypeMultiTile.PlaceWhereTrue(IsPart);
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(3);
			TileObjectData.addTile(Type);
			ID = Type;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public static bool IsPart(int i, int j, int style) {
			return Shape[style, i, j];
		}
		public bool IsValidTile(Tile tile, int left, int top, int style) {
			(int i, int j) = tile.GetTilePosition();
			i -= left;
			j -= top;
			if (IsPart(i, j, style)) {
				if (tile.TileType != Type) return false;
				Tile topLeft = Main.tile[left, top];
				return tile.TileFrameX == topLeft.TileFrameX + i * 18 && tile.TileFrameY == topLeft.TileFrameY + j * 18;
			}
			return true;
		}
		public bool CanBlockPlacement(Tile tile, int left, int top, int style) {
			if (!tile.HasTile) return false;
			Point pos = tile.GetTilePosition();
			return IsPart(pos.X - left, pos.Y - top, style);
		}
		public bool ShouldBreak(int x, int y, int left, int top, int style) => IsPart(x - left, y - top, style);
	}
}
