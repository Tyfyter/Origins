using MagicStorage.Stations;
using Origins.Items.Materials;
using Origins.Tiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.CrossMod.MagicStorage.Tiles {
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class Evil_Altar_Tile(string texture) : ModTile {
		protected internal Fake_Altar_Item item;
		public override string Texture => $"Origins/Tiles/{texture}";
		public override void Load() {
			Mod.AddContent(item = new Fake_Altar_Item(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
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
	[ExtendsFromMod(nameof(MagicStorage)), Autoload(false)]
	public class Fake_Altar_Item(ModTile tile) : ModItem {
		public override string Name => tile.Name + "_Item";
		public override string Texture => tile.GetType().ToString().Replace(".", "/") + "_Item";
		public event Action<Item> ExtraDefaults;
		public event Action<Item> OnAddRecipes;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 5;
			ModCompatSets.AnyFakeDemonAltars[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<DemonAltar>());
			Item.createTile = tile.Type;
		}
		public override void AddRecipes() {
			if (OnAddRecipes is not null) {
				OnAddRecipes(Item);
				OnAddRecipes = null;
			}
		}
	}
	public class Fake_Defiled_Altar : Evil_Altar_Tile {
		public Fake_Defiled_Altar() : base("Defiled/Defiled_Altar") { }
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Defiled_Bar>(10)
				.AddIngredient<Undead_Chunk>(15)
				.AddTile(TileID.DemonAltar)
				.Register();
			};
		}
	}
	public class Fake_Riven_Altar : Evil_Altar_Tile {
		public Fake_Riven_Altar() : base("Riven/Riven_Altar") { }
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Encrusted_Bar>(10)
				.AddIngredient<Riven_Carapace>(15)
				.AddTile(TileID.DemonAltar)
				.Register();
			};
		}
	}/*
	public class Fake_Ashen_Altar : Evil_Altar_Tile {
		public Fake_Ashen_Altar() : base("Defiled/Defiled_Altar") { }
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Sanguinite_Bar>(10)
				.AddIngredient<NE8>(15)
				.AddTile(TileID.DemonAltar)
				.Register();
			};
		}
	}*/
}
