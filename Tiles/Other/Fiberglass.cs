using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    //current sprites are very unfinished
    public class Fiberglass : OriginTile {
		static AutoCastingAsset<Texture2D>? vineTexture;
		public static AutoCastingAsset<Texture2D>? VineTexture => vineTexture ??= Origins.instance.Assets.Request<Texture2D>("Tiles/Other/Fiber");
		public override void Unload() {
			vineTexture = null;
		}
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = false;
            Main.tileMergeDirt[Type] = false;
			ItemDrop = ItemType<Fiberglass_Item>();
			ModTranslation name = CreateMapEntryName();
			AddMapEntry(new Color(42, 116, 160), name);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			var curr = Framing.GetTileSafely(i, j).Get<TileExtraVisualData>();
			spriteBatch.Draw(VineTexture, new Vector2(i * 16, j * 16) - Main.screenPosition, new Rectangle(curr.TileFrameX * 18, curr.TileFrameY * 18, 16, 16), Lighting.GetColor(i, j));
			return true;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            OriginSystem originWorld = OriginSystem.instance;
            originWorld.AddFiberglassFrameTile(i, j);
			return true;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        }
    }
    public class Fiberglass_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fiberglass");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Glass);
            Item.createTile = TileType<Fiberglass>();
            Item.rare = ItemRarityID.Green;
		}
    }
}
