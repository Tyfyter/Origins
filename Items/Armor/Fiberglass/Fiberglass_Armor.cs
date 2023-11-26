using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Fiberglass {
    [AutoloadEquip(EquipType.Head)]
	public class Fiberglass_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 5;
			Item.value = Item.sellPrice(silver: 75);
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Fiberglass_Body>() && legs.type == ModContent.ItemType<Fiberglass_Legs>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Weapon damage increased by 4";
			player.GetDamage(DamageClass.Default).Flat += 4;
			player.GetDamage(DamageClass.Generic).Flat += 4;
			//player.GetModPlayer<OriginPlayer>().fiberglassSet = true;
			int inv = player.FindBuffIndex(BuffID.Invisibility);
			if (inv > -1) player.buffTime[inv]++;
		}
		public string ArmorSetName => "Fiberglass_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Fiberglass_Body>();
		public int LegsItemID => ModContent.ItemType<Fiberglass_Legs>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Fiberglass_Body : ModItem, INoSeperateWikiPage {
		public static int SlotID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			SlotID = Item.bodySlot;
		}
		public override void SetDefaults() {
			Item.defense = 6;
			Item.value = Item.sellPrice(silver: 60);
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Fiberglass_Legs : ModItem, INoSeperateWikiPage {
		public static int SlotID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			SlotID = Item.legSlot;
		}
		public override void SetDefaults() {
			Item.defense = 5;
			Item.value = Item.sellPrice(silver: 45);
		}
	}
}
