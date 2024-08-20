using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Other {
    [AutoloadEquip(EquipType.Head)]
	public class Chlorophyte_Crown : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "HardmodeArmorSet",
            "SummonBoostGear"
        ];
        public override void SetDefaults() {
			Item.width = 24;
			Item.height = 18;
			Item.defense = 2;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Lime;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.12f;
			player.GetModPlayer<OriginPlayer>().artifactDamage += 0.15f;
			player.maxMinions += 1;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ItemID.ChlorophytePlateMail && legs.type == ItemID.ChlorophyteGreaves;
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Chlorophyte") + "\n" + Language.GetTextValue("Mods.Origins.SetBonuses.Chlorohpyte_Summoner");
			player.maxMinions += 2;
			player.AddBuff(BuffID.LeafCrystal, 18000);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public string MergedArmorSetName => "Chlorophyte_Armor";
		public string ArmorSetName => Name;
		public int HeadItemID => Type;
		public int BodyItemID => ItemID.ChlorophytePlateMail;
		public int LegsItemID => ItemID.ChlorophyteGreaves;
		public IEnumerable<int> SharedPageItems {
			get {
				yield return ModContent.ItemType<Chlorophyte_Visage>();
			}
		}
	}
}
