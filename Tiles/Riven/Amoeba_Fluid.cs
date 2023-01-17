using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Amoeba_Fluid : OriginTile, RivenTile, IGlowingModTile {
        public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
        public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
        public float GlowValue => (float)(Math.Sin(Main.GlobalTimeWrappedHourly) + 2) * 0.5f;
        public override void SetStaticDefaults() {
			if (!Main.dedServ) {
                GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Amoeba_Fluid_Glow");
            }
            Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			ItemDrop = ItemType<Riven_Flesh_Item>();
			AddMapEntry(new Color(0, 200, 200));
            MinPick = 10;
            MineResist = 1f;
        }
        bool recursion = false;
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            if (recursion) {
                return true;
            }
            recursion = true;
            WorldGen.TileFrame(i, j, resetFrame, noBreak);
            recursion = false;
            if ((!WorldGen.genRand.NextBool(Main.tile[i, j].TileFrameX == 54 ? 12 : 8)) || CheckOtherTilesGlow(i, j)) {
                Main.tile[i, j].TileFrameY += 90;
            }
            return false;
        }
        bool CheckOtherTilesGlow(int i, int j) {
			if (Main.tile[i + 1, j].TileIsType(Type) && Main.tile[i + 1, j].TileFrameY > 90) {
                return true;
            }
            if (Main.tile[i - 1, j].TileIsType(Type) && Main.tile[i + 1, j].TileFrameY > 90) {
                return true;
            }
            if (Main.tile[i, j + 1].TileIsType(Type) && Main.tile[i + 1, j].TileFrameY > 90) {
                return true;
            }
            if (Main.tile[i, j - 1].TileIsType(Type) && Main.tile[i + 1, j].TileFrameY > 90) {
                return true;
            }
            return false;
		}
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            this.DrawTileGlow(i, j, spriteBatch);
        }
		public override void RandomUpdate(int i, int j) {
            if (Main.rand.NextBool((int)(100 * MathHelper.Lerp(151, 151 * 2.8f, MathHelper.Clamp(Main.maxTilesX / 4200f - 1f, 0f, 1f)))) && !TileObject.CanPlace(i, j + 1, TileType<Wrycoral>(), 2, 0, out TileObject objectData, onlyCheck: false, checkStay: true)) {
                TileObject.Place(objectData);
            }
        }
	}
    public class Amoeba_Fluid_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Amoeba Fluid");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.FleshBlock);
            Item.createTile = TileType<Amoeba_Fluid>();
		}
    }
}
