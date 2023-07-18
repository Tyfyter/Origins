using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Other {
	[AutoloadEquip(EquipType.Head)]
	public class Chlorophyte_Crown : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Chlorophyte Crown");
			// Tooltip.SetDefault("Increases summon damage by 12%\nIncreases artifact minion damage by 15%\nIncreases your max number of minions by 1");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 18;
			Item.defense = 2;
			Item.value = Item.buyPrice(gold: 6);
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
			player.setBonus = Language.GetTextValue("ArmorSetBonus.Chlorophyte") + ",\nand increases your max number of minions by 2";
			player.maxMinions += 2;
			player.AddBuff(BuffID.LeafCrystal, 18000);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}
