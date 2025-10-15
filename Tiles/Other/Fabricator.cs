using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public class Fabricator : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(1f, 0.5960784313725490196078431372549f, 0.5960784313725490196078431372549f) * 0.25f);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileBlockLight[Type] = false;
			//TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Width = 4;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(68, 68, 68), CreateMapEntryName());
			DustType = DustID.Lead;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 4) {
				frameCounter = 0;
				frame = (frame + 1) % 6;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			frameYOffset = Main.tileFrame[type] * 54;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.05f;
			g = 0.025f;
			b = 0.025f;
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Fabricator_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Furniture,
			WikiCategories.CraftingStation
		];
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Fabricator>());
			Item.value = Item.buyPrice(platinum: 2);
			Item.rare = ItemRarityID.LightPurple;
		}
	}
}