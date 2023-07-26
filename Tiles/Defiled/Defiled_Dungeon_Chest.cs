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

namespace Origins.Tiles.Defiled {
	public class Defiled_Dungeon_Chest : ModChest {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Defiled} Chest");
			AddMapEntry(new Color(200, 200, 200), name, MapChestName);
			name = Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{Name}_Locked.MapEntry"));
			// name.SetDefault("Locked {$Defiled} Chest");
			AddMapEntry(new Color(140, 140, 140), name, MapChestName);
			//disableSmartCursor = true;
			AdjTiles = new int[] { TileID.Containers };
			keyItem = ModContent.ItemType<Defiled_Key>();
		}

	}
	public class Defiled_Dungeon_Chest_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Chest");
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
			Item.createTile = ModContent.TileType<Defiled_Dungeon_Chest>();
		}
	}
	public class Locked_Defiled_Dungeon_Chest_Item : Defiled_Dungeon_Chest_Item {
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Dungeon_Chest_Item";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Defiled Chest (locked)");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.placeStyle = 1;
		}
	}
}
