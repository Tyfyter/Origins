using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Defiled;
using Origins.World;
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
    public class Defiled_Heart : ModTile {
        public static int ID { get; private set; }
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileHammer[Type] = true;
            Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorWall = false;
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;
            TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
            TileObjectData.newTile.AnchorRight = AnchorData.Empty;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Defiled Heart");
			AddMapEntry(new Color(50, 50, 50), name);
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.ShadowOrbs };
            ID = Type;
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = g = b = 0.3f;
        }
        public override void PlaceInWorld(int i, int j, Item item) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {

            }
            ModContent.GetInstance<OriginWorld>().Defiled_Hearts.Add(new Point(i, j));
        }
    }
    public class Defiled_Heart_Item : ModItem {
        public override string Texture => "Origins/Tiles/Defiled/Defiled_Heart";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Heart (Debugging Item)");
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
            item.createTile = ModContent.TileType<Defiled_Heart>();
        }
    }
}
