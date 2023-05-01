using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Peat_Moss : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			ItemDrop = ItemType<Peat_Moss_Item>();
			AddMapEntry(new Color(18, 160, 56));
			HitSound = SoundID.Dig;
		}
	}
	public class Peat_Moss_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Peat Moss");
			Tooltip.SetDefault("The Demolitionist might find this interesting...");
			SacrificeTotal = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.createTile = TileType<Peat_Moss>();
			Item.maxStack = 999;
			Item.value = Item.buyPrice(silver: 3);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.ExplosivePowder);
			recipe.AddIngredient(this, 3);
			recipe.AddTile(TileID.GlassKiln);
			recipe.Register();
		}
	}
}
