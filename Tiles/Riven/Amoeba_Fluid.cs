using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Amoeba_Fluid : OriginTile, RivenTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => (float)(Math.Sin(Main.GlobalTimeWrappedHourly) + 2) * 0.5f;
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Amoeba_Fluid_Glow");
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(0, 200, 200));
			MinPick = 10;
			MineResist = 8f;
			HitSound = SoundID.NPCHit13;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
	}
	public class Amoeba_Fluid_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amoeba Fluid");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FleshBlock);
			Item.createTile = TileType<Amoeba_Fluid>();
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 10);
			recipe.AddIngredient(ModContent.ItemType<Gooey_Water>());
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
}
