using Microsoft.Xna.Framework;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Marrowick {
    public class Marrowick_Door : DoorBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
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
		}
	}
	public class Marrowick_Chair : ChairBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
	}
	public class Marrowick_Toilet : ChairBase {
		public override int BaseTileID => TileID.Toilets;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void HitWire(int i, int j) {
			ToiletHitWire(i, j);
		}
	}
	public class Marrowick_Sofa : ChairBase {
		public override int BaseTileID => TileID.Benches;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Marrowick_Bathtub : FurnitureBase {
		public override int BaseTileID => TileID.Bathtubs;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(14)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Marrowick_Sink : FurnitureBase {
		public override int BaseTileID => TileID.Sinks;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddIngredient(ItemID.WaterBucket)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
	}
	public class Marrowick_Candle : LightFurnitureBase {
		public override int BaseTileID => TileID.Candles;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
				r = 0f;
			}
		}
	}
	public class Marrowick_Candelabra : LightFurnitureBase {
		public override int BaseTileID => TileID.Candelabras;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
        }
    }
	public class Marrowick_Lamp : LightFurnitureBase {
		public override int BaseTileID => TileID.Lamps;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Torch)
				.AddIngredient<Marrowick_Item>(3)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
        }
    }
	public class Marrowick_Chandelier : LightFurnitureBase {
		public override int BaseTileID => TileID.Chandeliers;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.Anvils)
				.Register();
			};
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
        }
    }
	public class Marrowick_Lantern : LightFurnitureBase {
		public override int BaseTileID => TileID.HangingLanterns;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(6)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            if (IsOn(Main.tile[i, j])) {
                TorchID.TorchColor(TorchID.Torch, out r, out g, out b);
                r = 0f;
            }
        }
    }
	public class Marrowick_Bookcase : FurnitureBase {
		public override int BaseTileID => TileID.Bookcases;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Marrowick_Piano : FurnitureBase {
		public override int BaseTileID => TileID.Pianos;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Marrowick_Table : FurnitureBase {
		public override int BaseTileID => TileID.Tables;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(8)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
	}
	public class Marrowick_Work_Bench : FurnitureBase {
		public override int BaseTileID => TileID.WorkBenches;
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(10)
				.Register();
			};
		}
	}
	public class Marrowick_Dresser : DresserBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(16)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Marrowick_Bed : BedBase {
		public override Color MapColor => new(245, 225, 143);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Marrowick_Item>(15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Marrowick_Chest : ModChest {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapChestName);
			AdjTiles = new int[] { TileID.Containers };
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
	}
	public class Marrowick_Chest_Item : ModItem {
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
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
	}
}
