using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Riven_Altar : ModTile, IGlowingModTile, IComplexMineDamageTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => (float)(Math.Sin(Main.GlobalTimeWrappedHourly) + 2) * 0.5f;
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
			TileObjectData.newTile.CoordinateHeights = new[] { 18, 18 };
			TileObjectData.addTile(Type);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Riven Altar");
			AddMapEntry(new Color(20, 136, 182), name);
			//disableSmartCursor = true;
			AdjTiles = new int[] { TileID.DemonAltar };
			ID = Type;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			Player player = Main.LocalPlayer;
			if (Main.hardMode && player.HeldItem.hammer >= 80) {
				damage = (int)(1.2f * minePower);
			} else {
				player.Hurt(PlayerDeathReason.ByOther(4), player.statLife / 2, -player.direction);
			}
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.02f * GlowValue;
			g = 0.1f * GlowValue;
			b = 0.2f * GlowValue;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			WorldGen.SmashAltar(i, j);
		}
		public override bool CanExplode(int i, int j) {
			return base.CanExplode(i, j);
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
	}
}
