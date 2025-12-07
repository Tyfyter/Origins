using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Walls;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Marrowick : OriginTile {
		public string[] Categories => [
			"Plant"
		];
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			AddMapEntry(new Color(165, 175, 100));
			mergeID = TileID.WoodBlock;
			//HitSound = SoundID.NPCHit2;
			DustType = DustID.TintablePaint;
		}
	}
	public class Marrowick_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Marrowick>());
		}
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Marrowick_Bow>())
			.AddIngredient(Type, 10)
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ModContent.ItemType<Marrowick_Sword>())
			.AddIngredient(Type, 7)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
