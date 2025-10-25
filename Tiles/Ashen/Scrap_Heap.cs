using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Scrap_Heap : ComplexFrameTile, IAshenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x2c212a));
			DustType = DustID.Copper;
			HitSound = SoundID.NPCHit42.WithPitch(1.5f).WithVolume(0.5f);
		}
	}
	public class Scrap_Heap_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ExtractinatorMode[Type] = Type;
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Scrap_Heap>());
		}
		public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) {
			resultType = ModContent.ItemType<Scrap>();
			resultStack = Main.rand.Next(7, 14);
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Scrap>(15)
			.Register();
	}
}
