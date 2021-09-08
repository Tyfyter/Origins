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

namespace Origins.Tiles.Riven {
    public class Riven_Dungeon_Chest : ModChest {
        public override void SetDefaults() {
            base.SetDefaults();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Riven Chest");
			AddMapEntry(new Color(200, 200, 200), name, MapChestName);
			name = CreateMapEntryName(Name + "_Locked"); // With multiple map entries, you need unique translation keys.
			name.SetDefault("Locked Riven Chest");
			AddMapEntry(new Color(140, 140, 140), name, MapChestName);
			disableSmartCursor = true;
			adjTiles = new int[] { TileID.Containers };
			chest = "Riven Chest";
			chestDrop = ModContent.ItemType<Riven_Dungeon_Chest_Item>();
            keyItem = ModContent.ItemType<Riven_Key>();
		}
    }
    public class Riven_Dungeon_Chest_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Chest");
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
            item.createTile = ModContent.TileType<Riven_Dungeon_Chest>();
        }
    }
}
