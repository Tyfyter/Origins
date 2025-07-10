using Microsoft.Xna.Framework;
using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Endowood {
	public class Endowood_Platform : Platform_Tile {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type, 2)
				.AddIngredient<Endowood_Item>(1)
				.Register();
				Recipe.Create(ModContent.ItemType<Endowood_Item>())
				.AddIngredient(item.type, 2)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Door : DoorBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(6)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Grandfather_Clock : ClockBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddIngredient<Endowood_Item>(10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Chair : ChairBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Toilet : ChairBase {
		public override int BaseTileID => TileID.Toilets;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(6)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
		public override void HitWire(int i, int j) {
			ToiletHitWire(i, j);
		}
	}
	public class Endowood_Sofa : ChairBase {
		public override int BaseTileID => TileID.Benches;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Bathtub : FurnitureBase {
		public override int BaseTileID => TileID.Bathtubs;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(14)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Sink : FurnitureBase {
		public override int BaseTileID => TileID.Sinks;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(6)
				.AddIngredient(ItemID.WaterBucket)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Candle : LightFurnitureBase {
		public override int BaseTileID => TileID.Candles;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				g *= 0.8f;
				b *= 0.6f;
			}
		}
	}
	public class Endowood_Candelabra : LightFurnitureBase {
		public override int BaseTileID => TileID.Candelabras;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				g *= 0.8f;
				b *= 0.6f;
			}
		}
	}
	public class Endowood_Lamp : LightFurnitureBase {
		public override int BaseTileID => TileID.Lamps;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Torch)
				.AddIngredient<Endowood_Item>(3)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				g *= 0.8f;
				b *= 0.6f;
			}
		}
	}
	public class Endowood_Chandelier : LightFurnitureBase {
		public override int BaseTileID => TileID.Chandeliers;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.Anvils)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				g *= 0.8f;
				b *= 0.6f;
			}
		}
	}
	public class Endowood_Lantern : LightFurnitureBase {
		public override int BaseTileID => TileID.HangingLanterns;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(6)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				g *= 0.8f;
				b *= 0.6f;
			}
		}
	}
	public class Endowood_Bookcase : FurnitureBase {
		public override int BaseTileID => TileID.Bookcases;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Piano : FurnitureBase {
		public override int BaseTileID => TileID.Pianos;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Table : FurnitureBase {
		public override int BaseTileID => TileID.Tables;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(8)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Work_Bench : FurnitureBase {
		public override int BaseTileID => TileID.WorkBenches;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(10)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Dresser : DresserBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(16)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Bed : BedBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.Sawmill)
				.Register();
			};
			DustType = DustID.t_Granite;
		}
	}
	public class Endowood_Chest : ModChest {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapChestName);
			AdjTiles = [TileID.Containers];
			DustType = DustID.t_Granite;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
	}
	public class Endowood_Chest_Item : ModItem {
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
			Item.createTile = ModContent.TileType<Endowood_Chest>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Endowood_Item>(8)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
