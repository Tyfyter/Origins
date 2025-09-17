using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Marrowick {
	public class Marrowick_Furniture : FurnitureSet<Marrowick_Item> {
		public override Color MapColor => new(245, 225, 143);
		public override int DustType => DustID.TintablePaint;
		public override Vector3 LightColor {
			get {
				Vector3 color = default;
				TorchID.TorchColor(TorchID.Torch, out color.X, out color.Y, out color.Z);
				color.X = 0f;
				return color;
			}
		}
		public override bool LanternSway => false;
		public override bool ChandelierSway => false;
		public override void SetupTile(ModTile tile) {
			if (tile is FurnitureSet_Bookcase) OriginsSets.Tiles.MultitileCollisionOffset[tile.Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if (tile.TileFrameX / 18 != 1) height = -1600;
		}
	}/* left as backup just in case we still need it
	public class Marrowick_Platform : Platform_Tile {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type, 2)
				.AddIngredient<Marrowick_Item>(1)
				.Register();
				Recipe.Create(ModContent.ItemType<Marrowick_Item>())
				.AddIngredient(item.type, 2)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Door : DoorBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Grandfather_Clock : ClockBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddIngredient<Marrowick_Item>(10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Chair : ChairBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Toilet : ChairBase {
		public override int BaseTileID => TileID.Toilets;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
		public override void HitWire(int i, int j) {
			ToiletHitWire(i, j);
		}
	}
	public class Marrowick_Sofa : ChairBase {
		public override int BaseTileID => TileID.Benches;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Bathtub : FurnitureBase {
		public override int BaseTileID => TileID.Bathtubs;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(14)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Sink : FurnitureBase {
		public override int BaseTileID => TileID.Sinks;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddIngredient(ItemID.WaterBucket)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Candle : LightFurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.Candles;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				r = 0f;
			}
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
	}
	public class Marrowick_Candelabra : LightFurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.Candelabras;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsOn(tile)) {
				color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
			}
		}
	}
	public class Marrowick_Lamp : LightFurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.Lamps;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Torch)
				.AddIngredient<Marrowick_Item>(3)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsOn(tile)) {
				color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
			}
		}
	}
	public class Marrowick_Chandelier : LightFurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.Chandeliers;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.Anvils)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsOn(tile)) {
				color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
			}
		}
	}
	public class Marrowick_Lantern : LightFurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.HangingLanterns;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsOn(tile)) {
				color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
			}
		}
	}
	public class Marrowick_Bookcase : FurnitureBase {
		public override int BaseTileID => TileID.Bookcases;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(short frameX, ref float y, ref int height) {
			if (frameX / 18 != 1) height = -1600;
		}
	}
	public class Marrowick_Piano : FurnitureBase {
		public override int BaseTileID => TileID.Pianos;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Table : FurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.Tables;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(8)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
		}
	}
	public class Marrowick_Work_Bench : FurnitureBase, IGlowingModTile {
		public override int BaseTileID => TileID.WorkBenches;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(10)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue());
		}
	}
	public class Marrowick_Dresser : DresserBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(16)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Bed : BedBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Chest : ModChest {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapChestName);
			AdjTiles = [TileID.Containers];
			DustType = DustID.TintablePaint;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
	}
	public class Marrowick_Chest_Item : ModItem {
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Marrowick_Chest>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Marrowick_Item>(8)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}*/
}
