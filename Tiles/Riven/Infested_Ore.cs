using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Infested_Ore : OriginTile, IGlowingModTile, IComplexMineDamageTile {
        public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
        public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
        public float GlowValue => (float)(Math.Sin(Main.GlobalTimeWrappedHourly)+2)*0.5f;
        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.Ore[Type] = true;
			ItemDrop = ItemType<Infested_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Encrusted Ore");
			AddMapEntry(new Color(40, 148, 207), name);
            mergeID = TileID.Crimtane;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = 0.02f * GlowValue;
            g = 0.15f * GlowValue;
            b = 0.2f * GlowValue;
        }
        public void MinePower(int i, int j, int minePower, ref int damage) {
            if (minePower >= 55 || j <= Main.worldSurface) {
                damage += (int)(minePower / MineResist);
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            this.DrawTileGlow(i, j, spriteBatch);
        }
    }
    public class Infested_Ore_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Encrusted Ore");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CrimtaneOre);
            Item.createTile = TileType<Infested_Ore>();
		}
    }
}
