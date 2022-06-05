using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Defiled;
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
		public override void SetStaticDefaults() {
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
			if (player.HeldItem.hammer >= 45) {
				return true;
			}
			Projectile.NewProjectile(new Vector2((i + 1) * 16, (j + 1) * 16), Vector2.Zero, ModContent.ProjectileType<Projectiles.Misc.Defiled_Wastelands_Signal>(), 0, 0, ai0:0, ai1:Main.myPlayer);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
        
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            if (noBreak) {
                return true;
            }
            World.BiomeData.DefiledWastelands.CheckFissure(i, j, Type);
            return true;
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
            Item.width = 26;
            Item.height = 22;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 500;
            Item.createTile = ModContent.TileType<Defiled_Fissure>();
        }
    }
}
