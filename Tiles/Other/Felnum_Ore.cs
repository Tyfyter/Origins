using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    public class Felnum_Ore : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
			ItemDrop = ItemType<Felnum_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Felnum Ore");
			AddMapEntry(new Color(160, 116, 42), name);
			mergeID = TileID.Demonite;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (!Main.tile[i, j].HasTile) return;
			float v = (float)Math.Sin((Main.time - i) / 45) * 2;
			if (v >= 1.99f && Main.rand.Next(0, 4) == 0) Dust.NewDust(new Vector2(i * 16 - Main.rand.Next(0, 16), j * 16 - Main.rand.Next(0, 16)), 30, 30, DustID.Electric, 0f, 0f, 0, Color.White, 0.5f);//Main.NewText($"{i},{j},{v}");
			if (v < 0) v = 0;
			r = 0.4f - (0.4f * v);
			g = 0.3f + (0.2f * v);
			b = 0.1f + (0.3f * v);
		}
	}
	public class Felnum_Ore_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Ore");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DemoniteOre);
			Item.createTile = TileType<Felnum_Ore>();
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 50);
			recipe.AddIngredient(ItemID.CopperOre);
			recipe.AddIngredient(ItemID.FallenStar, 12);
			recipe.AddIngredient(ItemID.SpellTome);
			recipe.AddTile(TileID.CrystalBall);
			recipe.Register();
		}
	}
}
