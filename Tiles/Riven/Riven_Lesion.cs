using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.Localization;
using Origins.Items.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace Origins.Tiles.Riven {
    public class Riven_Lesion : ModTile, IGlowingModTile {
        public Texture2D GlowTexture { get; private set; }
        public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
        public float GlowValue => (float)(Math.Sin(Main.GlobalTime) + 2) * 0.5f;
        public override void SetDefaults() {
            if (Main.netMode != NetmodeID.Server) {
                GlowTexture = mod.GetTexture("Tiles/Riven/Riven_Lesion_Glow");
            }
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileValue[Type] = 500;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.AnchorInvalidTiles = new[] { (int)TileID.MagicalIceBlock };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Riven Lesion");
            AddMapEntry(new Color(217, 95, 54), name);
			adjTiles = new int[] { TileID.ShadowOrbs };
            soundType = SoundID.NPCKilled;
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            if (noBreak) {
                return true;
            }
            World.BiomeData.RivenHive.CheckLesion(i, j, Type);
            return true;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            this.DrawTileGlow(i, j, spriteBatch);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = 0.05f * GlowValue;
            g = 0.0375f * GlowValue;
            b = 0.015f * GlowValue;
        }
    }
    public class Riven_Lesion_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Lesion (Debugging Item)");
        }
        public override void SetDefaults() {
            item.width = 26;
            item.height = 22;
            item.maxStack = 99;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.value = 500;
            item.createTile = ModContent.TileType<Riven_Lesion>();
        }
    }
}
