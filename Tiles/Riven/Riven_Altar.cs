using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Testing;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Riven_Altar : ModTile, IGlowingModTile, IComplexMineDamageTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color.DoFancyGlow(new Vector3(0.394f, 0.879f, 0.912f) * GlowValue, tile.TileColor);
		}
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Riven_Altar_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(20, 136, 182), CreateMapEntryName());
			DustType = Riven_Hive.DefaultTileDust;
			AdjTiles = [TileID.DemonAltar];
			ID = Type;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			Player player = Main.LocalPlayer;
			if (!Main.hardMode || player.HeldItem.hammer < 80) {
				player.Hurt(PlayerDeathReason.ByOther(4), player.statLife / 2, -player.direction);
				damage = 0;
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			WorldGen.SmashAltar(i, j);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			float glowValue = GlowValue * 3f;
			r = 0.02f * glowValue;
			g = 0.1f * glowValue;
			b = 0.2f * glowValue;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override bool CanExplode(int i, int j) => false;
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Riven_Altar_Item : TestingItem {
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Riven_Altar>();
		}
	}
}
