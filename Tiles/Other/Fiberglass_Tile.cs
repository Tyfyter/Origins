using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    //current sprites are very unfinished
    public class Fiberglass_Tile : OriginTile {
		static AutoCastingAsset<Texture2D>? vineTexture;
		public static AutoCastingAsset<Texture2D>? VineTexture => vineTexture ??= Origins.instance.Assets.Request<Texture2D>("Tiles/Other/Fiberglass_Vines");
		public override void Unload() {
			vineTexture = null;
		}
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = false;
			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileMergeDirt[Type] = false;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(42, 116, 160), name);
            HitSound = SoundID.Shatter;
        }
		/*public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			var curr = Framing.GetTileSafely(i, j).Get<TileExtraVisualData>();
			//spriteBatch.Draw(VineTexture, new Vector2((i + 12) * 16, (j + 12) * 16) - Main.screenPosition, new Rectangle(curr.TileFrameX * 18, curr.TileFrameY * 18, 16, 16), Lighting.GetColor(i, j));
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			OriginSystem originWorld = OriginSystem.Instance;
			originWorld.AddFiberglassFrameTile(i, j);
			return true;
		}*/
	}
	public class Fiberglass_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Glass);
			Item.createTile = TileType<Fiberglass_Tile>();
		}
	}
}
