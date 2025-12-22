using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Background_Door : OriginTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this, true).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ModContent.ItemType<Scrap>(), 6)
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
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(new Color(21, 28, 25));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.RandomStyleRange = 6;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
}
