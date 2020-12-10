using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
    public class Defiled_Altar : ModTile {
        public static int id;
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileHammer[Type] = true;
            Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 18, 18 };
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Defiled Altar");
			AddMapEntry(new Color(200, 200, 200), name);
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.DemonAltar };
            id = Type;
		}

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
            Player player = Main.LocalPlayer;
            if(Main.hardMode&&player.HeldItem.hammer>=80)return true;
            player.Hurt(PlayerDeathReason.ByOther(4), player.statLife / 2, -player.direction);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            WorldGen.SmashAltar(i, j);
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = g = b = 0.5f;
        }
    }
}
