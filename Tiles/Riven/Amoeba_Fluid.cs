using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Amoeba_Fluid : ComplexFrameTile, IRivenTile, IGlowingModTile {
		public string[] Categories => [
			"OtherBlock"
		];
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue() + 0.2f;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (OriginsModIntegrations.CheckAprilFools()) color = Vector3.Max(color, new Vector3(0.912f) * GlowValue);
			else color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Amoeba_Fluid_Glow");
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileBouncy[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(0, 200, 200));
			MinPick = 10;
			MineResist = 3f;
			HitSound = SoundID.NPCHit13;
			DustType = DustID.Water_Desert;
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay("Origins/Tiles/Riven/Mud_Overlay", TileID.Sets.Mud);
			yield return new TileMergeOverlay(Texture + "_Spug_Overlay", TileType<Spug_Flesh>());
			yield return new TileMergeOverlay(Texture + "_Calcified_Overlay", TileType<Calcified_Riven_Flesh>());
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				r = 0.02f;
				g = 0.02f;
				b = 0.02f;
			} else {
				r = 0.002f;
				g = 0.015f;
				b = 0.02f;
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return true;
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Amoeba_Fluid).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Amoeba_Fluid).GetDefaultTMLName() + "_AF";
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Tile[Type] = afTexture;
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Amoeba_Fluid_Glow_AF");
			} else {
				TextureAssets.Tile[Type] = normalTexture;
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Amoeba_Fluid_Glow");
			}
			this.DrawTileGlow(i, j, spriteBatch);
			base.PostDraw(i, j, spriteBatch);
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Amoeba_Fluid_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Amoeba_Fluid>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 10)
			.AddIngredient(ItemType<Gooey_Water>())
			.AddTile(TileID.HeavyWorkBench)
			.Register();
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Amoeba_Fluid_Item).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Amoeba_Fluid_Item).GetDefaultTMLName() + "_AF";
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (OriginsModIntegrations.CheckAprilFools()) TextureAssets.Item[Type] = afTexture;
			else TextureAssets.Item[Type] = normalTexture;
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			if (OriginsModIntegrations.CheckAprilFools()) TextureAssets.Item[Type] = afTexture;
			else TextureAssets.Item[Type] = normalTexture;
		}
	}
}
