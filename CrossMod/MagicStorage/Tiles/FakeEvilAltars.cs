using MagicStorage.Stations;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.CrossMod.MagicStorage.Tiles {
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class Evil_Altar_Tile(string texture) : ModTile {
		public override string Texture => $"Origins/Tiles/{texture}";
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = false;
			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 36;
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.CoordinateHeights = [ 16, 16 ];
			TileObjectData.addTile(Type);

			AdjTiles = [ Type, TileID.DemonAltar ];

			AddMapEntry(Color.MediumPurple, CreateMapEntryName());
		}
	}
	public class Fake_Defiled_Altar : Evil_Altar_Tile {
		public Fake_Defiled_Altar() : base("Defiled/Defiled_Altar") { }
	}
	public class Fake_Riven_Altar : Evil_Altar_Tile {
		public Fake_Riven_Altar() : base("Riven/Riven_Altar") { }
	}/*
	public class Fake_Ashen_Altar : Evil_Altar_Tile {
		public Fake_Ashen_Altar() : base("Defiled/Defiled_Altar") { }
	}*/
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Fake_Defiled_Altar_Item : ModItem {
		public override string Texture => base.Texture[..^"_Item".Length];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<DemonAltar>());
			Item.createTile = ModContent.TileType<Fake_Defiled_Altar>();
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Defiled_Bar>(10)
				.AddIngredient<Undead_Chunk>(15)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Fake_Riven_Altar_Item : ModItem {
		public override string Texture => base.Texture[..^"_Item".Length];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<DemonAltar>());
			Item.createTile = ModContent.TileType<Fake_Riven_Altar>();
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Encrusted_Bar>(10)
				.AddIngredient<Riven_Carapace>(15)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}/*
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Fake_Ashen_Altar_Item : ModItem {
		public override string Texture => base.Texture.Replace("Ashen", "Riven")[..^"_Item".Length];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<DemonAltar>());
			Item.createTile = ModContent.TileType<Fake_Ashen_Altar>();
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Sanguinite_Bar>(10)
				.AddIngredient<NE8>(15)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}*/
}
