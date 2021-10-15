using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;
using static Origins.OriginExtensions;

namespace Origins.Tiles.Defiled {
    public class Defiled_Fissure : ModTile {
        public static int ID { get; private set; }
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileHammer[Type] = true;
            Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 18, 18 };
			//TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Defiled Fissure");
			AddMapEntry(new Color(200, 200, 200), name);
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.ShadowOrbs };
            ID = Type;
		}

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
            Player player = Main.LocalPlayer;
			if (Main.hardMode && player.HeldItem.hammer >= 45) {

				return true;
			}

            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            if (Main.netMode == NetmodeID.MultiplayerClient || !Main.hardMode || WorldGen.noTileActions || WorldGen.gen){
		        return;
	        }
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = g = b = 0.3f;
        }
    }
    public class Defiled_Fissure_Item : ModItem {
        public override string Texture => "Origins/Tiles/Defiled/Defiled_Fissure";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Fissure (Debugging Item)");
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
            item.createTile = ModContent.TileType<Defiled_Fissure>();
        }
    }
}
