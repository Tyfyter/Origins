using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Tool_Rack : OriginTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.CopperBar)
				.AddIngredient(ModContent.ItemType<Scrap>(), 6)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
				Recipe.Create(item.type)
				.AddIngredient(ItemID.TinBar)
				.AddIngredient(ModContent.ItemType<Scrap>(), 6)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.Register();
			}).WithExtraStaticDefaults(i => RegisterItemDrop(i.type, -1)));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(new Color(21, 28, 25));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.SetHeight(2);
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			AnimationFrameHeight = 36;
		}
	}
	public class Tool_Rack_Table : GlobalTile {
		public override int[] AdjTiles(int type) {
			bool[] adjTile = Main.LocalPlayer.adjTile;
			if (adjTile[TileID.WorkBenches]) return [];
			if (adjTile[TileID.Tables] && adjTile[ModContent.TileType<Tool_Rack>()]) return [TileID.WorkBenches];
			return [];
		}
	}
}
