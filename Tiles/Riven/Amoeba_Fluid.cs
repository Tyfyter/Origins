using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Other.Consumables;
using Origins.World.BiomeData;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Amoeba_Fluid : ComplexFrameTile, IRivenTile, IGlowingModTile {
		public string[] Categories => [
			WikiCategories.OtherBlock
		];
		Asset<Texture2D> glowTexture;
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue() + 0.2f;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (OriginsModIntegrations.CheckAprilFools()) color = Vector3.Max(color, new Vector3(0.912f) * GlowValue);
			else color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				glowTexture = Request<Texture2D>(Texture + "_Glow");
				AprilFoolsAssetSwitcher<Asset<Texture2D>>.Add(() => ref glowTexture, Request<Texture2D>(Texture + "_Glow_AF"));
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
			AprilFoolsTextures.AddTile(this);
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Mud_Overlay", TileID.Sets.Mud);
			yield return new TileMergeOverlay(merge + "Spug_Overlay", TileType<Spug_Flesh>());
			yield return new TileMergeOverlay(merge + "Calcified_Overlay", TileType<Calcified_Riven_Flesh>());
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
			this.DrawTileGlow(i, j, spriteBatch);
			base.PostDraw(i, j, spriteBatch);
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Amoeba_Fluid_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			AprilFoolsTextures.AddItem(this);
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
	}
}
