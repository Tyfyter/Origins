using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Fiberglass_Tile : OriginTile {
		static AutoCastingAsset<Texture2D>? vineTexture;
        public string[] Categories => [
            "OtherBlock"
        ];
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
			AddMapEntry(new Color(42, 116, 160));
			DustType = DustID.Everscream;
			HitSound = SoundID.Shatter;
		}
		public override bool KillSound(int i, int j, bool fail) {
			if (!fail) SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i * 16, j * 16));
			return fail;
		}
	}
	public class Fiberglass_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Fiberglass_Tile>());
		}
	}
}
