using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Amoeba_Fluid : OriginTile, IRivenTile, IGlowingModTile {
        public string[] Categories => [
            "OtherBlock"
        ];
        public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue() + 0.2f;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Amoeba_Fluid_Glow");
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileBouncy[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(0, 200, 200));
			MinPick = 10;
			MineResist = 8f;
			HitSound = SoundID.NPCHit13;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.002f;
			g = 0.015f;
			b = 0.02f;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Amoeba_Fluid_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
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
