using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Other {
    [AutoloadEquip(EquipType.Head)]
	public class Hallowed_Visage : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "ArmorSet",
            "ExplosiveBoostGear"
        ];
        public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Ancient_Hallowed_Visage>()] = ModContent.ItemType<Hallowed_Visage>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Hallowed_Visage>()] = ModContent.ItemType<Ancient_Hallowed_Visage>();
        }
        public override void SetDefaults() {
			Item.width = 22;
			Item.height = 26;
			Item.defense = 3;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveProjectileSpeed += 0.15f;
			player.GetCritChance(DamageClasses.Explosive) += 15;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ItemID.HallowedPlateMail && legs.type == ItemID.HallowedGreaves;
		}
		public override void ArmorSetShadows(Player player) {
			if (player.body is ArmorIDs.Body.HallowedPlateMail or ArmorIDs.Body.AncientHallowedPlateMail && player.legs is ArmorIDs.Legs.HallowedGreaves or ArmorIDs.Body.AncientHallowedPlateMail) {
				player.armorEffectDrawOutlines = true;
				player.armorEffectDrawShadow = true;
			}
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Hallowed");
			player.onHitDodge = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.HallowedBar, 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string MergedArmorSetName => "Explosive_Hallowed_Armor";
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.HallowedPlateMail;
		public int LegsItemID => ItemID.HallowedGreaves;
		public IEnumerable<int> SharedPageItems {
			get {
				yield return ModContent.ItemType<Ancient_Hallowed_Visage>();
			}
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Ancient_Hallowed_Visage : Hallowed_Visage, IWikiArmorSet, INoSeperateWikiPage {
		public new string ArmorSetName => Name;
		public new int HeadItemID => Type;
		public new int BodyItemID => ItemID.AncientHallowedPlateMail;
		public new int LegsItemID => ItemID.AncientHallowedGreaves;
		public bool SharedPageSecondary => true;
	}
}
