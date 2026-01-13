using Origins.Dev;
using Origins.Items.Armor;
using Origins.Items.Pets;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class Lost_Diver_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => "Lost_Diver_Vanity";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Lost_Diver_Chest>();
		public int LegsItemID => ModContent.ItemType<Lost_Diver_Greaves>();
		public bool HasFemaleVersion => false;
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Lost_Diver_Chest : ModItem, INoSeperateWikiPage {
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Lost_Diver_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 35);
		}
	}
}
